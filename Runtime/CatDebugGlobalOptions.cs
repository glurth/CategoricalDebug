using UnityEngine;
namespace EyE.Diagnostics
{
    /// \ingroup CatDebug
    /// <summary>
    /// This class initializes and stores PlayerPrefs, which are persistent between sessions, that define how CatDebug will function.
    /// </summary>

    public static class CatDebugGlobalOptions
    {
        public static bool addCategoryNameToLog
        {
            get => PlayerPrefs.GetInt("CatDebug_AddCategoryNameToLog", 0) != 0;
            set => PlayerPrefs.SetInt("CatDebug_AddCategoryNameToLog", value ? 1 : 0);
        }

        public static bool addCategoryNameToLogSingleLine
        {
            get => PlayerPrefs.GetInt("CatDebug_AddCategoryNameToLogSingleLine", 0) != 0;
            set => PlayerPrefs.SetInt("CatDebug_AddCategoryNameToLogSingleLine", value ? 1 : 0);
        }

        public static bool alwaysShowWarnings
        {
            get => PlayerPrefs.GetInt("CatDebug_AlwaysShowWarnings", 0) != 0;
            set => PlayerPrefs.SetInt("CatDebug_AlwaysShowWarnings", value ? 1 : 0);
        }
        
        public static bool logToFileIncludeStackTrace
        {
            get => PlayerPrefs.GetInt("CatDebug_LogToFileIncludeStackTrace", 0) != 0;
            set => PlayerPrefs.SetInt("CatDebug_LogToFileIncludeStackTrace", value ? 1 : 0);
        }
    }
}