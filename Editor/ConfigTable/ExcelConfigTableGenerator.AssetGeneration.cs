using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public sealed partial class ExcelConfigTableGenerator
{
    public static void GenerateAsset()
    {
        if (!Directory.Exists(Settings.ExcelFolder))
        {
            Debug.LogError($"Config table folder not found: {Settings.ExcelFolder}");
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var statistics = new AssetGenerationStatistics();
        int workbookCount = 0;
        string[] excelFiles = Directory.GetFiles(Settings.ExcelFolder, "*.xlsx", SearchOption.AllDirectories);
        foreach (string file in excelFiles)
        {
            if (file.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (Path.GetFileName(file).StartsWith("~$"))
            {
                continue;
            }

            workbookCount++;
            try
            {
                statistics.Add(GenerateAssetFile(file.Replace("\\", "/")));
            }
            catch (Exception ex)
            {
                statistics.FailedAssets++;
                Debug.LogError($"Failed to read workbook '{Path.GetFileName(file)}': {ex.Message}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        stopwatch.Stop();
        LogAssetGenerationSummary(statistics, workbookCount, stopwatch.ElapsedMilliseconds);
    }

    public static void GenerateAsset(string path)
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            Debug.LogWarning("Unity is compiling or refreshing assets. Config table assets cannot be generated yet.");
            return;
        }

        if (!Path.GetExtension(path).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning($"Skipped unsupported file: {path}");
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var statistics = new AssetGenerationStatistics();
        try
        {
            statistics.Add(GenerateAssetFile(path));
        }
        catch (Exception ex)
        {
            statistics.FailedAssets++;
            Debug.LogError($"Failed to read workbook '{Path.GetFileName(path)}': {ex.Message}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        stopwatch.Stop();
        LogAssetGenerationSummary(statistics, 1, stopwatch.ElapsedMilliseconds);
    }

    private static AssetGenerationStatistics GenerateAssetFile(string path)
    {
        var statistics = new AssetGenerationStatistics();
        string workbookName = Path.GetFileName(path);
        var sheets = XlsxWorkbookReader.Read(path);
        if (sheets.Count == 0)
        {
            statistics.FailedAssets++;
            Debug.LogError($"Workbook contains no readable worksheets: {workbookName}");
            return statistics;
        }

        for (int sheetIndex = 0; sheetIndex < sheets.Count; sheetIndex++)
        {
            var sheet = sheets[sheetIndex];
            if (Generate(workbookName, sheet, out int rowCount, out int skippedRowCount))
            {
                statistics.GeneratedAssets++;
                statistics.GeneratedRows += rowCount;
            }
            else
            {
                statistics.FailedAssets++;
            }

            statistics.SkippedRows += skippedRowCount;
        }

        return statistics;
    }

    private static bool Generate(string workbookName, XlsxSheetData sheet, out int rowCount, out int skippedRowCount)
    {
        rowCount = 0;
        skippedRowCount = 0;
        const int fieldRowIndex = 1;
        const int typeRowIndex = 2;
        if (!sheet.HasRow(fieldRowIndex) || !sheet.HasRow(typeRowIndex))
        {
            Debug.LogError($"Invalid worksheet format: {workbookName} / {sheet.Name}. The field name row or field type row is missing.");
            return false;
        }

        string className = Path.GetFileNameWithoutExtension(sheet.Name);
        string outputDir = Settings.AssetOutputFolder;
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        string assetPath = Path.Combine(outputDir, $"{className}.asset").Replace("\\", "/");
        ScriptableObject soObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
        bool createAsset = soObject == null;
        if (soObject == null)
        {
            soObject = ScriptableObject.CreateInstance(GetConfigClassName(className));
            if (soObject == null)
            {
                Debug.LogError($"Config table type not found: {GetConfigClassName(className)}. Generate the config table classes and wait for Unity to finish compiling.");
                return false;
            }
        }

        int firstDataRow = 4;
        var dataList = new List<object>();

        Type dataType = null;
        foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            dataType = asm.GetType(GetRowClassName(className));
            if (dataType != null)
            {
                break;
            }
        }

        if (dataType == null)
        {
            Debug.LogError($"Config table row type not found: {GetRowClassName(className)}. Source: {workbookName} / {sheet.Name}.");
            return false;
        }

        int colCount = sheet.LastColumnIndex + 1;
        var fieldNames = new string[colCount];
        var fieldTypes = new string[colCount];

        for (int i = 1; i < colCount; i++)
        {
            fieldNames[i] = sheet.GetCell(fieldRowIndex, i).Trim();
            fieldTypes[i] = sheet.GetCell(typeRowIndex, i).Trim();
        }

        // 缓存字段反射信息，避免重复查找
        var fieldCache = dataType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).ToDictionary(f => f.Name, f => f);
        bool hasErrors = false;

        for (int r = firstDataRow; r <= sheet.LastRowIndex; r++)
        {
            if (IsRowEmpty(sheet, r))
            {
                skippedRowCount++;
                continue;
            }

#if UNITY_EDITOR && false
            // 打印整行内容（调试用）
            var sb = new StringBuilder($"Row {r}: ");
            for (int c2 = 0; c2 <= sheet.LastColumnIndex; c2++)
            {
                sb.Append($"[{c2}]={sheet.GetCell(r, c2)}, ");
            }
            Debug.Log(sb.ToString());
#endif

            var dataObj = Activator.CreateInstance(dataType);

            int c = 1;
            while (c < colCount)
            {
                var fieldName = fieldNames[c];
                var fieldType = fieldTypes[c];

                if (string.IsNullOrEmpty(fieldName))
                {
                    c++;
                    continue;
                }

                if (!fieldCache.TryGetValue(string.Format("m_{0}", fieldName), out var field))
                {
                    hasErrors = true;
                    Debug.LogError($"Generated type '{dataType.Name}' does not contain field '{fieldName}'. Source: {workbookName} / {sheet.Name} / {GetCellAddress(fieldRowIndex, c)}.");
                    c++;
                    continue;
                }

                if (fieldType == "int[]")
                {
                    var intList = new List<int>();
                    int startCol = c;
                    while (startCol < colCount && (string.IsNullOrEmpty(fieldTypes[startCol]) || fieldTypes[startCol] == fieldType) && fieldNames[startCol] == fieldName)
                    {
                        string cell = sheet.GetCell(r, startCol);
                        int value = 0;
                        try
                        {
                            if (string.IsNullOrWhiteSpace(cell))
                            {
                            }
                            else
                            {
                                value = Convert.ToInt32(GetCellValue(cell, "int"));
                                intList.Add(value);
                            }
                        }
                        catch (Exception ex)
                        {
                            hasErrors = true;
                            Debug.LogError($"Failed to parse {workbookName} / {sheet.Name} / {GetCellAddress(r, startCol)}: value '{cell}' cannot be converted to int for field '{fieldName}'. {ex.Message}");
                        }

                        startCol++;
                    }

                    field.SetValue(dataObj, intList.ToArray());
                    c = startCol;
                }
                else
                {
                    string cell = sheet.GetCell(r, c);
                    object value = null;
                    bool parseSucceeded = true;

                    try
                    {
                        value = GetCellValue(cell, fieldType);
                    }
                    catch (Exception ex)
                    {
                        parseSucceeded = false;
                        hasErrors = true;
                        Debug.LogError($"Failed to parse {workbookName} / {sheet.Name} / {GetCellAddress(r, c)}: value '{cell}' cannot be converted to {fieldType} for field '{fieldName}'. {ex.Message}");
                    }

                    if (value != null)
                    {
                        field.SetValue(dataObj, value);
                    }
                    else if (parseSucceeded)
                    {
                        hasErrors = true;
                        Debug.LogError($"Unsupported field type '{fieldType}' at {workbookName} / {sheet.Name} / {GetCellAddress(typeRowIndex, c)} for field '{fieldName}'.");
                    }

                    c++;
                }
            }

            dataList.Add(dataObj);
        }

        if (hasErrors)
        {
            Debug.LogError($"Failed to generate config table asset: {workbookName} / {sheet.Name}. The existing asset was not modified.");
            return false;
        }

        var mDataField = soObject.GetType().GetField("Data");
        if (mDataField == null)
        {
            Debug.LogError($"Config table type '{soObject.GetType().Name}' does not contain the Data field. Source: {workbookName} / {sheet.Name}.");
            return false;
        }

        var typedList = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(dataType));
        for (int i = 0; i < dataList.Count; i++)
        {
            typedList.Add(dataList[i]);
        }

        mDataField.SetValue(soObject, typedList);
        if (createAsset)
        {
            AssetDatabase.CreateAsset(soObject, assetPath);
        }

        EditorUtility.SetDirty(soObject);
        rowCount = dataList.Count;
        Debug.Log($"Generated config table asset: {assetPath}, {rowCount} row(s).", soObject);
        return true;
    }

    private static void LogAssetGenerationSummary(AssetGenerationStatistics statistics, int workbookCount, long elapsedMilliseconds)
    {
        if (workbookCount == 0)
        {
            Debug.LogWarning($"No .xlsx files found in config table folder: {Settings.ExcelFolder}");
        }
        else if (statistics.FailedAssets == 0)
        {
            Debug.Log($"Config table asset generation succeeded: processed {workbookCount} workbook(s), generated {statistics.GeneratedAssets} asset(s) with {statistics.GeneratedRows} row(s), skipped {statistics.SkippedRows} row(s), elapsed {elapsedMilliseconds} ms.");
        }
        else
        {
            Debug.LogError($"Config table asset generation completed with errors: {statistics.GeneratedAssets} succeeded, {statistics.FailedAssets} failed, skipped {statistics.SkippedRows} row(s), elapsed {elapsedMilliseconds} ms.");
        }
    }

    private sealed class AssetGenerationStatistics
    {
        public int GeneratedAssets;
        public int FailedAssets;
        public int GeneratedRows;
        public int SkippedRows;

        public void Add(AssetGenerationStatistics other)
        {
            GeneratedAssets += other.GeneratedAssets;
            FailedAssets += other.FailedAssets;
            GeneratedRows += other.GeneratedRows;
            SkippedRows += other.SkippedRows;
        }
    }

    private static object GetCellValue(string cell, string type)
    {
        cell = cell?.Trim() ?? string.Empty;
        switch (type)
        {
            case "int":
                return string.IsNullOrEmpty(cell) ? 0 : Convert.ToInt32(double.Parse(cell, CultureInfo.InvariantCulture));

            case "float":
                return string.IsNullOrEmpty(cell) ? 0f : float.Parse(cell, CultureInfo.InvariantCulture);

            case "string":
                return cell;

            case "bool":
                if (string.IsNullOrEmpty(cell))
                {
                    return false;
                }

                if (bool.TryParse(cell, out bool boolValue))
                {
                    return boolValue;
                }

                return double.Parse(cell, CultureInfo.InvariantCulture) != 0d;

            default:
                return null;
        }
    }

    private static bool IsRowEmpty(XlsxSheetData sheet, int rowIndex)
    {
        if (!sheet.HasRow(rowIndex))
        {
            return true;
        }

        for (int i = 0; i <= sheet.LastColumnIndex; i++)
        {
            string cell = sheet.GetCell(rowIndex, i);
            if (i == 0 && cell.Equals("#"))
            {
                return true;
            }

            // 如果单元格有任何非空内容就不是空行。
            if (!string.IsNullOrWhiteSpace(cell))
            {
                return false;
            }
        }

        return true;
    }
}
