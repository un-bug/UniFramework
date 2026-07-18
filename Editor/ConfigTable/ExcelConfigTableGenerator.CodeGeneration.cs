using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public sealed partial class ExcelConfigTableGenerator
{
    private static void GenerateClass()
    {
        if (!Directory.Exists(Settings.ExcelFolder))
        {
            Debug.LogError($"Config table folder not found: {Settings.ExcelFolder}");
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        int workbookCount = 0;
        int generatedCount = 0;
        int failedCount = 0;
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
                GenerateClass(file, ref generatedCount, ref failedCount);
            }
            catch (System.Exception ex)
            {
                failedCount++;
                Debug.LogError($"Failed to read workbook '{Path.GetFileName(file)}': {ex.Message}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        stopwatch.Stop();

        if (workbookCount == 0)
        {
            Debug.LogWarning($"No .xlsx files found in config table folder: {Settings.ExcelFolder}");
        }
        else if (failedCount == 0)
        {
            Debug.Log($"Config table class generation succeeded: processed {workbookCount} workbook(s), generated {generatedCount} file(s), elapsed {stopwatch.ElapsedMilliseconds} ms.");
        }
        else
        {
            Debug.LogError($"Config table class generation completed with errors: {generatedCount} succeeded, {failedCount} failed, elapsed {stopwatch.ElapsedMilliseconds} ms.");
        }
    }

    private static void GenerateClass(string path, ref int generatedCount, ref int failedCount)
    {
        if (!Path.GetExtension(path).Equals(".xlsx", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning($"Skipped unsupported file: {path}");
            return;
        }

        string fileName = Path.GetFileName(path);
        List<XlsxSheetData> sheets = XlsxWorkbookReader.Read(path);
        for (int sheetIndex = 0; sheetIndex < sheets.Count; sheetIndex++)
        {
            var sheet = sheets[sheetIndex];
            if (GenerateClassFile(fileName, sheet.Name, sheet))
            {
                generatedCount++;
            }
            else
            {
                failedCount++;
            }
        }
    }

    private static bool GenerateClassFile(string fileName, string sheetName, XlsxSheetData sheet)
    {
        const int fieldRowIndex = 1; // 字段名。
        const int typeRowIndex = 2;  // 类型。
        const int noteRowIndex = 3;  // 备注。

        if (!sheet.HasRow(fieldRowIndex) || !sheet.HasRow(typeRowIndex))
        {
            Debug.LogError($"Invalid worksheet format: {fileName} / {sheetName}. The field name row or field type row is missing.");
            return false;
        }

        string className = Path.GetFileNameWithoutExtension(sheetName);
        string rowClassName = GetRowClassName(className);
        string configClassName = GetConfigClassName(className);
        var sb = new StringBuilder();
        sb.AppendLine("/*");
        sb.AppendLine(" * ===========================================================");
        sb.AppendLine(" * 本文件由表格导出工具自动生成，请勿手动修改。");
        sb.AppendLine(" * 如需修改，请在对应的 Excel 表格中修改后重新生成。");
        sb.AppendLine(" * ");
        sb.AppendLine($" * 源文件: {fileName}");
        sb.AppendLine(" * ===========================================================");
        sb.AppendLine(" */");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine();
        sb.AppendLine("[Serializable]");
        sb.AppendLine($"public class {rowClassName} : {nameof(ConfigTableRow)}");
        sb.AppendLine("{");

        int colCount = sheet.LastColumnIndex + 1;
        for (int i = 1; i < colCount; i++)
        {
            string fieldName = sheet.GetCell(fieldRowIndex, i).Trim();
            string fieldType = sheet.GetCell(typeRowIndex, i).Trim();
            string fieldNote = sheet.GetCell(noteRowIndex, i).Trim();

            if (string.IsNullOrWhiteSpace(fieldName) || string.IsNullOrWhiteSpace(fieldType))
            {
                continue;
            }

            if (i == 1 && fieldType != "int")
            {
                Debug.LogError($"Invalid field type at {fileName} / {sheetName} / {GetCellAddress(typeRowIndex, i)}: the Id field must be declared as int, but found '{fieldType}'.");
                return false;
            }

            string privateFieldName = $"m_{fieldName}";

            if (!string.IsNullOrEmpty(fieldNote))
            {
                sb.AppendLine($"    [Header(\"{fieldNote}\")]");
            }

            sb.AppendLine($"    [SerializeField] private {fieldType} {privateFieldName};");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(fieldNote))
            {
                sb.AppendLine($"    /// <summary>");
                sb.AppendLine($"    /// {fieldNote}。");
                sb.AppendLine($"    /// </summary>");
            }

            if (i == 1) // Id 字段特殊处理。
            {
                sb.AppendLine($"    public override {fieldType} Id => {privateFieldName};");
            }
            else
            {
                sb.AppendLine($"    public {fieldType} {fieldName} => {privateFieldName};");
            }
            
            sb.AppendLine();
        }

        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine($"public partial class {configClassName} : ConfigTable<{rowClassName}>");
        sb.AppendLine("{");
        sb.AppendLine("}");
        if (!Directory.Exists(Settings.ClassesOutputFolder))
        {
            Directory.CreateDirectory(Settings.ClassesOutputFolder);
        }

        string filePath = Path.Combine(Settings.ClassesOutputFolder, $"{configClassName}.cs").Replace("\\", "/");
        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        Debug.Log($"Generated config table class: {filePath}");
        return true;
    }

}
