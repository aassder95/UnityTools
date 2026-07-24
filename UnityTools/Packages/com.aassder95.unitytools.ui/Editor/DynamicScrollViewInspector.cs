using UnityEditor;
using UnityEngine;
using UnityTools.Ui;

namespace UnityTools.Ui.Editor
{
    [CustomEditor(typeof(DynamicScrollViewBase), true)]
    [CanEditMultipleObjects]
    public class DynamicScrollViewInspector : UnityEditor.Editor
    {
        //============================================================
        // Constants
        //============================================================
        private const string ITEM_PROP = "_item";
        private const string AXIS_PROP = "_axisType";
        private const string LAYOUT_PROP = "_layoutMode";
        private const string FIXED_CELLS_PROP = "_fixedCellsPerGroup";
        private const string MIN_VISIBLE_LINE_CNT_PROP = "_minVisibleLineCnt";
        private const string MOVEMENT_PROP = "_movementType";
        private const string INERTIA_PROP = "_isInertia";
        private const string DECELERATION_PROP = "_decelerationRate";
        private const string ELASTICITY_PROP = "_elasticity";
        private const string ALIGNMENT_PROP = "_contentAlignment";
        private const string SPACING_PROP = "_spacing";
        private const string PADDING_PROP = "_padding";

        //============================================================
        // Logic
        //============================================================
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawScriptField();
            DrawProperty(ITEM_PROP);
            DrawProperty(AXIS_PROP);

            SerializedProperty layoutProperty = serializedObject.FindProperty(LAYOUT_PROP);
            if (layoutProperty != null)
            {
                EditorGUILayout.PropertyField(layoutProperty);
                EDynamicScrollLayoutMode layoutMode = (EDynamicScrollLayoutMode)layoutProperty.enumValueIndex;
                if (layoutMode == EDynamicScrollLayoutMode.FixedCnt)
                    DrawProperty(FIXED_CELLS_PROP);
            }

            DrawProperty(MIN_VISIBLE_LINE_CNT_PROP);
            DrawProperty(MOVEMENT_PROP);
            DrawProperty(INERTIA_PROP);
            DrawProperty(DECELERATION_PROP);
            DrawProperty(ELASTICITY_PROP);
            DrawProperty(ALIGNMENT_PROP);
            DrawProperty(SPACING_PROP);
            DrawProperty(PADDING_PROP);
            serializedObject.ApplyModifiedProperties();
        }

        //============================================================
        // Utilities
        //============================================================
        private void DrawScriptField()
        {
            MonoBehaviour monoBehaviour = target as MonoBehaviour;
            if (monoBehaviour == null)
                return;

            using (new EditorGUI.DisabledScope(true))
            {
                MonoScript script = MonoScript.FromMonoBehaviour(monoBehaviour);
                EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
            }
        }

        private void DrawProperty(string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
                return;

            EditorGUILayout.PropertyField(property);
        }
    }
}
