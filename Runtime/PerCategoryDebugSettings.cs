using UnityEngine;

namespace EyE.Debug
{
    /// <summary>
    /// Per-Category settings, stored in playerprefs using a key based on the category ID.
    /// </summary>
    [System.Serializable]
    public class PerCategoryDebugSettings
    {
        /// <summary>
        /// user assigned name of the category
        /// </summary>
        public string name = "no category assigned";
        /// <summary>
        /// CategoryID integer: received from registrar when first registered.
        /// </summary>
        public int category;
        /// <summary>
        /// Main control: is this category enabled.
        /// </summary>
        public bool logEnabled = true;

        /// <summary>
        /// Logs to file only, but not console, if logEnabled is false and this is true.
        /// Must be set to false to COMPLETELY disable logging.
        /// </summary>
        public bool alwaysLogToFile = false;

        public static PerCategoryDebugSettings Default => new PerCategoryDebugSettings();
        private string Key => DebugCategoryRegistrar.KeyBaseForCatID(category);
    
        /// <summary>
        /// Stores current values to PlayerPrefs- creates entiresif needed.
        /// </summary>
        public void Save()
        {
            PlayerPrefs.SetString(NameKey(), name);
            PlayerPrefs.SetInt(EnabledKey(), logEnabled.ToInt());
            PlayerPrefs.SetInt(AlwaysLogToFilekey(), alwaysLogToFile.ToInt());
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Attempts to load the settings with the given catID from PlauerPrefs
        /// </summary>
        /// <param name="catID"></param>
        /// <returns>return the found settings, or null if not found</returns>
        public static PerCategoryDebugSettings Load(int catID)
        {
            PerCategoryDebugSettings newInst = new PerCategoryDebugSettings(){ category = catID };
            if (newInst.TryLoad())
                return newInst;
            return null;

        }
        

        /// <summary>
        /// uses CategoryID to Load settings from player Prefs.  If not found, the default value are assigned.
        /// Overwrites existing stored values (other than `int category`)
        /// </summary>
        public void Load()
        {
            if(!TryLoad())
            {
                UnityEngine.Debug.Log($"Category at key: {NameKey()} does not exist. Using defaults.");
                name = PerCategoryDebugSettings.Default.name;
                logEnabled = PerCategoryDebugSettings.Default.logEnabled;
                alwaysLogToFile = PerCategoryDebugSettings.Default.alwaysLogToFile;
            }
        }

        /// <summary>
        /// uses CategoryID to Load settings from player Prefs.  If not found, returns false, and does not alter any fields.
        /// </summary>
        /// <returns></returns>
        public bool TryLoad()
        {
            if (PlayerPrefs.HasKey(NameKey()))
            {
                name = PlayerPrefs.GetString(NameKey(), name);
                logEnabled = PlayerPrefs.GetInt(EnabledKey(), logEnabled.ToInt()).ToBool();
                alwaysLogToFile = PlayerPrefs.GetInt(AlwaysLogToFilekey(), alwaysLogToFile.ToInt()).ToBool(); 
                return true;
            }
            return false;
        }


        /// <summary>
        /// Deletes all PleyrPref entries for this CategoryID
        /// </summary>
        public void Delete()
        {
            if (PlayerPrefs.HasKey(NameKey()))
            {
                PlayerPrefs.DeleteKey(NameKey());
                PlayerPrefs.DeleteKey(EnabledKey());
                PlayerPrefs.DeleteKey(AlwaysLogToFilekey());
            }

        }

        public void SetToDefault()
        {
            name = PerCategoryDebugSettings.Default.name;
            logEnabled = PerCategoryDebugSettings.Default.logEnabled;
            alwaysLogToFile = PerCategoryDebugSettings.Default.alwaysLogToFile;
            Save();
        }

        private string NameKey() => Key + "/Name";
        private string EnabledKey() => Key + "/Enabled";
        private string AlwaysLogToFilekey() => Key + "/alwaysLogToFile";


    }//end CategoricalDebug namespace
}