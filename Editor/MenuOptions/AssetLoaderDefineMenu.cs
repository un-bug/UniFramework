using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UniFramework.Editor
{
    public static class AssetLoaderDefineMenu
    {
        private const string Define = "ENABLE_ADDRESSABLES";
        private const string MenuRoot = "UniFramework/Asset Loader Mode/";
        private const string MenuResources = MenuRoot + "Resources";
        private const string MenuAddressables = MenuRoot + "Addressables";

        private static readonly BuildTargetGroup[] BuildTargetGroups = new BuildTargetGroup[]
        {
            BuildTargetGroup.Standalone,
            BuildTargetGroup.iOS,
            BuildTargetGroup.Android
        };

        [MenuItem(MenuResources, priority = 1)]
        private static void UseResources() => SetDefine(false);

        [MenuItem(MenuAddressables, priority = 2)]
        private static void UseAddressables() => SetDefine(true);

        [MenuItem(MenuResources, true)]
        private static bool ValidateResources()
        {
            Menu.SetChecked(MenuResources, !HasDefine());
            return true;
        }

        [MenuItem(MenuAddressables, true)]
        private static bool ValidateAddressables()
        {
            Menu.SetChecked(MenuAddressables, HasDefine());
            return true;
        }

        private static void SetDefine(bool enable)
        {
            foreach (BuildTargetGroup group in BuildTargetGroups)
            {
                if (group == BuildTargetGroup.Unknown)
                {
                    continue;
                }

                HashSet<string> symbols = new HashSet<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
                if (enable)
                {
                    symbols.Add(Define);
                }
                else
                {
                    symbols.Remove(Define);
                }

                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", symbols));
            }

            Debug.Log($"Asset Loader Mode => {(enable ? "Addressables" : "Resources")}");
            AssetDatabase.Refresh();
        }

        private static bool HasDefine()
        {
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').Contains(Define);
        }
    }
}