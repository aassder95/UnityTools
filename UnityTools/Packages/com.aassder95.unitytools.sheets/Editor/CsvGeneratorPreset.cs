using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Sheets.Editor
{
    public class CsvGeneratorPreset : ScriptableObject
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [Header("CSV")]
        [SerializeField] private TextAsset _csv;
        [SerializeField] private List<CsvPresetColumn> _columns = new List<CsvPresetColumn>();
        [SerializeField] private string _keyHeader = string.Empty;
        [SerializeField] private List<CsvReferenceRule> _references = new List<CsvReferenceRule>();
        [SerializeField] private List<CsvValueRule> _rules = new List<CsvValueRule>();
        [Header("Output")]
        [SerializeField] private string _namespaceName;
        [SerializeField] private string _className;
        [SerializeField] private string _outputPath;

        //============================================================
        // Properties
        //============================================================
        public IReadOnlyList<CsvValueRule> Rules => _rules.AsReadOnly();
        public string KeyHeader => _keyHeader;
        public IReadOnlyList<CsvReferenceRule> References => _references.AsReadOnly();
        public TextAsset Csv => _csv;
        public string NamespaceName => _namespaceName;
        public string ClassName => _className;
        public string OutputPath => _outputPath;

        //============================================================
        // Logic
        //============================================================
        public void Capture(TextAsset csv, IReadOnlyList<CsvColumn> columns, IReadOnlyList<string> enumNames, string namespaceName, string className, string outputPath, string keyHeader = "", IReadOnlyList<CsvReferenceRule> references = null, IReadOnlyList<CsvValueRule> rules = null)
        {
            _rules.Clear();
            if (rules != null)
            {
                for (int idx = 0; idx < rules.Count; idx++)
                {
                    _rules.Add(rules[idx]);
                }
            }

            _keyHeader = keyHeader;
            _references.Clear();
            if (references != null)
            {
                for (int idx = 0; idx < references.Count; idx++)
                {
                    _references.Add(references[idx]);
                }
            }

            _csv = csv;
            _namespaceName = namespaceName;
            _className = className;
            _outputPath = outputPath;
            _columns.Clear();
            for (int idx = 0; idx < columns.Count; idx++)
            {
                _columns.Add(new CsvPresetColumn(columns[idx], enumNames[idx]));
            }
        }

        public bool TryRestore(CsvTable table, List<CsvColumn> columns, List<string> enumNames, out string error)
        {
            error = string.Empty;
            if (table == null || columns == null || enumNames == null || _columns == null || table.Headers.Count != _columns.Count)
            {
                error = "CSV 헤더 수가 프리셋과 다릅니다. 새 설정을 확인한 뒤 프리셋을 저장하세요.";
                return false;
            }

            for (int idx = 0; idx < _columns.Count; idx++)
            {
                if (_columns[idx] == null || table.Headers[idx] != _columns[idx].Header)
                {
                    error = (idx + 1) + "열: CSV 헤더 이름 또는 순서가 프리셋과 다릅니다.";
                    return false;
                }
            }

            columns.Clear();
            enumNames.Clear();
            for (int idx = 0; idx < _columns.Count; idx++)
            {
                columns.Add(_columns[idx].Restore());
                enumNames.Add(_columns[idx].EnumName);
            }

            return true;
        }
    }
}
