using UnityEngine;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace EyE.Debug
{
    static class StringBuilderExtensions
    {
        static public void Append(this StringBuilder stringBuilder, params object[] objectArray)
        {
            if (objectArray != null)
            {
                foreach (object o in objectArray)
                    stringBuilder.Append(o);
            }

        }

        static public string NewLineString
        {
            get { return (string)System.Environment.NewLine; }
        }

        static public string ObjectArrayToString(params object[] objectArray)
        {
            return new StringBuilder().Append(objectArray).ToString();
        }
    }

    /// <summary>
    /// Instanced Version of CatDebug.  Create an new instance of this class when you want to log in a separate thread and use it's own prepend/append values.
    /// Otherwise, it is recommened to use the static CatDebug class instead.
    /// </summary>
    public class CatDebugInstance
    {
        /// <summary>
        /// Default constructor, blank string is assigned to instance name. Default log filename and path used.
        /// </summary>
        public CatDebugInstance() { InitFile(); }

        /// <summary>
        /// Constructor assigns the provided string to instance name, and if not null or empty enables the prependInstanceName flag.
        /// </summary>
        /// <param name="instanceName">may be null or empty to have no effect- otherwise, will be prepended to log output</param>
        /// <param name="logFilePath">use to override the default filename path</param>
        public CatDebugInstance(string instanceName, string logFilePath = "CategoricalLog.txt")
        {
            this.instanceName = instanceName;
            this.catLogFilePath = logFilePath;
            prependInstanceName = ! string.IsNullOrEmpty(instanceName);
            InitFile();
        }
        /// <summary>
        /// Name assigned to this instance, for potential display. By default instances will have a blank string for the name.
        /// Uniqueness of names is not checked, nor required (but it is recommended for most use-cases).
        /// </summary>
        public string instanceName = "CatDebug";

        #region globalSettings

        /// <summary>
        /// Controls weather or not the instanceName will be included Log messages, at the start of the message.
        /// Note: if the category is also being displayed, the instance name will after the category in the output message.
        /// </summary>
        public bool prependInstanceName = false;

        /// <summary>
        /// When single-line false, a newline will be included in the log, after the instance name.  When true, it will not add a newline.
        /// </summary>
        public bool prependInstanceNameSingleLine = false;

        /// <summary>
        ///  the option controls whether or not categories names will be prepended to log displays.
        ///  </summary>
        public bool addCategoryNameToLog = false;

        /// <summary>
        ///  the option controls whether or not categories names will be prepended to log displays.
        ///  </summary>
        public bool addCategoryNameToLogSingleLine = false;

        /// <summary>
        /// stores the state of the alwaysShowWarnings Preference.  When true, warning will be displayed, even if logging for the category is disabled.
        /// </summary>
        public bool alwaysShowWarnings = true;

        /// <summary>
        /// when true, log entries that are not sent to unity console, but ARE sent to a log file, will include the stack trace in the file.  When false, stack trace will not be included.
        /// </summary>
        public bool logToFileIncludeStackStrace = true;

        #endregion


        /// <summary>
        /// internal function checks to see if a newline should be added after the name (checks prependInstanceNameSingleLine), and does so- returning the result.
        /// </summary>
        /// <returns>the assigned instanceName string is returned, conditionally, with a newline at the end</returns>
        string InstanceNameText()
        {
            if (prependInstanceNameSingleLine) return instanceName;
            return instanceName + System.Environment.NewLine;
        }

        /// <summary>
        /// Text that includes the category name, and a label ("Category: ") for it.
        /// </summary>
        /// <param name="category">categoryID for which the name will be looked up.</param>
        /// <returns></returns>
        internal string CategoryDisplayText(int category)
        {   
            string catName = DebugCategoryRegistrar.GetCategoryName(category);
            if (catName == null) return "Unassigned Category.";
            if (addCategoryNameToLogSingleLine)
                return catName + ": ";
            return "Category: " + catName + StringBuilderExtensions.NewLineString;
        }

        /// <summary>
        /// Retrieves the internally stored name of category specified by the provided index.  If no name has been stored, it will return and empty, non-null string.
        /// While you could use this function to manually setup the categories, if multiple categories are in use, it is recommended that you use the ConditionalDebugRegistrar Class instead, which also saves names and states to storage.
        /// </summary>
        /// <param name="category">Unique Category index to get the name of.</param>
        /// <returns>Name found for the category. If no name has been stored, it will return and empty, non-null string.</returns>
        public static string GetCategoryName(int category)
        {
            string s = DebugCategoryRegistrar.GetCategoryName(category);
            if (s == null) return "Unassigned Category.";
            return s;
        }

        #region fileStream
        public readonly string catLogFilePath = "CategoricalLog.txt"; //will store in root of project folder.
        static System.IO.StreamWriter logFileStream = null;
        private static readonly object logStreamThreadLock = new object();

        /// <summary>
        /// Static constructor for this class, performs initialization the first time the class is touched.
        /// opens logfile steam
        /// </summary>
        void InitFile()
        {
            if (System.IO.File.Exists(catLogFilePath))
            {
                try
                {
                    System.IO.File.Copy(catLogFilePath, catLogFilePath + ".BAK");
                    System.IO.File.Delete(catLogFilePath);
                }
                catch
                {
                    UnityEngine.Debug.LogWarning("Failed to delete existing log file "+ catLogFilePath+ ", (possibly open).  Trying Write anyway.");
                }
            }

            //Create the file.
            try
            {
                logFileStream = new System.IO.StreamWriter(catLogFilePath);
                Application.quitting -= FlushAndCloseStream;//just incase was present
                Application.quitting += FlushAndCloseStream;
            }
            catch
            {
                UnityEngine.Debug.LogWarning("Failed to create file " + catLogFilePath);
            }
            
        }

        void FlushAndCloseStream()
        {
            CatDebugLog.Log("Flushing and closing log file stream.");//before closing.
            if (logFileStream != null)
            {
                logFileStream.Flush();
                logFileStream.Close();
            }
        }


        /// <summary>
        /// Write message to current file stream, if valid.
        /// </summary>
        /// <param name="s">message to write</param>
        internal void ToFile(string s)
        {
            Assert.isNotNull(logFileStream, "Failure attempting to write to Log File: provided stream for file ("+catLogFilePath+"), is invalid.");
            StringBuilder builder = new StringBuilder(s);
            if (logToFileIncludeStackStrace)
                builder.Append(StringBuilderExtensions.NewLineString, "StackTrace: ", StringBuilderExtensions.NewLineString, System.Environment.StackTrace);
            lock (logStreamThreadLock)
            {
                logFileStream.WriteLine(builder);
            }
        }

        #endregion

        /// <summary>
        /// This function will take an array of strings, and if a build with CatDebug.CONDITONAL_DEFINE_STRING ("EYE_DEBUG") defined is running, it will concatenate together the strings and send the result to Debug.Log for display.
        /// </summary>
        /// <param name="message">A set of string parameters that will be concatenated and sent to Debug.Log for display.</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public void Log(params object[] message)
        {
            Log(StringBuilderExtensions.ObjectArrayToString(message));
        }

        /// <summary>
        /// This function will take an array of strings, and if a build with CatDebug.CONDITONAL_DEFINE_STRING ("EYE_DEBUG") defined is running, it will concatenate together the strings and send the result to Debug.Log for display.
        /// </summary>
        /// <param name="message">A set of string parameters that will be concatenated and sent to Debug.Log for display.</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public void Log(string message)
        {
            StringBuilder outputMessage = new StringBuilder();
            if (prependInstanceName)
                outputMessage.Append(InstanceNameText());
            outputMessage.Append(prependText);
            prependText.Clear();
            outputMessage.Append(message);
            outputMessage.Append(appendText);
            appendText.Clear();
            UnityEngine.Debug.Log(outputMessage);
        }

        /// <summary>
        /// This function will take a single string, and if the category is enabled, and if a DEBUG build is running, it will send it to Debug.Log for display.
        /// If addCategoryNameToLog is true, the category name will be prepended to the final string before sending it to Debug.Log
        /// </summary>
        /// <param name="category">Only if this Category index is enabled will this function display a log.  With the appropriate options selected, this category's name may be prepended the log entry.</param>
        /// <param name="message">A string sent to Debug.Log for display.</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public void Log(int category, string message)
        {
            Log(category, new string[1] { message });
            return;
        }


        /// <summary>
        /// This function will take an array of strings, and if the specified category is enabled, and if a DEBUG build is running, it will concatenate together the strings and send the result to Debug.Log for display.
        /// The advantage to passing a comma separated array of strings, rather than a single preformed string, is that concatenation can be avoided, when unnecessary.
        /// If addCategoryNameToLog is true, the category name will be prepended to the final string before sending it to Debug.Log
        /// </summary>
        /// <param name="category">Only if this Category index is enabled will this function display a log.  With the appropriate options selected, this category's name may be prepended the log entry.</param>
        /// <param name="message">A set of string parameters that will be concatenated and sent to Debug.Log for display.</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public void Log(int category, params object[] message)
        {
            StringBuilder output = new StringBuilder();
            PerCategoryDebugSettings settings = DebugCategoryRegistrar.GetCategorySettings(category);

            if (settings.enableConsoleLogging || settings.enableFileLogging)
            {
                if (addCategoryNameToLog)
                    output.Append(CategoryDisplayText(category));
                if (prependInstanceName)
                    output.Append(InstanceNameText());

                output.Append(GetThenClearPrependText(category));
                output.Append(StringBuilderExtensions.ObjectArrayToString(message));
                output.Append(GetThenClearAppendText(category));

                if (settings.enableConsoleLogging)
                    UnityEngine.Debug.Log(output.ToString());
                if (settings.enableFileLogging)
                    ToFile(output.ToString());
            }
        }

        /// <summary>
        /// This function takes a set of strings as parameters and uses the first one as the category, the rest are part of the message to be displayed.
        /// The message is only displayed if the Category has been registered, and is enabled
        /// </summary>
        /// <param name="categoryName">The category name determines which category will be checked. Only if this category has been registered, and is enabled will the function display the message.</param>
        /// <param name="message">A set of string parameters that will be concatenated and sent to Debug.Log for display.</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public void Log(string categoryName, params object[] message)
        {
            int categoryID = DebugCategoryRegistrar.GetCategoryID(categoryName);
            if (categoryID != -1)
                Log(categoryID, message);
        }

        /// <summary>
        /// This function will take a single string, and if the category is enabled, and if a DEBUG build is running, it will send it to Debug.LogError for display.
        /// If addCategoryNameToLog is true, the category name will be prepended to the final string before sending it to Debug.LogError
        /// </summary>
        /// <param name="category">Errors are always logged. With the appropriate options selected, this category's name may be prepended the log entry.</param>
        /// <param name="message">A string sent to Debug.LogError for display.</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public void LogError(int category, string message)
        {

            if (addCategoryNameToLog)
                message=(CategoryDisplayText(category) + message);
            UnityEngine.Debug.LogError(message);
            PerCategoryDebugSettings settings = DebugCategoryRegistrar.GetCategorySettings(category);
            if (settings.enableFileLogging)
                ToFile(" **ERROR** " + message);

        }


        /// <summary>
        /// This function will take a single string, and if the category is enabled, and if a DEBUG build is running, it will send it to Debug.LogWarning for display.
        /// If addCategoryNameToLog is true, the category name will be prepended to the final string before sending it to Debug.LogWarning
        /// </summary>
        /// <param name="category">Only if this Category index is enabled, or the alwaysShowWarnings option is set to true, will this function display a log.  With the appropriate options selected, this category's name may be prepended the log entry.</param>
        /// <param name="message">A string sent to Debug.LogWarning for display.</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public void LogWarning(int category, string message)
        {
            PerCategoryDebugSettings settings = DebugCategoryRegistrar.GetCategorySettings(category);
            if (addCategoryNameToLog)
                message=(CategoryDisplayText(category) + message);

            if (alwaysShowWarnings || settings.enableConsoleLogging)
                UnityEngine.Debug.LogWarning(message);
            if (alwaysShowWarnings || settings.enableFileLogging)
                ToFile(" **Warning** " + message);

        }

        #region append and prepend members

        #region append and prepend- internal classes, data members, functions
        /// <summary>
        /// internal class used to connect a string builder and a lock object, to make the append,clear, and tostring threadsafe
        /// </summary>
        class StringBuilderAndLock
        {
            /// <summary>
            /// internal stringBuilderReference.  Only touched when locked.
            /// </summary>
            StringBuilder stringBuilder;
            /// <summary>
            /// object used to hold the lock on the stringBuilder.
            /// </summary>
            object builderLock = new object();

            /// <summary>
            /// default constructor instantiates a new StringBuilder object
            /// </summary>
            public StringBuilderAndLock()
            {
                stringBuilder = new StringBuilder();
            }
            /// <summary>
            /// uses provided reference for internalStringBuilder
            /// </summary>
            /// <param name="stringBuilder">StringBuilder to reference internally</param>
            public StringBuilderAndLock(StringBuilder stringBuilder)
            {
                this.stringBuilder = stringBuilder;
            }


            /// <summary>
            /// Appends the provided string to the stringBuilder
            /// </summary>
            /// <param name="s">sting to be appended</param>
            public void Append(string s)
            {
                lock (builderLock)
                    stringBuilder.Append(s);
            }

            /// <summary>
            /// locks then clears the contents of the stringBuilder
            /// </summary>
            public void Clear()
            {
                lock (builderLock)
                    stringBuilder.Clear();
            }

            /// <summary>
            /// locks, then converts the stringBuilder to a string
            /// </summary>
            /// <returns>returns the string value stored in the StringBuilder</returns>
            public override string ToString()
            {
                string str;
                lock (builderLock)
                    str = stringBuilder.ToString();
                return str;
            }

            /// <summary>
            /// Automatically creates a new StringBuilderAndLock when assigned a StringBuilder
            /// </summary>
            /// <param name="builder">the string builder that will be referenced by the new StringBuilderAndLock</param>
            public static implicit operator StringBuilderAndLock(StringBuilder builder)
            {
                return new StringBuilderAndLock(builder);
            }
        }

        /// <summary>
        /// Internal string builder class, used to store text to be prepended to the next log.
        /// </summary>
        StringBuilder prependText = new StringBuilder();
        /// <summary>
        /// Internal dictionary of string builders, one for each category. stores values that will be prepended to that category's logs.
        /// </summary>
        ConcurrentDictionary<int, StringBuilderAndLock> prependTextByCategory = new ConcurrentDictionary<int, StringBuilderAndLock>();

        /// <summary>
        /// Internal string builder class, used to store text to be appended to the next log, when no category is specified.
        /// </summary>
        StringBuilder appendText = new StringBuilder();
        /// <summary>
        /// Internal dictionary of string builders, one for each category. stores values that will be appended to that category's logs.
        /// </summary>
        ConcurrentDictionary<int, StringBuilderAndLock> appendTextByCategory = new ConcurrentDictionary<int, StringBuilderAndLock>();


        /// <summary>
        /// Checks the provided dictionary for existence of an entry for the provided category key.
        /// If found, the function returns the value found in the key, and also clears the key of any text.
        /// </summary>
        /// <param name="category">category to use for lookup in the dictionary</param>
        /// <param name="dictionary">dictionary to look in</param>
        /// <returns>the value found in the dictionary, that is associated with the key.  If no value is found, return an empty string "".</returns>
        static string GetThenClearTextFromDictionary(int category, ConcurrentDictionary<int, StringBuilderAndLock> dictionary)
        {
            StringBuilderAndLock builderRef;
            if (dictionary.TryGetValue(category, out builderRef))
            {
                if (builderRef != null)//sanity check
                {
                    string retValue = builderRef.ToString();
                    builderRef.Clear();
                    return retValue;
                }
            }
            return "";
        }


        /// <summary>
        /// Checks the prependTextByCategory dictionary for existence of an entry for the provided category key.
        /// If found, the function returns the value found in the key, and also clears the key of any text.
        /// </summary>
        /// <param name="category">category to use for lookup in the dictionary</param>
        /// <returns>the value found in the dictionary, that is associated with the key.  If no value is found, return an empty string "".</returns>
        string GetThenClearPrependText(int category)
        {
            return GetThenClearTextFromDictionary(category, prependTextByCategory);
        }
        /// <summary>
        /// Checks the appendTextByCategory dictionary for existence of an entry for the provided category key.
        /// If found, the function returns the value found in the key, and also clears the key of any text.
        /// </summary>
        /// <param name="category">category to use for lookup in the dictionary</param>
        /// <returns>the value found in the dictionary, that is associated with the key.  If no value is found, return an empty string "".</returns>
        string GetThenClearAppendText(int category)
        {
            return GetThenClearTextFromDictionary(category, appendTextByCategory);
        }
        #endregion
        /// <summary>
        /// This function will store the provided text, and prepend it to the text of the next call to a Log function.  When finally displayed by a Log function, the prepend text will be reset to an empty string.
        /// This text is not prepended to error, exception or warning logs.
        /// Calling this function multiple times, will grow the prepend text, by appending the passed message to it.
        /// </summary>
        /// <param name="message">Text that will be prepended to the next log message</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public void PrependToNextLog(string message)
        {
            prependText.Append(message);
        }

        /// <summary>
        /// Clears the prepend text used when no category is provided.
        /// </summary>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public void ClearPrependText()
        {
            prependText.Clear();
        }

        /// <summary>
        /// This function will store the provided text, and associate it with the provided category.
        /// However, if all logging for the category is disabled: the function will return immediately, and do nothing.
        /// The text will then be prepend to the text of the next call of a Log function with the same category.
        /// After be drawn in a log call the prepend text, for that category, will be cleared.
        /// This text is not prepended to error, exception or warning logs.
        /// Calling this function multiple times, will grow the prepend text, by appending the passed message to it.
        /// </summary>
        /// <param name="category">Only if this Category index is enabled will this function Prepend to the next log (which may be of a different category)</param>
        /// <param name="message">Text that will be prepended to the next log message</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public void PrependToNextLog(int category, string message)
        {
            AppendOrAddMessgeToTextByCategoryDictionary(category, new string[1] { message }, prependTextByCategory);
        }

        /// <summary>
        /// Clears the prepend text for the provided category, if any exists.
        /// </summary>
        /// <param name="category">category to clear the prepend Text for</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public void ClearPrependText(int category)
        {
            StringBuilderAndLock prependStringBuilder;
            if (prependTextByCategory.TryGetValue(category, out prependStringBuilder))
            {
                prependStringBuilder.Clear();
            }
        }

        /// <summary>
        /// This function will store the provided text, and associate it with the provided category.
        /// However, if all logging for the category is disabled: the function will return immediately, and do nothing.
        /// The text will then be prepend to the text of the next call of a Log function with the same category.
        /// After be drawn in a log call the prepend text, for that category, will be cleared.
        /// This text is not prepended to error, exception or warning logs.
        /// Calling this function multiple times, will grow the prepend text, by appending the passed message to it.
        /// </summary>
        /// <param name="category">Only if this Category index is enabled will this function Prepend to the next log (which may be of a different category)</param>
        /// <param name="messageObjects">Array of objects that will be converted to strings, then are be prepended to the next log message</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public void PrependToNextLog(int category, params object[] messageObjects)
        {
            AppendOrAddMessgeToTextByCategoryDictionary(category, messageObjects, prependTextByCategory);
        }


        /// <summary>
        ///  This function will store the provided text, and associate it with the provided category.
        /// However, if all logging for the category is disabled: the function will return immediately, and do nothing.
        /// The text will then be prepend to the text of the next call of a Log function with the same category.
        /// After be drawn in a log call the append text, for that category, will be cleared.
        /// This text is not append to error, exception or warning logs.
        /// Calling this function multiple times, will grow the prepend text, by appending the passed message to it.
        /// </summary>
        /// <param name="message">Text that will be appended to the next log message</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public void AppendToNextLog(string message)
        {
            appendText.Append(message);
        }

        /// <summary>
        /// Clears the prepend text used when no category is provided.
        /// </summary>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public void ClearAppendText()
        {
            appendText.Clear();
        }

        /// <summary>
        /// This function will store the provided text, and append it to the text of the next call to a Log function.  When finally displayed by a Log function, the append-text will be reset to an empty string.
        /// This text is not append to error, exception or warning logs.
        /// Calling this function multiple times, will grow the prepend text, by appending the passed message to it.
        /// </summary>
        /// <param name="category">Only if this Category index is enabled will this function append to the next log (which may be of a different category)</param>
        /// <param name="message">Text that will be appended to the next log message</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public void AppendToNextLog(int category, string message)
        {
            AppendOrAddMessgeToTextByCategoryDictionary(category, new string[1] { message }, appendTextByCategory);
        }

        /// <summary>
        /// Clears the append text for the provided category, if any exists.
        /// </summary>
        /// <param name="category">category to clear the prepend Text for</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public void ClearAppendText(int category)
        {
            StringBuilderAndLock appendStringBuilder;
            if (appendTextByCategory.TryGetValue(category, out appendStringBuilder))
            {
                appendStringBuilder.Clear();
            }
        }

        /// <summary>
        /// This function will store the provided text, and append it to the text of the next call to a Log function.  When finally displayed by a Log function, the append-text will be reset to an empty string.
        /// This text is not append to error, exception or warning logs.
        /// Calling this function multiple times, will grow the prepend text, by appending the passed message to it.
        /// </summary>
        /// <param name="category">Only if this Category index is enabled will this function append to the next log (which may be of a different category)</param>
        /// <param name="messageObjects">Array of objects that will be converted to strings, then are be appended to the next log message</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public void AppendToNextLog(int category, params object[] messageObjects)
        {
            AppendOrAddMessgeToTextByCategoryDictionary(category, messageObjects, appendTextByCategory);
        }


        /// <summary>
        /// Internal function used to check the dictionary for an entry associated with the provided category, and creating one if needed.
        /// The provided text will be appended to any entry found.
        /// </summary>
        /// <param name="category">category to use for lookup in the dictionary</param>
        /// <param name="messageObjects">objects that will be converted to strings and appended to the found/created dictionary entry</param>
        /// <param name="dictionary">dictionary reference to use for lookup</param>
        static void AppendOrAddMessgeToTextByCategoryDictionary(int category, object[] messageObjects, ConcurrentDictionary<int, StringBuilderAndLock> dictionary)
        {
            PerCategoryDebugSettings settings = DebugCategoryRegistrar.GetCategorySettings(category);
            if (settings.enableConsoleLogging || settings.enableFileLogging)
            {
                string message = StringBuilderExtensions.ObjectArrayToString(messageObjects);
                StringBuilderAndLock builderRef;
                if (dictionary.TryGetValue(category, out builderRef))
                {
                    if (builderRef != null)// sanity check- should always be true
                        builderRef.Append(message);
                    else
                        dictionary[category] = new StringBuilder(message);
                }
                else if (!dictionary.TryAdd(category, new StringBuilder(message)))
                    dictionary[category].Append(message);
            }
        }
        #endregion

    }

    /// <summary>
    /// This static singleton of the CatDebugInstance class, is used by the DebugCategoryRegistrar and unity editor components for UI.
    /// Allow logging by category, each of which may be enabled or disabled individually.  Disabled categories, will NOT output messages, log to file, or throw asserts, depending on the category's settings.
    /// </summary>
    public static class CatDebug
    {
        public const string CONDITIONAL_DEFINE_STRING = "EYE_DEBUG";
        
        static CatDebugInstance instance = new CatDebugInstance();

        /// <summary>
        /// Static constructor for this class, performs initialization the first time the class is touched.
        /// Loads setting from PlayerPrefs via CatDebugOptions
        /// </summary>
        static CatDebug()
        {
            //general settings that affect all categories
            CatDebug.addCategoryNameToLog = CatDebugGlobalOptions.addCategoryNameToLog;
            CatDebug.addCategoryNameToLogSingleLine = CatDebugGlobalOptions.addCategoryNameToLogSingleLine;
            CatDebug.alwaysShowWarnings = CatDebugGlobalOptions.alwaysShowWarnings;
            CatDebug.logToFileIncludeStackTrace = CatDebugGlobalOptions.logToFileIncludeStackTrace;

        }

        #region globalSettings

        public static string catLogFilePath => instance.catLogFilePath;

        /// <summary>
        ///  the option controls whether or not categories names will be prepended to log displays.
        ///  </summary>
        public static bool addCategoryNameToLog
        {
            get { return instance.addCategoryNameToLog; }
            set { instance.addCategoryNameToLog = value; }
        }

        /// <summary>
        ///  the option controls whether or not categories names will be prepended to log displays.
        ///  </summary>
        public static bool addCategoryNameToLogSingleLine
        {
            get { return instance.addCategoryNameToLogSingleLine; }
            set { instance.addCategoryNameToLogSingleLine = value; }
        }

        /// <summary>
        /// stores the state of the alwaysShowWarnings Preference.  When true, warning will be displayed, even if logging for the category is disabled.
        /// </summary>
        public static bool alwaysShowWarnings
        {
            get { return instance.alwaysShowWarnings; }
            set { instance.alwaysShowWarnings = value; }
        }

        /// <summary>
        /// when true, log entries that are not sent to unity console, but ARE sent to a log file, will include the stack trace in the file.  When false, stack trace will not be included.
        /// </summary>
        public static bool logToFileIncludeStackTrace
        {
            get { return instance.logToFileIncludeStackStrace; }
            set { instance.logToFileIncludeStackStrace = value; }
        }
        #endregion

        /// <summary>
        /// This function will take an array of strings, and if a DEBUG build is running, it will concatenate together the strings and send the result to Debug.Log for display.
        /// </summary>
        /// <param name="message">A set of string parameters that will be concatenated and sent to Debug.Log for display.</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public static void Log(string message)
        {
            instance.Log(message);
        }


        /// <summary>
        /// This function will take an array of strings, and if the specified category is enabled, and if a DEBUG build is running, it will concatenate together the strings and send the result to Debug.Log for display.
        /// The advantage to passing a comma separated array of strings, rather than a single preformed string, is that concatenation can be avoided, when unnecessary.
        /// If addCategoryNameToLog is true, the category name will be prepended to the final string before sending it to Debug.Log
        /// </summary>
        /// <param name="category">Only if this Category index is enabled will this function display a log.  With the appropriate options selected, this category's name may be prepended the log entry.</param>
        /// <param name="message">A set of string parameters that will be concatenated and sent to Debug.Log for display.</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public static void Log(int category, params object[] message)
        {
            instance.Log(category, message);
        }

        /// <summary>
        /// This function will take a single string, and if the category is enabled, and if a DEBUG build is running, it will send it to Debug.Log for display.
        /// If addCategoryNameToLog is true, the category name will be prepended to the final string before sending it to Debug.Log
        /// </summary>
        /// <param name="category">Only if this Category index is enabled will this function display a log.  With the appropriate options selected, this category's name may be prepended the log entry.</param>
        /// <param name="message">A string sent to Debug.Log for display.</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public static void Log(int category, string message)
        {
            instance.Log(category, message);
        }

        /// <summary>
        /// This function will take a single string, and if the category is enabled, and if a DEBUG build is running, it will send it to Debug.LogError for display.
        /// If addCategoryNameToLog is true, the category name will be prepended to the final string before sending it to Debug.LogError
        /// </summary>
        /// <param name="category">Errors are always logged. With the appropriate options selected, this category's name may be prepended the log entry.</param>
        /// <param name="message">A string sent to Debug.LogError for display.</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public static void LogError(int category, string message)
        {
            instance.LogError(category, message);
        }

        /// <summary>
        /// This function will take a single string, and if the category is enabled, and if a DEBUG build is running, it will send it to Debug.LogWarning for display.
        /// If addCategoryNameToLog is true, the category name will be prepended to the final string before sending it to Debug.LogWarning
        /// </summary>
        /// <param name="category">Only if this Category index is enabled, or the alwaysShowWarnings option is set to true, will this function display a log.  With the appropriate options selected, this category's name may be prepended the log entry.</param>
        /// <param name="message">A string sent to Debug.LogWarning for display.</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public static void LogWarning(int category, string message)
        {
            instance.LogWarning(category, message);
        }

        #region append and prepend functions
        /// <summary>
        /// This function will store the provided text, and prepend it to the text of the next call to a Log function.  When finally displayed by a Log function, the prepend text will be reset to an empty string.
        /// This text is not prepended to error, exception or warning logs.
        /// Calling this function multiple times, will grow the prepend text, by appending the passed message to it.
        /// </summary>
        /// <param name="message">Text that will be prepended to the next log message</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public static void PrependToNextLog(string message)
        {
            instance.PrependToNextLog(message);
        }

        /// <summary>
        /// This function will store the provided text, and associate it with the provided category.
        /// However, if all logging for the category is disabled: the function will return immediately, and do nothing.
        /// The text will then be prepend to the text of the next call of a Log function with the same category.
        /// After be drawn in a log call the prepend text, for that category, will be cleared.
        /// This text is not prepended to error, exception or warning logs.
        /// Calling this function multiple times, will grow the prepend text, by appending the passed message to it.
        /// </summary>
        /// <param name="category">Only if this Category index is enabled will this function Prepend to the next log (which may be of a different category)</param>
        /// <param name="message">Text that will be prepended to the next log message</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public static void PrependToNextLog(int category, string message)
        {
            instance.PrependToNextLog(category, message);
        }

        /// <summary>
        /// This function will store the provided text, and associate it with the provided category.
        /// However, if all logging for the category is disabled: the function will return immediately, and do nothing.
        /// The text will then be prepend to the text of the next call of a Log function with the same category.
        /// After be drawn in a log call the prepend text, for that category, will be cleared.
        /// This text is not prepended to error, exception or warning logs.
        /// Calling this function multiple times, will grow the prepend text, by appending the passed message to it.
        /// </summary>
        /// <param name="category">Only if this Category index is enabled will this function Prepend to the next log (which may be of a different category)</param>
        /// <param name="messageObjects">Array of objects that will be converted to strings, then are be prepended to the next log message</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public static void PrependToNextLog(int category, params object[] messageObjects)
        {
            instance.PrependToNextLog(category, messageObjects);
        }


        /// <summary>
        ///  This function will store the provided text, and associate it with the provided category.
        /// However, if all logging for the category is disabled: the function will return immediately, and do nothing.
        /// The text will then be prepend to the text of the next call of a Log function with the same category.
        /// After be drawn in a log call the append text, for that category, will be cleared.
        /// This text is not append to error, exception or warning logs.
        /// Calling this function multiple times, will grow the prepend text, by appending the passed message to it.
        /// </summary>
        /// <param name="message">Text that will be appended to the next log message</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public static void AppendToNextLog(string message)
        {
            instance.AppendToNextLog(message);
        }

        /// <summary>
        /// This function will store the provided text, and append it to the text of the next call to a Log function.  When finally displayed by a Log function, the append-text will be reset to an empty string.
        /// This text is not append to error, exception or warning logs.
        /// Calling this function multiple times, will grow the prepend text, by appending the passed message to it.
        /// </summary>
        /// <param name="category">Only if this Category index is enabled will this function append to the next log (which may be of a different category)</param>
        /// <param name="message">Text that will be appended to the next log message</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public static void AppendToNextLog(int category, string message)
        {
            instance.AppendToNextLog(category, message);
        }

        /// <summary>
        /// This function will store the provided text, and append it to the text of the next call to a Log function.  When finally displayed by a Log function, the append-text will be reset to an empty string.
        /// This text is not append to error, exception or warning logs.
        /// Calling this function multiple times, will grow the prepend text, by appending the passed message to it.
        /// </summary>
        /// <param name="category">Only if this Category index is enabled will this function append to the next log (which may be of a different category)</param>
        /// <param name="messageObjects">Array of objects that will be converted to strings, then are be appended to the next log message</param>
        [Conditional(CatDebug.CONDITIONAL_DEFINE_STRING)]
        public static void AppendToNextLog(int category, params object[] messageObjects)
        {
            instance.AppendToNextLog(category, messageObjects);
        }
        #endregion



    }//End Cat Debug
}
