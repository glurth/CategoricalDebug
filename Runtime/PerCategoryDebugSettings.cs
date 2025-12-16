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
        /// Enables logging to the Unity console.
        /// </summary>
        public bool enableConsoleLogging = true;

        /// <summary>
        /// Enables logging to file.
        /// </summary>
        public bool enableFileLogging = false;

        /// <summary>
        /// Enables asserts for this category.
        /// </summary>
        public bool enableAsserts = true;

        public static PerCategoryDebugSettings Default => new PerCategoryDebugSettings();

        private string Key => DebugCategoryRegistrar.KeyBaseForCatID(category);

        /// <summary>
        /// Stores current values to PlayerPrefs – creates entries if needed.
        /// </summary>
        public void Save()
        {
            PlayerPrefs.SetString(NameKey(), name);
            PlayerPrefs.SetInt(ConsoleLoggingKey(), enableConsoleLogging.ToInt());
            PlayerPrefs.SetInt(FileLoggingKey(), enableFileLogging.ToInt());
            PlayerPrefs.SetInt(AssertsKey(), enableAsserts.ToInt());
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Attempts to load the settings with the given catID from PlayerPrefs
        /// </summary>
        /// <param name="catID"></param>
        /// <returns>return the found settings, or null if not found</returns>
        public static PerCategoryDebugSettings Load(int catID)
        {
            PerCategoryDebugSettings newInst = new PerCategoryDebugSettings() { category = catID };
            if (newInst.TryLoad())
                return newInst;

            return null;
        }

        /// <summary>
        /// uses CategoryID to Load settings from PlayerPrefs.
        /// If not found, the default values are assigned.
        /// Overwrites existing stored values (other than `int category`)
        /// </summary>
        public void Load()
        {
            if (!TryLoad())
            {
                UnityEngine.Debug.Log($"Category at key: {NameKey()} does not exist. Using defaults.");
                name = Default.name;
                enableConsoleLogging = Default.enableConsoleLogging;
                enableFileLogging = Default.enableFileLogging;
                enableAsserts = Default.enableAsserts;
            }
        }

        /// <summary>
        /// uses CategoryID to Load settings from PlayerPrefs.
        /// If not found, returns false, and does not alter any fields.
        /// </summary>
        public bool TryLoad()
        {
            if (PlayerPrefs.HasKey(NameKey()))
            {
                name = PlayerPrefs.GetString(NameKey(), name);
                enableConsoleLogging = PlayerPrefs.GetInt(ConsoleLoggingKey(), enableConsoleLogging.ToInt()).ToBool();
                enableFileLogging = PlayerPrefs.GetInt(FileLoggingKey(), enableFileLogging.ToInt()).ToBool();
                enableAsserts = PlayerPrefs.GetInt(AssertsKey(), enableAsserts.ToInt()).ToBool();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deletes all PlayerPrefs entries for this CategoryID
        /// </summary>
        public void Delete()
        {
            if (PlayerPrefs.HasKey(NameKey()))
            {
                PlayerPrefs.DeleteKey(NameKey());
                PlayerPrefs.DeleteKey(ConsoleLoggingKey());
                PlayerPrefs.DeleteKey(FileLoggingKey());
                PlayerPrefs.DeleteKey(AssertsKey());
            }
        }

        public void SetToDefault()
        {
            name = Default.name;
            enableConsoleLogging = Default.enableConsoleLogging;
            enableFileLogging = Default.enableFileLogging;
            enableAsserts = Default.enableAsserts;
            Save();
        }

        private string NameKey() => Key + "/Name";
        private string ConsoleLoggingKey() => Key + "/EnableConsoleLogging";
        private string FileLoggingKey() => Key + "/EnableFileLogging";
        private string AssertsKey() => Key + "/EnableAsserts";
    }

}