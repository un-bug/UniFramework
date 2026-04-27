using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed partial class ExcelConfigTableGenerator
{
    [MenuItem("UniFramework/Config Table/Generate Assets", false, 21)]
    public static void GenerateAsset()
    {
        if (!Directory.Exists(Settings.ExcelFolder))
        {
            Debug.LogError($"Excel folder not found: {Settings.ExcelFolder}");
            return;
        }

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

            GenerateAsset(file.Replace("\\", "/"));
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Excel asset generation finished.");
    }

    public static void GenerateAsset(string path)
    {
        if (!Path.GetExtension(path).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning($"Unsupported file extension: {path}");
            return;
        }

        var sheets = XlsxWorkbookReader.Read(path);
        for (int sheetIndex = 0; sheetIndex < sheets.Count; sheetIndex++)
        {
            var sheet = sheets[sheetIndex];
            Generate(sheet.Name, sheet);
        }
    }

    private static void Generate(string name, XlsxSheetData sheet)
    {
        const int fieldRowIndex = 1;
        const int typeRowIndex = 2;
        if (!sheet.HasRow(fieldRowIndex) || !sheet.HasRow(typeRowIndex))
        {
            return;
        }

        string className = Path.GetFileNameWithoutExtension(name);
        string outputDir = Settings.AssetOutputFolder;
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        string assetPath = Path.Combine(outputDir, $"{className}.asset").Replace("\\", "/");
        ScriptableObject soObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
        if (soObject == null)
        {
            soObject = ScriptableObject.CreateInstance(GetConfigClassName(className));
            if (soObject == null)
            {
                Debug.LogWarning($"Class not found: {className}");
                return;
            }

            AssetDatabase.CreateAsset(soObject, assetPath);
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
            Debug.LogError($"Data type not found: {GetRowClassName(className)}");
            return;
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

        for (int r = firstDataRow; r <= sheet.LastRowIndex; r++)
        {
            if (IsRowEmpty(sheet, r))
            {
                //Debug.Log($"[Excel] Skip empty row {r}");
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
                    Debug.LogWarning($"[Excel] Field '{fieldName}' not found in type {dataType.Name}");
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
                                //Debug.Log($"[Excel] ({r},{startCol}) cell is empty or blank.");
                            }
                            else
                            {
                                value = Convert.ToInt32(GetCellValue(cell, "int"));
                                intList.Add(value);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[Excel] '{fieldName}' Parse int[] cell ({r},{startCol}) failed: {ex.Message}");
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

                    try
                    {
                        value = GetCellValue(cell, fieldType);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[Excel] '{fieldName}' Parse cell ({r},{c}) '{fieldName}' failed: {ex.Message}");
                    }

                    if (value != null)
                    {
                        field.SetValue(dataObj, value);
                    }

                    c++;
                }
            }

            dataList.Add(dataObj);
        }

        var mDataField = soObject.GetType().GetField("m_Data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (mDataField != null)
        {
            var typedList = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(dataType));
            for (int i = 0; i < dataList.Count; i++)
            {
                typedList.Add(dataList[i]);
            }

            mDataField.SetValue(soObject, typedList);
        }

        EditorUtility.SetDirty(soObject);
        AssetDatabase.SaveAssets();
        Debug.Log($"Excel asset generation {assetPath}");
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
