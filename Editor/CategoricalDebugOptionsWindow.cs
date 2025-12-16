using UnityEditor;

namespace EyE.EditorUnity
{
    public class CategoricalDebugOptionsWindow : EditorWindow
    {
        [MenuItem("Edit/CatDebugPreferences...")]
        static void Open()
        {
            GetWindow<CategoricalDebugOptionsWindow>(
                true,
                "Categorical Debug",
                true);
        }

        void OnGUI()
        {
            CategoricalDebugOptionsGUI.DisplayCatDebugPrefs();
        }
    }

}//end namespace 
