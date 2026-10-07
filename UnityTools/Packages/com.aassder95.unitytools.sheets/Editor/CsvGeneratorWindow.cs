using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Sheets.Editor
{
    public class CsvGeneratorWindow : EditorWindow
    {
        //============================================================
        // Fields
        //============================================================
        [SerializeField] private TextAsset _csv;
        [SerializeField] private CsvGeneratorPreset _preset;
        [SerializeField] private string _outputPath = string.Empty;
        private CsvTable _table;
        private string _keyHeader = string.Empty;
        private readonly List<CsvValueRule> _rules = new List<CsvValueRule>();
        private readonly List<CsvReferenceRule> _references = new List<CsvReferenceRule>();
        private readonly List<CsvValidationIssue> _issues = new List<CsvValidationIssue>();
        private readonly List<CsvColumn> _columns = new List<CsvColumn>();
        private readonly List<string> _enumNames = new List<string>();
        private string _namespaceName = "Game.Data";
        private string _className = "ItemData";
        private string _source;
        private string _error = string.Empty;
        private Vector2 _scrollPos;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnEnable()
        {
            if (_preset != null)
                LoadPreset();
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            _preset = (CsvGeneratorPreset)EditorGUILayout.ObjectField("Preset", _preset, typeof(CsvGeneratorPreset), false);
            if (EditorGUI.EndChangeCheck() && _preset != null)
                LoadPreset();

            EditorGUI.BeginChangeCheck();
            _csv = (TextAsset)EditorGUILayout.ObjectField("CSV", _csv, typeof(TextAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                _preset = null;
                _outputPath = string.Empty;
                _issues.Clear();
                _references.Clear();
                _rules.Clear();
                _keyHeader = string.Empty;
                _table = null;
                _columns.Clear();
                _enumNames.Clear();
                _source = null;
                _error = string.Empty;
            }

            using (new EditorGUI.DisabledScope(_csv == null))
            {
                if (GUILayout.Button("Read CSV"))
                    ReadCsv();
            }

            if (_table != null)
            {
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                EditorGUILayout.LabelField(_table.Rows.Count + " rows / " + _table.Headers.Count + " columns");
                EditorGUI.BeginChangeCheck();
                _namespaceName = EditorGUILayout.TextField("Namespace", _namespaceName);
                _className = EditorGUILayout.TextField("Class", _className);
                for (int idx = 0; idx < _columns.Count; idx++)
                {
                    CsvColumn column = _columns[idx];
                    string name;
                    ECsvColumnType type;
                    bool isArray;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(column.Header, GUILayout.Width(160.0f));
                        name = EditorGUILayout.TextField(column.Name);
                        type = (ECsvColumnType)EditorGUILayout.EnumPopup(column.Type, GUILayout.Width(90.0f));
                        isArray = GUILayout.Toggle(column.IsArray, "Array", GUILayout.Width(60.0f));
                    }

                    string enumName = _enumNames[idx];
                    if (type == ECsvColumnType.Enum)
                        enumName = EditorGUILayout.TextField(new GUIContent("Enum", "public top-level enum의 전체 이름, 예: Game.Data.EGrade"), enumName);

                    if (name != column.Name || type != column.Type || enumName != _enumNames[idx] || isArray != column.IsArray)
                    {
                        _enumNames[idx] = enumName;
                        Type enumType = CsvEnumResolver.Resolve(enumName);
                        _columns[idx] = new CsvColumn(column.Header, name, type, enumType, isArray);
                    }
                }

                if (EditorGUI.EndChangeCheck())
                {
                    _source = null;
                    _issues.Clear();
                }

                DrawConstraints();

                if (GUILayout.Button("Save preset"))
                    SavePreset();

                if (GUILayout.Button("Validate & Preview"))
                {
                    ValidateSource();
                }

                if (_source != null)
                {
                    if (GUILayout.Button("Save C#"))
                        SaveSource();

                    EditorGUILayout.TextArea(_source);
                }

                for (int idx = 0; idx < _issues.Count; idx++)
                {
                    CsvValidationIssue issue = _issues[idx];
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(issue.LineNum + "행 / " + issue.Header, GUILayout.Width(160.0f));
                        EditorGUILayout.LabelField(issue.Message, EditorStyles.wordWrappedLabel);
                        if (GUILayout.Button("Open", GUILayout.Width(50.0f)))
                            AssetDatabase.OpenAsset(_csv, Math.Max(1, issue.LineNum));
                    }
                }

                EditorGUILayout.EndScrollView();
            }

            if (!string.IsNullOrEmpty(_error))
                EditorGUILayout.HelpBox(_error, MessageType.Error);
        }

        //============================================================
        // Logic
        //============================================================
        [MenuItem("Tools/UnityTools/CSV Generator")]
        public static void Open()
        {
            CsvGeneratorWindow window = GetWindow<CsvGeneratorWindow>("CSV Generator");
            window.minSize = new Vector2(600.0f, 400.0f);
            window.Show();
        }

        private void ReadCsv()
        {
            _issues.Clear();
            _source = null;
            _columns.Clear();
            _enumNames.Clear();
            if (!CsvParser.TryParse(_csv.text, out _table, out _error))
                return;

            for (int idx = 0; idx < _table.Headers.Count; idx++)
            {
                _columns.Add(new CsvColumn(_table.Headers[idx], "Column" + (idx + 1), ECsvColumnType.String));
                _enumNames.Add(string.Empty);
            }

            if (_preset != null && _preset.Csv == _csv)
            {
                if (_preset.TryRestore(_table, _columns, _enumNames, out _error))
                {
                    _namespaceName = _preset.NamespaceName;
                    _className = _preset.ClassName;
                    _outputPath = _preset.OutputPath;
                    _keyHeader = _preset.KeyHeader;
                    _references.Clear();
                    _references.AddRange(_preset.References);
                    _rules.Clear();
                    _rules.AddRange(_preset.Rules);
                }
            }
        }

        private void DrawConstraints()
        {
            EditorGUI.BeginChangeCheck();
            _keyHeader = EditorGUILayout.TextField("Unique key header", _keyHeader);
            for (int idx = 0; idx < _references.Count; idx++)
            {
                CsvReferenceRule rule = _references[idx];
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    string header = EditorGUILayout.TextField("Source header", rule.Header);
                    TextAsset target = (TextAsset)EditorGUILayout.ObjectField("Target CSV", rule.TargetCsv, typeof(TextAsset), false);
                    string targetHeader = EditorGUILayout.TextField("Target key header", rule.TargetHeader);
                    bool isArray = EditorGUILayout.Toggle("Pipe array", rule.IsArray);
                    if (header != rule.Header || target != rule.TargetCsv || targetHeader != rule.TargetHeader || isArray != rule.IsArray)
                        _references[idx] = new CsvReferenceRule(header, target, targetHeader, isArray);

                    if (GUILayout.Button("Remove reference"))
                    {
                        GUI.changed = true;
                        _references.RemoveAt(idx);
                        idx--;
                    }
                }
            }

            if (GUILayout.Button("Add reference"))
            {
                GUI.changed = true;
                _references.Add(new CsvReferenceRule(_table.Headers[0], null, "Id", false));
            }

            for (int idx = 0; idx < _rules.Count; idx++)
            {
                CsvValueRule rule = _rules[idx];
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    string header = EditorGUILayout.TextField("Rule header", rule.Header);
                    ECsvRule kind = (ECsvRule)EditorGUILayout.EnumPopup("Rule", rule.Kind);
                    double min = rule.Min;
                    double max = rule.Max;
                    if (kind != ECsvRule.Required)
                    {
                        min = EditorGUILayout.DoubleField("Min (inclusive)", min);
                        max = EditorGUILayout.DoubleField("Max (inclusive)", max);
                    }

                    if (header != rule.Header || kind != rule.Kind || min != rule.Min || max != rule.Max)
                        _rules[idx] = new CsvValueRule(header, kind, min, max);

                    if (GUILayout.Button("Remove rule"))
                    {
                        GUI.changed = true;
                        _rules.RemoveAt(idx);
                        idx--;
                    }
                }
            }

            if (GUILayout.Button("Add value rule"))
            {
                GUI.changed = true;
                _rules.Add(new CsvValueRule(_table.Headers[0], ECsvRule.Required));
            }

            if (EditorGUI.EndChangeCheck())
            {
                _source = null;
                _issues.Clear();
            }
        }

        private void ValidateSource()
        {
            _source = null;
            _issues.Clear();
            _issues.AddRange(CsvCodeGenerator.Validate(_table, _columns, _namespaceName, _className));
            CsvConstraintValidator.Validate(_table, _keyHeader, _references, _issues);
            CsvRuleValidator.Validate(_table, _rules, _issues);
            _error = string.Empty;
            if (_issues.Count > 0)
                return;

            bool isGenerated = CsvCodeGenerator.TryGenerate(_table, _columns, _namespaceName, _className, out _source, out _error);
            if (!isGenerated)
                _source = null;
        }

        private void LoadPreset()
        {
            _keyHeader = string.Empty;
            _references.Clear();
            _rules.Clear();
            _csv = _preset.Csv;
            _source = null;
            _table = null;
            _columns.Clear();
            _enumNames.Clear();
            _error = string.Empty;
            if (_csv != null)
                ReadCsv();
        }

        private void SavePreset()
        {
            if (_preset == null)
            {
                string path = EditorUtility.SaveFilePanelInProject("Save CSV preset", _className + "Preset", "asset", "프리셋 저장 경로를 선택하세요.");
                if (string.IsNullOrEmpty(path))
                    return;

                if (AssetDatabase.LoadMainAssetAtPath(path) != null)
                {
                    _error = "기존 에셋은 Preset 필드에서 선택하세요.";
                    return;
                }

                _preset = CreateInstance<CsvGeneratorPreset>();
                AssetDatabase.CreateAsset(_preset, path);
            }

            Undo.RecordObject(_preset, "Save CSV preset");
            _preset.Capture(_csv, _columns, _enumNames, _namespaceName, _className, _outputPath, _keyHeader, _references, _rules);
            EditorUtility.SetDirty(_preset);
            AssetDatabase.SaveAssets();
            _error = string.Empty;
        }

        private void SaveSource()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save generated C#", _className, "cs", "생성할 C# 파일 경로를 선택하세요.", string.IsNullOrEmpty(_outputPath) ? "Assets" : Path.GetDirectoryName(_outputPath));
            if (string.IsNullOrEmpty(path))
                return;

            if (Path.GetFileNameWithoutExtension(path) != _className)
            {
                _error = "파일 이름은 클래스 이름과 같아야 합니다.";
                return;
            }

            try
            {
                File.WriteAllText(path, _source, new UTF8Encoding(false));
                AssetDatabase.ImportAsset(path);
                _outputPath = path;
                if (_preset != null)
                    SavePreset();
                _error = string.Empty;
            }
            catch (IOException ex)
            {
                _error = "파일 저장에 실패했습니다: " + ex.Message;
            }
            catch (UnauthorizedAccessException ex)
            {
                _error = "파일 저장 권한이 없습니다: " + ex.Message;
            }
        }
    }
}
