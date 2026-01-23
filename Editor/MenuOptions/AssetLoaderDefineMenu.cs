using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UniFramework.Editor
{
    public static class AssetLoaderDefineMenu
    {
        private const string DefineSymbol = "ENABLE_ADDRESSABLES";
        private const string MenuRoot = "UniFramework/Asset Loader Mode/";

        private static readonly BuildTargetGroup[] TargetGroups = new BuildTargetGroup[]
        {
            BuildTargetGroup.Standalone,
            BuildTargetGroup.iOS,
            BuildTargetGroup.Android
        };

        [MenuItem(MenuRoot + "Resources", priority = 1)]
        private static void SwitchToResources() => UpdateDefines(false);

        [MenuItem(MenuRoot + "Addressables", priority = 2)]
        private static void SwitchToAddressables() => UpdateDefines(true);

        [MenuItem(MenuRoot + "Resources", true)]
        private static bool ValidateResources()
        {
            Menu.SetChecked(MenuRoot + "Resources", !IsAddressablesEnabled());
            return true;
        }

        [MenuItem(MenuRoot + "Addressables", true)]
        private static bool ValidateAddressables()
        {
            Menu.SetChecked(MenuRoot + "Addressables", IsAddressablesEnabled());
            return true;
        }

        private static void UpdateDefines(bool enableAddressables)
        {
            bool changed = false;

            foreach (var group in TargetGroups)
            {
                if (group == BuildTargetGroup.Unknown) continue;

                string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                var symbols = new HashSet<string>(currentDefines.Split(';').Where(s => !string.IsNullOrWhiteSpace(s)));

                if (enableAddressables)
                {
                    if (symbols.Add(DefineSymbol)) changed = true;
                }
                else
                {
                    if (symbols.Remove(DefineSymbol)) changed = true;
                }

                if (changed)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", symbols));
                }
            }

            if (changed)
            {
                Debug.Log($"[AssetLoader] Switched to: {(enableAddressables ? "Addressables" : "Resources")}");
            }
            else
            {
                Debug.Log($"[AssetLoader] Already in {(enableAddressables ? "Addressables" : "Resources")} mode.");
            }
        }

        private static bool IsAddressablesEnabled()
        {
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            return symbols.Split(';').Contains(DefineSymbol);
        }
    }
}