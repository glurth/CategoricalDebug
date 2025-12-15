using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Diagnostics;

namespace EyE.Unity.CategoricalDebug
{
    /// <summary>
    /// example: 
    /// class PhysicsDebug : CategoryLogBase<PhysicsDebug>
    /// {
    ///     protected override string CategoryName => "Physics";
    /// }
    /// ...
    /// PhysicsDebug.Log("Impulse resolved");
    /// ...
    /// </summary>
    /// <typeparam name="TSelf"></typeparam>
    public abstract class CategoryLogBase<TSelf>
        where TSelf : CategoryLogBase<TSelf>, new()
    {
        protected static readonly TSelf Instance = new TSelf();

        protected abstract string CategoryName { get; }

        private int? cachedCategorID;
        private DebugCategorySettings _categorySettingsRef=null;
        public DebugCategorySettings categorySettings
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

        protected bool Active => categorySettings.logEnabled;

        // ---------- Logging ----------

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void Log(string message)
        {
            if (!Instance.Active) return;
            CatDebug.Log(Instance.categoryID, message);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void Log(params object[] message)
        {
            if (!Instance.Active) return;
            CatDebug.Log(Instance.categoryID, message);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void LogWarning(string message)
        {
            if (!Instance.Active) return;
            CatDebug.LogWarning(Instance.categoryID, message);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void LogError(string message)
        {
            if (!Instance.Active) return;
            CatDebug.LogError(Instance.categoryID, message);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void PrependToNextLog(string message)
        {
            if (!Instance.Active) return;
            CatDebug.PrependToNextLog(Instance.categoryID, message);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void AppendToNextLog(string message)
        {
            if (!Instance.Active) return;
            CatDebug.AppendToNextLog(Instance.categoryID, message);
        }

        // ---------- Category control passthrough ----------

        public static bool IsEnabled()
            => Instance.Active;

        public static string GetCategoryName()
            => Instance.CategoryName;
    }

    class CatDebugLog : CategoryLogBase<CatDebugLog>
    {
        protected override string CategoryName => "CetegoricalLoggingItself";
    }
}

