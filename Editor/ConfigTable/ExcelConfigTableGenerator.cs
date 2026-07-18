using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public sealed partial class ExcelConfigTableGenerator
{
    private const string DefaultSettingsPath = "Assets/Settings/ConfigTable/ConfigTableGeneratorSettings.asset";
    private static ConfigTableGeneratorSettings cachedSettings;

    public static ConfigTableGeneratorSettings Settings
    {
        get
        {
            if (cachedSettings != null)
            {
                return cachedSettings;
            }

            cachedSettings = AssetDatabase.LoadAssetAtPath<ConfigTableGeneratorSettings>(DefaultSettingsPath);
            if (cachedSettings != null)
            {
                return cachedSettings;
            }

            string[] guids = AssetDatabase.FindAssets($"t:{nameof(ConfigTableGeneratorSettings)}");
            if (guids.Length > 0)
            {
                Array.Sort(guids, StringComparer.Ordinal);
                string existingPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                cachedSettings = AssetDatabase.LoadAssetAtPath<ConfigTableGeneratorSettings>(existingPath);

                if (guids.Length > 1)
                {
                    Debug.LogError($"Multiple {nameof(ConfigTableGeneratorSettings)} assets were found. Using '{existingPath}'. Delete the duplicates so the generator has a single source of truth.", cachedSettings);
                }

                return cachedSettings;
            }

            string directory = Path.GetDirectoryName(DefaultSettingsPath)?.Replace("\\", "/");
            EnsureAssetFolderExists(directory);

            cachedSettings = ScriptableObject.CreateInstance<ConfigTableGeneratorSettings>();
            AssetDatabase.CreateAsset(cachedSettings, DefaultSettingsPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Created config table generator settings: {DefaultSettingsPath}", cachedSettings);
            return cachedSettings;
        }
    }

    private static void EnsureAssetFolderExists(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath) || AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string parent = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
        EnsureAssetFolderExists(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(folderPath));
    }

    private static string GetConfigClassName(string className)
    {
        return $"{className}ConfigTable";
    }

    private static string GetRowClassName(string className)
    {
        return $"DR{className}";
    }

    private static string GetCellAddress(int rowIndex, int columnIndex)
    {
        int columnNumber = columnIndex + 1;
        string columnName = string.Empty;
        while (columnNumber > 0)
        {
            columnNumber--;
            columnName = (char)('A' + columnNumber % 26) + columnName;
            columnNumber /= 26;
        }

        return $"{columnName}{rowIndex + 1}";
    }
}
