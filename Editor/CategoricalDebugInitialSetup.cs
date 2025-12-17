using System.Collections.Generic;
using UnityEditor;
using EyE.Diagnostics;

namespace EyE.EditorUnity
{
    public static class DefineSymbolUtility
    {
        public static void AddSymbol(string symbol)
        {
            BuildTargetGroup target = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            string definesStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
            string[] defines = definesStr.Split(';');

            for (int i = 0; i < defines.Length; i++)
            {
                if (defines[i] == symbol)
                    return; // already exists- skip add
            }

            string newDefines = string.IsNullOrEmpty(definesStr) ? symbol : definesStr + ";" + symbol;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(target, newDefines);
        }

        public static void RemoveSymbol(string symbol)
        {
            BuildTargetGroup target = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            string definesStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
            string[] defines = definesStr.Split(';');

            List<string> result = new List<string>();
            for (int i = 0; i < defines.Length; i++)
            {
                if (defines[i] != symbol && !string.IsNullOrEmpty(defines[i]))
                    result.Add(defines[i]);
            }

            string newDefines = string.Join(";", result.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(target, newDefines);
        }
    }

    [InitializeOnLoad]
    public static class CategoricalDebugInitialSetup
    {
        const string EditorPrefKey_OneTimePassDone = "CategoricalDebugDefineInitialized";

        static CategoricalDebugInitialSetup()
        {
            // Only run once
            if (EditorPrefs.GetBool(EditorPrefKey_OneTimePassDone, false))
                return;
            UnityEngine.Debug.Log("Categorical Debug one-time-pass:  Assigning build define symbol " + CatDebug.CONDITIONAL_DEFINE_STRING + " in player build options.  Can be removed manually in player settings, or via toggle in Edit->Categorical Debug Settings");
            DefineSymbolUtility.AddSymbol(CatDebug.CONDITIONAL_DEFINE_STRING);
            EditorPrefs.SetBool(EditorPrefKey_OneTimePassDone, true);
        }
    }
}