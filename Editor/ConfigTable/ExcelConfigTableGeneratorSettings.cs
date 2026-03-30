using UnityEngine;

[CreateAssetMenu(fileName = "ConfigTableGeneratorSettings", menuName = "UniFramework/Config Table Generator/Settings")]
public class ExcelConfigTableGeneratorSettings : ScriptableObject
{
    [Header("Excel 原始表格文件夹 (输入)")]
    public string ExcelFolder = "Excel";

    [Header("生成的 C# 类输出路径")]
    public string ClassesOutputFolder = "Assets/Scripts/ConfigTable";

    [Header("生成的 ScriptableObject 输出路径")]
    public string AssetOutputFolder = "Assets/Resources/ConfigTable";
}