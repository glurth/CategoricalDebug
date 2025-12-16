using System;
using System.Diagnostics;

//NOTE: this file references the Assert class defined in the same namespace, but in the dependency repo https://github.com/glurth/Assert 
namespace EyE.Debug
{
    /// <summary>
    /// Derive from this class to create a static category specific Assert class for use in your code.
    /// example: 
    /// class PhysicsAssert : CategoryAssertBase<PhysicsAssert>
    /// {
    ///     protected override string CategoryName => "Physics";
    /// }
    /// ...
    /// PhysicsAssert.IsNotNull(rigidBody,"rigidBody not assigned");
    /// ...
    /// <typeparam name="TSelf">The class derived from this one </typeparam>
    public abstract class CategoryAssertBase<TSelf>
        where TSelf : CategoryAssertBase<TSelf>, new()
    {
        protected static readonly TSelf Instance = new TSelf();

        protected abstract string CategoryName { get; }

        private int? cachedCategory;
        private PerCategoryDebugSettings _categorySettingsRef = null;
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
                if (!cachedCategory.HasValue)
                {
                    cachedCategory = DebugCategoryRegistrar.RegisterCategory(CategoryName);
                    _categorySettingsRef = DebugCategoryRegistrar.GetCategorySettings(categoryID);
                }
                return cachedCategory.Value;
            }
        }

        protected bool Active =>
            categorySettings.enableAsserts;

        // ---------- Boolean asserts (delegated) ----------

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void IsTrue(bool b, string failMessage, object context = null)
        {
            if (!Instance.Active) return;
            Assert.isTrue(b, failMessage, context);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void IsFalse(bool b, string failMessage, object context = null)
        {
            if (!Instance.Active) return;
            Assert.isFalse(b, failMessage, context);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void ExpensiveIsTrue<T>(Func<bool> condition, string failMessage, object context = null)
        {
            if (!Instance.Active) return;
            Assert.expensiveIsTrue<T>(condition, failMessage, context);
        }

        // ---------- Null checks ----------

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void IsNotNull(object obj, string failMessage, object context = null)
        {
            if (!Instance.Active) return;
            Assert.isNotNull(obj, failMessage, context);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void AreNotNull(string failMessage, params object[] objs)
        {
            if (!Instance.Active) return;
            Assert.areNotNull(failMessage, objs);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void AreNotNullWithContext(string failMessage, object context, params object[] objs)
        {
            if (!Instance.Active) return;
            Assert.areNotNullWithContext(failMessage, context, objs);
        }

        // ---------- Equality ----------

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void IsEqual<T>(T expected, T actual, string failMessage, object context = null)
            where T : IEquatable<T>
        {
            if (!Instance.Active) return;
            Assert.isEqual(expected, actual, failMessage, context);
        }

        // ---------- Type ----------

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void Is<T>(object obj, string failMessage)
        {
            if (!Instance.Active) return;
            Assert.Is<T>(obj, failMessage);
        }

        // ---------- Generic display variants ----------

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void IsTrue<T>(bool b, string failMessage, object context = null)
        {
            if (!Instance.Active) return;
            Assert.isTrue<T>(b, failMessage, context);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void IsFalse<T>(bool b, string failMessage, object context = null)
        {
            if (!Instance.Active) return;
            Assert.isFalse<T>(b, failMessage, context);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void IsNotNull<T>(object obj, string failMessage, object context = null)
        {
            if (!Instance.Active) return;
            Assert.isNotNull<T>(obj, failMessage, context);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void AreNotNull<T>(string failMessage, params object[] objs)
        {
            if (!Instance.Active) return;
            Assert.areNotNull<T>(failMessage, objs);
        }

        [Conditional(CatDebug.CONDITONAL_DEFINE_STRING)]
        public static void AreNotNullWithContext<T>(string failMessage, object context, params object[] objs)
        {
            if (!Instance.Active) return;
            Assert.areNotNullWithContext<T>(failMessage, context, objs);
        }

    }
}
