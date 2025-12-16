using UnityEngine;
using UnityEditor;
using EyE.Debug;

namespace EyE.EditorUnity
{

    public static class CatDebugGlobalOptionsEditor
    {
        static bool _loaded;

        static bool _addCategoryNameToLog;
        static bool _addCategoryNameToLogSingleLine;
        static bool _alwaysShowWarnings;
        static bool _logToFileIncludeStackTrace;

        static void LoadIfNeeded()
        {
            if (_loaded)
                return;

            _addCategoryNameToLog =
                CatDebugGlobalOptions.addCategoryNameToLog;
            _addCategoryNameToLogSingleLine =
                CatDebugGlobalOptions.addCategoryNameToLogSingleLine;
            _alwaysShowWarnings =
                CatDebugGlobalOptions.alwaysShowWarnings;
            _logToFileIncludeStackTrace =
                CatDebugGlobalOptions.logToFileIncludeStackTrace;

            _loaded = true;
        }

        public static void Draw()
        {
            LoadIfNeeded();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Enabled via Compiler Directive", "  Turing this setting off will disable all processing of logging and asserts, **including evaluating parameters**.\n  Recommend: turn off for Release build.\n  Note: Changing this setting may force rebuild or domain reload"), GUILayout.Width(250)); // Set an explicit width for the label
                bool defineSet = IsSymbolDefined(CatDebug.CONDITONAL_DEFINE_STRING);
                bool newDefineSet = EditorGUILayout.Toggle(defineSet, GUILayout.ExpandWidth(true));
                if (defineSet != newDefineSet)
                    UpdatePlayerBuildSettingsDefine(CatDebug.CONDITONAL_DEFINE_STRING, newDefineSet);
                EditorGUILayout.EndHorizontal();


                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Add Category Name To Log", "Enabling this option will automatically preprend the category name to any logs generate for that category"), GUILayout.Width(250)); // Set an explicit width for the label
                _addCategoryNameToLog = EditorGUILayout.Toggle(_addCategoryNameToLog, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Category Name Single Line", "Only applicable if Add Category Name To Log is enabled.  When disabled a new line will be generated after the category name."), GUILayout.Width(250));
                _addCategoryNameToLogSingleLine = EditorGUILayout.Toggle(_addCategoryNameToLogSingleLine, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Always Show Warnings", "When set to true, warnings will be displayed even if the category is disabled."), GUILayout.Width(250));
                _alwaysShowWarnings = EditorGUILayout.Toggle(_alwaysShowWarnings, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Log To File: Include Stack Trace", "Specifies weather of not stack traces should be written to log files"), GUILayout.Width(250));
                _logToFileIncludeStackTrace = EditorGUILayout.Toggle(_logToFileIncludeStackTrace, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            //GUILayout.FlexibleSpace();
            //EditorGUILayout.BeginHorizontal();
            //GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open Log File"))
                EditorUtility.OpenWithDefaultApp(CatDebug.catLogFilePath);
            //GUILayout.FlexibleSpace();
            //EditorGUILayout.EndHorizontal();
            //GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            if (!EditorGUI.EndChangeCheck())
                return;

            // Write only what changed
            CatDebugGlobalOptions.addCategoryNameToLog =
                _addCategoryNameToLog;
            CatDebugGlobalOptions.addCategoryNameToLogSingleLine =
                _addCategoryNameToLogSingleLine;
            CatDebugGlobalOptions.alwaysShowWarnings =
                _alwaysShowWarnings;
            CatDebugGlobalOptions.logToFileIncludeStackTrace =
                _logToFileIncludeStackTrace;

            PlayerPrefs.Save();
        }


        static void UpdatePlayerBuildSettingsDefine(string defineSymbol, bool enabled)
        {
            if(enabled)
                DefineSymbolUtility.AddSymbol(CatDebug.CONDITONAL_DEFINE_STRING);
            else
                DefineSymbolUtility.RemoveSymbol(CatDebug.CONDITONAL_DEFINE_STRING);

        }
        static bool IsSymbolDefined(string symbol, BuildTargetGroup group = BuildTargetGroup.Standalone)
        {
            if (string.IsNullOrEmpty(symbol))
                return false;

            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            string[] split = symbols.Split(';');
            for (int i = 0; i < split.Length; i++)
            {
                if (split[i].Trim() == symbol)
                    return true;
            }
            return false;
        }
    }


    /// <summary>
    /// This class provides a function that can be used to display the Various Debug Categories along with toggle controls to enable and disable each.
    /// These options are not automatically drawn so you will need to be called from an editor window.
    /// </summary>
    public static class CategoricalDebugOptionsGUI
    {
        static bool showIndex;


        private static Vector2 _scrollPos;

        private static readonly Color RowColorA = new Color(0.82f, 0.82f, 0.82f);
        private static readonly Color RowColorB = new Color(0.75f, 0.75f, 0.75f);
        private static readonly GUIStyle ScrollFrameStyle = new GUIStyle("box")
        {
            padding = new RectOffset(2, 2, 2, 2),
            margin = new RectOffset(0, 0, 4, 4)
        };
        /// <summary>
        /// This function displays the PlayerPrefOptions that define the behavior of CatDebug functions. Then displays them using EditorGUILayout Functions.
        /// </summary>
        public static void DisplayCatDebugPrefs()
        {

            CatDebugGlobalOptionsEditor.Draw();


            EditorGUILayout.Space();
            Rect r = EditorGUILayout.GetControlRect();
            r.height = 300;
            GUI.Box(r, "Registered Categories");
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos,GUILayout.Height(300) );
            
            int rowIndex = 0;

            foreach (int i in DebugCategoryRegistrar.registeredIDs)
            {
                PerCategoryDebugSettings categoryOptions =
                    DebugCategoryRegistrar.GetCategorySettings(i);

                if (categoryOptions == null)
                {
                    continue;
                }

                Color prevBg = GUI.backgroundColor;
                GUI.backgroundColor = (rowIndex & 1) == 0 ? RowColorA : RowColorB;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(categoryOptions.name, GUILayout.Width(300));
                EditorGUILayout.LabelField("ID: " + i.ToString(), GUILayout.Width(80));

                EditorGUILayout.BeginVertical();


                bool oldValue = categoryOptions.enableConsoleLogging;
                bool newValue = EditorGUILayout.Toggle("Console Logging Enabled", oldValue);
                if (newValue != oldValue)
                {
                    categoryOptions.enableConsoleLogging = newValue;
                    categoryOptions.Save();
                }

                oldValue = categoryOptions.enableFileLogging;
                newValue = EditorGUILayout.Toggle("File Logging Enabled", oldValue);
                if (newValue != oldValue)
                {
                    categoryOptions.enableFileLogging = newValue;
                    categoryOptions.Save();
                }

                oldValue = categoryOptions.enableAsserts;
                newValue = EditorGUILayout.Toggle("Asserts Enabled", oldValue);
                if (newValue != oldValue)
                {
                    categoryOptions.enableAsserts = newValue;
                    categoryOptions.Save();
                }
                


                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                GUI.backgroundColor = prevBg;
                rowIndex++;
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            if (GUILayout.Button(new GUIContent("Clear all save Debug Categories", "Registration of each category will require project be closed and re-opened.")))
            {
                DebugCategoryRegistrar.DeleteAllSavedCategoryKeysFromPlayerPrefs();
            }
            GUIStyle nonbreakingLabelStyle= new GUIStyle(GUI.skin.textArea);
            nonbreakingLabelStyle.wordWrap = true;
            
            //EditorGUILayout.LabelField("To add new Categories for Debug Log, you must invoke DebugCategoryRegistrar.RegisterCategory(string categoryName) in your code, before using the category.", nonbreakingLabelStyle);//,GUILayout.ExpandHeight(true));
        }// end DisplayCategoriesOnGUI

    }// end class

}//end namespace 
