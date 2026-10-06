using UnityEditor;
using UnityEditor.UI;

namespace UnityTools.Ui.Editor
{
    [CustomEditor(typeof(UiRepeatButton)), CanEditMultipleObjects]
    public class UiRepeatButtonInspector : ButtonEditor
    {
        //============================================================
        // Logic
        //============================================================
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_initialDelaySec"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_intervalSec"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_minIntervalSec"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_acceleration"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
