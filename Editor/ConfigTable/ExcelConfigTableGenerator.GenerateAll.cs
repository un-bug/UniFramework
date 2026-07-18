using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public sealed partial class ExcelConfigTableGenerator
{
    private const string GenerateAllPendingKey = "ExcelConfigTableGenerator.GenerateAllPending";

    static ExcelConfigTableGenerator()
    {
        if (SessionState.GetBool(GenerateAllPendingKey, false))
        {
            EditorApplication.delayCall += ResumeGenerateAll;
        }
    }

    [MenuItem("UniFramework/Config Table/Generate Config Table", false, 19)]
    public static void GenerateConfigTable()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            Debug.LogWarning("Unity is compiling or refreshing assets. Config table generation cannot start yet.");
            return;
        }

        if (!Directory.Exists(Settings.ExcelFolder))
        {
            Debug.LogError($"Config table folder not found: {Settings.ExcelFolder}");
            return;
        }

        SessionState.SetBool(GenerateAllPendingKey, true);
        Debug.Log("Config table generation started. Generating classes first.");
        GenerateClass();
        EditorApplication.delayCall += ResumeGenerateAll;
    }

    [MenuItem("UniFramework/Config Table/Generate Config Table", true)]
    private static bool ValidateGenerateConfigTable()
    {
        return !EditorApplication.isCompiling && !EditorApplication.isUpdating && !SessionState.GetBool(GenerateAllPendingKey, false);
    }

    private static void ResumeGenerateAll()
    {
        if (!SessionState.GetBool(GenerateAllPendingKey, false))
        {
            return;
        }

        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += ResumeGenerateAll;
            return;
        }

        SessionState.EraseBool(GenerateAllPendingKey);
        Debug.Log("Config table scripts are ready. Continuing with asset generation.");
        GenerateAsset();
    }
}
