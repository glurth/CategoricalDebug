using UnityEngine;

using System.Collections.Generic;

namespace EyE.Unity.CategoricalDebug
{
    static public class PlayerPrefUtil
    {

        static public bool ToBool(this int i) { return (i != 0); }
        static public int ToInt(this bool b)
        {
            if (b) return 1;
            return 0;
        }
    }

    /// <summary>
    /// Per-Category settings, stored in playerprefs using a key based on the category ID.
    /// </summary>
    [System.Serializable]
    public class DebugCategorySettings
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

        public static DebugCategorySettings Default => new DebugCategorySettings();
        private string Key => DebugCategoryRegistrar.KeyBaseForCatID(category);
    
        public void Save()
        {
            PlayerPrefs.SetString(NameKey(), name);
            PlayerPrefs.SetInt(EnabledKey(), logEnabled ? 1 : 0);
            PlayerPrefs.SetInt(AlwaysLogToFilekey(), alwaysLogToFile ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Attempts to load the settings with the given catID from PlauerPrefs
        /// </summary>
        /// <param name="catID"></param>
        /// <returns>return the found settings, or null if not found</returns>
        public static DebugCategorySettings Load(int catID)
        {
            DebugCategorySettings newInst = new DebugCategorySettings(){ category = catID };
            if (newInst.TryLoad())
                return newInst;
            return null;

        }
        //uses category field, to load an potential overwrite all other fields.
        /// <summary>
        /// uses CategoryID to Load settings from player Prefs.  If not found, the default value are assigned.
        /// Overwrites existing stored values (other than `int category`)
        /// </summary>
        public void Load()
        {
            if(!TryLoad())
            {
                UnityEngine.Debug.Log($"Category at key: {NameKey()} does not exist. Using defaults.");
                name = DebugCategorySettings.Default.name;
                logEnabled = DebugCategorySettings.Default.logEnabled;
                alwaysLogToFile = DebugCategorySettings.Default.alwaysLogToFile;
            }
        }
        //uses category field, to load an potentially overwrite all other fields.

        /// <summary>
        /// uses Categoryid to Load settings from player Prefs.  If not found, returns false, and does not alter any fields.
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
            name = DebugCategorySettings.Default.name;
            logEnabled = DebugCategorySettings.Default.logEnabled;
            alwaysLogToFile = DebugCategorySettings.Default.alwaysLogToFile;
            Save();
        }

        private string NameKey() => Key + "/Name";
        private string EnabledKey() => Key + "/Enabled";
        private string AlwaysLogToFilekey() => Key + "/alwaysLogToFile";


    }//end CategoricalDebug namespace

    /// <summary>
    /// This class provides functionality to easily register categories for use by CatDebug.    
    /// It also provides functions that allow getting/setting the enabled state or name of a particular category, and automatically stores these values to disk using Unity's PlayerPrefs.  
    /// If registered previously, these categories, and their states, are loaded from storage for initialization.
    /// </summary>
    public static class DebugCategoryRegistrar
    {
        const int maxCategoryID = 65001;
        static Dictionary<int, DebugCategorySettings> catDebugLoggingInfo = new Dictionary<int, DebugCategorySettings>();
        static Dictionary<string, int> categoryIndexByName = new Dictionary<string, int>();
        public const string debugCategoryPreferencesKeyBase = "CatDebugKey";
        //this is used to build the keys for storage in player prefs
        static public string KeyBaseForCatID(int catID)
        {
            return debugCategoryPreferencesKeyBase + catID.ToString();
        }
        //this one is used to check for existence (only key + fieldname gets stored as a values in playerprefs)
        static public string NameKeyForCatID(int catID)
        {
            return KeyBaseForCatID(catID) + "/Name";  //WARNING: code duplication- must match DebugCategorySettings.private string NameKey()(or SOME existing key)
        }


        /// <summary>
        /// returns an enumeration of all the currently registered category ID numbers
        /// </summary>
        static public IEnumerable<int> registeredIDs
        {
            get
            {
                //DoSetupIfNeeded()
                return catDebugLoggingInfo.Keys;//.GetEnumerator();
            }
        }
        /// <summary>
        /// A first-pass flag used for setup
        /// </summary>
        static bool setupComplete = false;

        /// <summary>
        /// This function performs a setup operation once, based on the setupComplete flag.  Subsequent calls will do nothing.
        /// Looks through the Unity PlayerPrefs to determine if any Debug Categories have been registered previously, and if so, loads and registers them.
        /// </summary>

