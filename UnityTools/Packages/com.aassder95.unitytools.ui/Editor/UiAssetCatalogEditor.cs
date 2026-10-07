using UnityEditor;
using UnityEngine;
using UnityTools.Ui.Assets;

namespace UnityTools.Ui.Editor
{
    [CustomEditor(typeof(UiAssetCatalog))]
    public class UiAssetCatalogEditor : UnityEditor.Editor
    {
        //============================================================
        // Unity Methods
        //============================================================
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            bool isValid = ((UiAssetCatalog)target).TryValidate(out string error);
            EditorGUILayout.HelpBox(isValid ? "키와 참조 검증을 통과했습니다." : error, isValid ? MessageType.Info : MessageType.Warning);
        }
    }
}
