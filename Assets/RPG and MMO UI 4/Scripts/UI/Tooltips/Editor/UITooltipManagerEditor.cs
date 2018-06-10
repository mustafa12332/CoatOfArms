using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(UITooltipManager))]
    public class UITooltipManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This object must always be in the Resources folder in order to function correctly.", MessageType.Info);
            EditorGUILayout.Space();
            base.OnInspectorGUI();
        }

        private static string GetSavePath()
        {
            return EditorUtility.SaveFilePanelInProject("New tooltip manager", "New tooltip manager", "asset", "Create a new tooltip manager.");
        }

        [MenuItem("Assets/Create/Tooltip Manager")]
        public static void CreateManager()
        {
            string assetPath = GetSavePath();
            UITooltipManager asset = ScriptableObject.CreateInstance("UITooltipManager") as UITooltipManager;  //scriptable object
            AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(assetPath));
            AssetDatabase.Refresh();
        }
    }
}