        static DebugCategoryRegistrar()
        {
            //  PlayerPrefs.DeleteAll();  //needed for test
            //loop though all editor pref to see if the key exists:  if so, register it
            DoSetupIfNeeded();
        }
        static void DoSetupIfNeeded()
        {
            if (setupComplete) return;
            setupComplete = true;


            //CatDebugLog.PrependToNextLog("Initializing DebugCatRegistrar: START ... ");
            catDebugLoggingInfo.Clear();
            categoryIndexByName.Clear();
            for (int i = 0; i < maxCategoryID; i++)
            {
                bool exists = PlayerPrefs.HasKey(NameKeyForCatID(i));
                if (exists) // a key with this value has been found! Load it.
                {
                    
                    DebugCategorySettings newSetting = DebugCategorySettings.Load(i);
                    if (newSetting != null)
                    {
                        if (categoryIndexByName.ContainsKey(newSetting.name))
                        {
                            UnityEngine.Debug.LogWarning("Unexpected Data:  more that one entry with the same name found in PlayerPrefs.  name: " + newSetting.name + "  first found at ID:" + categoryIndexByName[newSetting.name] + "  also found at ID:" + i);
                            newSetting.Delete();
                        }
                        else
                        {
                            catDebugLoggingInfo.Add(i, newSetting);
                            categoryIndexByName.Add(newSetting.name, i);
                            UnityEngine.Debug.Log("found entry for ID: " + i + " Category Name: " + newSetting.name);
                        }
                    }
                    
                }

            }
           CatDebugLog.Log("Initializing DebugCatRegistrar: DONE.");
        }

        /// <summary>
        /// Function takes a name and returns an id.  if the name has been registered previously, it will get the same number, otherwise it will get an available id.  if no id's remain, this function will return -1
        /// The caller should retain this value, and reference the category with it, when using CatDebug.Log commands.
        /// </summary>
        /// <param name="catName">The name of the category to be registered.</param>
        /// <returns>The integer index assigned to the registered Category.  -1 if no more index values are available.</returns>
        public static int RegisterCategory(string catName)
        {
            if (string.IsNullOrEmpty(catName))
            {
                UnityEngine.Debug.LogException(new System.NullReferenceException("Attempting to set category name with blank or null category name is not permitted."));
                return 0;
            }

            int catID = GetCategoryID(catName); //if it was registered on a previous run and saved.. it will already exist in the dictionary, and this will return the catID it was assigned.
            if (catID != -1) 
                return catID;

            //     Debug.Log("Finding first available key");
            int firstKeyAvaialble = 0;
            for (; firstKeyAvaialble < maxCategoryID; firstKeyAvaialble++)
            {
                if (!catDebugLoggingInfo.ContainsKey(firstKeyAvaialble))
                    break;
            }
            // Debug.Log("Found first available key: "+ firstKeyAvaialble);
            DebugCategorySettings newCategorySettings = new DebugCategorySettings();
            newCategorySettings.name = catName;
            newCategorySettings.category = firstKeyAvaialble;
            newCategorySettings.logEnabled = true;
            newCategorySettings.alwaysLogToFile = false;
            newCategorySettings.Save();
            catDebugLoggingInfo.Add(firstKeyAvaialble, newCategorySettings);
            categoryIndexByName.Add(catName, firstKeyAvaialble);
           // CatDebugLog.Log("Saved new DebugCategorySettings object.   category name:"+ catName + "  assigned ID:"+ firstKeyAvaialble);

            return firstKeyAvaialble;


        }

        /// <summary>
        /// Removes the specified category from storage.  It's index may be reassigned after this.
        /// </summary>
        /// <param name="catName">The name of the category to be unregistered.</param>
        public static void UnRegisterCategory(string catName)
        {

            int catID = GetCategoryID(catName);
            if (catID != -1)
            {
                catDebugLoggingInfo[catID].Delete();
                catDebugLoggingInfo.Remove(catID);
                categoryIndexByName.Remove(catName);
            }
        }
        /// <summary>
        /// Removes the specified category from storage.  It's index may be reassigned after this.
        /// </summary>
        /// <param name="catName">The name of the category to be unregistered.</param>
        public static void UnRegisterCategory(int catID)
        {
            if (catID != -1)
            {
                DebugCategorySettings settings = catDebugLoggingInfo[catID];
                catDebugLoggingInfo.Remove(catID);
                categoryIndexByName.Remove(settings.name);
                settings.Delete();
            }
        }

        /// <summary>
        /// Finds the registered integer index of the specified category.  if not registered, returns -1;
        /// </summary>
        /// <param name="catName">The name of the registered category to lookup.</param>
        /// <returns>The integer index of the category.  returns -1 if not registered.</returns>
        public static int GetCategoryID(string catName)
        {
            if (categoryIndexByName.TryGetValue(catName, out int catID))
            {
                return catID;
            }
            /*foreach (DebugCategorySettings settingsPref in catDebugLoggingInfo.Values)
            {
                if (settingsPref.name == catName)
                    return settingsPref.category;
            }*/
            return -1;
        }

