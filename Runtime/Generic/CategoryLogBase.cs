using System.Diagnostics;

namespace EyE.Debug
{
    /// <summary>
    /// Derive from this class to create a static category specific log class for use in your code.
    /// </summary>
    /// <example>
    /// class PhysicsDebug : CategoryLogBase<PhysicsDebug>
    /// {
    ///     protected override string CategoryName => "Physics";
    /// }
    /// ...
    /// PhysicsDebug.Log("Impulse resolved");
    /// ...
    /// </example>
    /// <typeparam name="TSelf">The class derived from this one </typeparam>
    public abstract class CategoryLogBase<TSelf>
        where TSelf : CategoryLogBase<TSelf>, new()
    {
        protected static readonly TSelf Instance = new TSelf();

        protected abstract string CategoryName { get; }

        private int? cachedCategorID;
        private PerCategoryDebugSettings _categorySettingsRef=null;
        public PerCategoryDebugSettings categorySettings
        {
            get
            {
                if (_categorySettingsRef == null)
                {
                    _categorySettingsRef = DebugCategoryRegistrar.GetCategorySettings(categoryID);
                }
                return _categorySettingsRef;
            }
        }

        protected int categoryID
        {
            get
            {
                if (!cachedCategorID.HasValue)
                {
                    cachedCategorID = DebugCategoryRegistrar.RegisterCategory(CategoryName);
                   // _categorySettingsRef = DebugCategoryRegistrar.GetCategoryStateOption(categoryID).catSettings;
                }
                return cachedCategorID.Value;
            }
        }


        // ---------- Logging ----------

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void Log(string message)
        {
            CatDebug.Log(Instance.categoryID, message);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void Log(params object[] message)
        {
            CatDebug.Log(Instance.categoryID, message);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void LogWarning(string message)
        {
            CatDebug.LogWarning(Instance.categoryID, message);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void LogError(string message)
        {
            CatDebug.LogError(Instance.categoryID, message);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void PrependToNextLog(params object[] message)
        {
            CatDebug.PrependToNextLog(Instance.categoryID, message);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void AppendToNextLog(params object[] message)
        {
            CatDebug.AppendToNextLog(Instance.categoryID, message);
        }

        // ---------- Category control passthrough ----------

        public static string GetCategoryName()
            => Instance.CategoryName;
    }

    class CatDebugLog : CategoryLogBase<CatDebugLog>
    {
        protected override string CategoryName => "CetegoricalLoggingItself";
    }
}

