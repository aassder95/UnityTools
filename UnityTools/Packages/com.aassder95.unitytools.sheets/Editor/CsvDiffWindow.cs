using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Sheets.Editor
{
    public class CsvDiffWindow : EditorWindow
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private TextAsset _before;
        [SerializeField] private TextAsset _after;
        [SerializeField] private string _keyHeader = "Id";

        //============================================================
        // Fields
        //============================================================
        private IReadOnlyList<CsvChange> _changes;
        private string _error = string.Empty;
        private Vector2 _scrollPos;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            _before = (TextAsset)EditorGUILayout.ObjectField("Before", _before, typeof(TextAsset), false);
            _after = (TextAsset)EditorGUILayout.ObjectField("After", _after, typeof(TextAsset), false);
            _keyHeader = EditorGUILayout.TextField("Key header", _keyHeader);
            if (EditorGUI.EndChangeCheck())
                _changes = null;

            if (GUILayout.Button("Compare"))
                Compare();

            if (!string.IsNullOrEmpty(_error))
                EditorGUILayout.HelpBox(_error, MessageType.Error);

            if (_changes == null)
                return;

            EditorGUILayout.LabelField(_changes.Count + " cell / header changes");
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            for (int idx = 0; idx < _changes.Count; idx++)
            {
                CsvChange change = _changes[idx];
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(change.Kind + " / " + change.Key + " / " + change.Header);
                    EditorGUILayout.LabelField("Before: " + change.Before, EditorStyles.wordWrappedLabel);
                    EditorGUILayout.LabelField("After: " + change.After, EditorStyles.wordWrappedLabel);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        //============================================================
        // Logic
        //============================================================
        [MenuItem("Tools/UnityTools/CSV Diff")]
        public static void Open()
        {
            GetWindow<CsvDiffWindow>("CSV Diff").Show();
        }

        private void Compare()
        {
            _changes = null;
            _error = string.Empty;
            if (_before == null || _after == null)
            {
                _error = "비교할 CSV 두 개를 연결하세요.";
                return;
            }

            if (!CsvParser.TryParse(_before.text, out CsvTable before, out _error) || !CsvParser.TryParse(_after.text, out CsvTable after, out _error))
                return;

            bool isCompared = CsvTableDiff.TryCompare(before, after, _keyHeader, out _changes, out _error);
            if (!isCompared)
                _changes = null;
        }
    }
}