        /// <summary>
        /// Finds the name of the specified category.  if not registered, returns empty string.
        /// </summary>
        /// <param name="catID">The index of the registered category to lookup.</param>
        /// <returns>The name of the category.  if not registered, returns null.</returns>
        public static string GetCategoryName(int catID)
        {
            return catDebugLoggingInfo[catID].name;
        }

        /// <summary>
        /// Returns the enabled/disabled state of the specified category.  if not registered, returns false.
        /// </summary>
        /// <param name="catID">The index of the registered category to lookup.</param>
        /// <returns>The enabled state of the category.  if not registered, returns false.</returns>
        public static bool GetCategoryState(int catID)
        {

            return catDebugLoggingInfo[catID].logEnabled;
        }

        /// <summary>
        /// Finds the enabled/disabled state of the specified category.  if not registered, returns false.
        /// </summary>
        /// <param name="catID">The index of the registered category to lookup.</param>
        /// <returns>The enabled state of the category.  if not registered, returns false.</returns>
        public static bool GetCategoryAlwaysLogToFileState(int catID)
        {

            return catDebugLoggingInfo[catID].alwaysLogToFile;
        }


        /// <summary>
        /// Finds the PlayerPrefOption that stores the state of the specified category.  if not registered, returns null.
        /// </summary>
        /// <param name="catID">The index of the registered category to lookup.</param>
        /// <returns>The enabled state of the category.  if not registered, returns null.</returns>
        public static DebugCategorySettings GetCategorySettings(int catID)
        {

            if (catDebugLoggingInfo.ContainsKey(catID))
                return catDebugLoggingInfo[catID];
            return null;
        }


        /// <summary>
        /// Sets and saves the enabled/disabled state of the specified category.
        /// </summary>
        /// <param name="catID">The index of the registered category to set.</param>
        /// <param name="state">The new enabled state to be assigned to this category.</param>
        public static void SetCategoryState(int catID, bool state)
        {

            if (catDebugLoggingInfo.ContainsKey(catID))
            {
                catDebugLoggingInfo[catID].logEnabled = state;
            }
            else
                CatDebugLog.LogWarning("Failure attempting to set state of an unregistered DebugCategory ID:" + catID.ToString());
        }


        /// <summary>
        /// Sets and saves the enabled/disabled state of the specified category.
        /// </summary>
        /// <param name="catID">The index of the registered category to set.</param>
        /// <param name="state">The new enabled state to be assigned to this category.</param>
        public static void SetCategoryLogToFileOnDisabledState(int catID, bool state)
        {

            if (catDebugLoggingInfo.ContainsKey(catID))
            {
                catDebugLoggingInfo[catID].alwaysLogToFile = state;
            }
            else
                CatDebugLog.LogWarning("Failure attempting to set state of an unregistered DebugCategory ID:" + catID.ToString());
        }

        /// <summary>
        /// Generates a list of all the Registered category names, and puts them into the string List reference provided
        /// </summary>
        /// <param name="outputList">This ref parameter will be filled with all the Registered Names after calling this function.</param>
        public static void GetAllRegisteredCategoryNames(ref System.Collections.Generic.List<string> outputList)
        {
            if (outputList == null)
                outputList = new System.Collections.Generic.List<string>();
            else
                outputList.Clear();


            foreach (DebugCategorySettings sp in catDebugLoggingInfo.Values)
            {
                outputList.Add(sp.name);
            }

        }

        /// <summary>
        /// Creates and returns a new list containing all the Registered category names.
        /// </summary>        
        /// <returns>The returned List of strings will be filled with all the Registered Names.</returns>
        public static System.Collections.Generic.List<string> GetAllRegisteredCategoryNames()
        {
            System.Collections.Generic.List<string> outputList = new System.Collections.Generic.List<string>();
            GetAllRegisteredCategoryNames(ref outputList);
            return outputList;
        }
        /// <summary>
        /// Be removing all registered categories, this function will make previously registered category id's available (on a first come, first serve basis)
        /// </summary>
        public static void DeleteAllSavedCategoryKeysFromPlayerPrefs()
        {
            foreach (DebugCategorySettings sp in catDebugLoggingInfo.Values)
            {
                sp.Delete();
            }
            catDebugLoggingInfo.Clear();
            categoryIndexByName.Clear();
            for (int i = 0; i < maxCategoryID; i++)
            {
                string key = KeyBaseForCatID(i);// DebugCategorySettings.DebugPrefKeyByIndex(i);
                bool exists = PlayerPrefs.HasKey(key);// CatDebug.debugCategoryEnabledPreferencesKeyBase + i.ToString());
                if (exists) // a key with this value has been found! Load it.
                    PlayerPrefs.DeleteKey(key);
            }

        }//end delete all saved function


    }//end DebugCategoryRegistrar class
}