using UnityEngine;

[CreateAssetMenu(fileName = "ConfigTableGeneratorSettings", menuName = "Config Table Generator/Settings")]
public class ConfigTableGeneratorSettings : ScriptableObject
{
    public string ExcelFolder = "Excel";
    public string ClassesOutputFolder = "Assets/Scripts/ConfigTable";
    public string AssetOutputFolder = "Assets/Resources/ConfigTable";
}