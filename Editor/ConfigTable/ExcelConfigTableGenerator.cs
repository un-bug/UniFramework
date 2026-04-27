using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public sealed partial class ExcelConfigTableGenerator
{
    public static ExcelConfigTableGeneratorSettings Settings
    {
        get
        {
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", nameof(ExcelConfigTableGeneratorSettings)));
            if (guids.Length == 0)
            {
                return ScriptableObject.CreateInstance<ExcelConfigTableGeneratorSettings>();
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<ExcelConfigTableGeneratorSettings>(path);
        }
    }

    [MenuItem("UniFramework/Config Table/Generate Classes", false, 20)]
    private static void GenerateClass()
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

            GenerateClass(file);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Excel class generation finished.");
    }

    private static void GenerateClass(string path)
    {
        if (!Path.GetExtension(path).Equals(".xlsx", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning($"Unsupported file extension: {path}");
            return;
        }

        string fileName = Path.GetFileName(path);
        List<XlsxSheetData> sheets = XlsxWorkbookReader.Read(path);
        for (int sheetIndex = 0; sheetIndex < sheets.Count; sheetIndex++)
        {
            var sheet = sheets[sheetIndex];
            GenerateClassFile(fileName, sheet.Name, sheet);
        }
    }

    private static void GenerateClassFile(string fileName, string sheetName, XlsxSheetData sheet)
    {
        const int fieldRowIndex = 1; // 字段名。
        const int typeRowIndex = 2;  // 类型。
        const int noteRowIndex = 3;  // 备注。

        if (!sheet.HasRow(fieldRowIndex) || !sheet.HasRow(typeRowIndex))
        {
            Debug.LogError($"Invalid format in: {sheetName}");
            return;
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
        sb.AppendLine($" * 生成时间: {System.DateTime.Now.ToString("yyyy-MM-dd")}");
        sb.AppendLine(" * ===========================================================");
        sb.AppendLine(" */");
        sb.AppendLine();
        sb.AppendLine($"[ConfigTableAttribute(\"{className}\")]");
        sb.AppendLine("[System.Serializable]");
        sb.AppendLine($"public class {rowClassName} : {nameof(ConfigTableRow)}");
        sb.AppendLine("{");

        int colCount = sheet.LastColumnIndex + 1;
        for (int i = 1; i < colCount; i++)
        {
            string fieldName = sheet.GetCell(fieldRowIndex, i).Trim();
            string fieldType = sheet.GetCell(typeRowIndex, i).Trim();
            string fieldNote = sheet.GetCell(noteRowIndex, i).Trim();

            if (string.IsNullOrEmpty(fieldName) || string.IsNullOrEmpty(fieldType))
            {
                continue;
            }

            if (i == 1)//fieldType == "int" && fieldName == "Id")
            {
                sb.AppendLine($"    public override {fieldType} ID => m_{fieldName};");
                sb.AppendLine();
            }

            sb.AppendLine($"    [UnityEngine.Header(\"{fieldNote}\")]");
            sb.AppendLine($"    [UnityEngine.SerializeField] {fieldType} m_{fieldName};");
            sb.AppendLine();
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// {fieldNote}。");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    public {fieldType} {fieldName} => m_{fieldName};");
            sb.AppendLine();
        }

        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine($"public class {configClassName} : ConfigTable<{rowClassName}>");
        sb.AppendLine("{");
        sb.AppendLine("}");
        if (!Directory.Exists(Settings.ClassesOutputFolder))
        {
            Directory.CreateDirectory(Settings.ClassesOutputFolder);
        }

        string filePath = Path.Combine(Settings.ClassesOutputFolder, $"{configClassName}.cs").Replace("\\", "/");
        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        Debug.Log($"Excel class generation: {filePath}");
    }

    private static string GetConfigClassName(string className)
    {
        return $"{className}ConfigTable";
    }

    private static string GetRowClassName(string className)
    {
        return $"DR{className}";
    }
}
