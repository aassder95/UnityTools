using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Sheets.Editor
{
    public class CsvBatchWindow : EditorWindow
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private CsvGeneratorPreset[] _presets = new CsvGeneratorPreset[0];

        //============================================================
        // Fields
        //============================================================
        private readonly List<string> _messages = new List<string>();
        private Vector2 _scrollPos;
        private bool _isPassed;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnGUI()
        {
            SerializedObject serialized = new SerializedObject(this);
            EditorGUILayout.PropertyField(serialized.FindProperty("_presets"), true);
            if (serialized.ApplyModifiedProperties())
                _messages.Clear();

            if (GUILayout.Button("Validate all"))
                _isPassed = CsvBatchGenerator.TryRun(_presets, false, _messages);

            if (GUILayout.Button("Generate all") && EditorUtility.DisplayDialog("일괄 생성", "모든 프리셋을 검증한 후 지정한 C# 파일을 덮어씁니다. 진행할까요?", "생성", "취소"))
                _isPassed = CsvBatchGenerator.TryRun(_presets, true, _messages);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            for (int idx = 0; idx < _messages.Count; idx++)
            {
                EditorGUILayout.HelpBox(_messages[idx], _isPassed ? MessageType.Info : MessageType.Error);
            }

            EditorGUILayout.EndScrollView();
        }

        //============================================================
        // Logic
        //============================================================
        [MenuItem("Tools/UnityTools/CSV Batch")]
        public static void Open()
        {
            GetWindow<CsvBatchWindow>("CSV Batch").Show();
        }
    }
}
