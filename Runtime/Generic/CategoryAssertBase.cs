using System;
using System.Diagnostics;
using EyE.Debug;

namespace EyE.Unity.CategoricalDebug
{
    public abstract class CategoryAssertBase<TSelf>
        where TSelf : CategoryAssertBase<TSelf>, new()
    {
        protected static readonly TSelf Instance = new TSelf();

        protected abstract string CategoryName { get; }

        private int? cachedCategory;
        private DebugCategorySettings _categorySettingsRef = null;
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
                if (!cachedCategory.HasValue)
                {
                    cachedCategory = DebugCategoryRegistrar.RegisterCategory(CategoryName);
                    _categorySettingsRef = DebugCategoryRegistrar.GetCategorySettings(categoryID);
                }
                return cachedCategory.Value;
            }
        }

        protected bool Active =>
            categorySettings.logEnabled;

        // ---------- Boolean asserts (delegated) ----------

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void IsTrue(bool b, string message, object context = null)
        {
            if (!Instance.Active) return;
            Assert.isTrue(b, message, context);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void IsFalse(bool b, string message, object context = null)
        {
            if (!Instance.Active) return;
            Assert.isFalse(b, message, context);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void ExpensiveIsTrue<T>(Func<bool> condition, string message, object context = null)
        {
            if (!Instance.Active) return;
            Assert.expensiveIsTrue<T>(condition, message, context);
        }

        // ---------- Null checks ----------

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void IsNotNull(object obj, string message, object context = null)
        {
            if (!Instance.Active) return;
            Assert.isNotNull(obj, message, context);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void AreNotNull(string message, params object[] objs)
        {
            if (!Instance.Active) return;
            Assert.areNotNull(message, objs);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void AreNotNullWithContext(string message, object context, params object[] objs)
        {
            if (!Instance.Active) return;
            Assert.areNotNullWithContext(message, context, objs);
        }

        // ---------- Equality ----------

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void IsEqual<T>(T expected, T actual, string message, object context = null)
            where T : IEquatable<T>
        {
            if (!Instance.Active) return;
            Assert.isEqual(expected, actual, message, context);
        }

        // ---------- Type ----------

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void Is<T>(object obj, string message)
        {
            if (!Instance.Active) return;
            Assert.Is<T>(obj, message);
        }

        // ---------- Generic display variants ----------

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void IsTrue<T>(bool b, string message, object context = null)
        {
            if (!Instance.Active) return;
            Assert.isTrue<T>(b, message, context);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void IsFalse<T>(bool b, string message, object context = null)
        {
            if (!Instance.Active) return;
            Assert.isFalse<T>(b, message, context);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void IsNotNull<T>(object obj, string message, object context = null)
        {
            if (!Instance.Active) return;
            Assert.isNotNull<T>(obj, message, context);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void AreNotNull<T>(string message, params object[] objs)
        {
            if (!Instance.Active) return;
            Assert.areNotNull<T>(message, objs);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void AreNotNullWithContext<T>(string message, object context, params object[] objs)
        {
            if (!Instance.Active) return;
            Assert.areNotNullWithContext<T>(message, context, objs);
        }

    }
}
