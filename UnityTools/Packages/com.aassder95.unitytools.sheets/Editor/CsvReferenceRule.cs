using System;
using UnityEngine;

namespace UnityTools.Sheets.Editor
{
    [Serializable]
    public class CsvReferenceRule
    {
        //============================================================
        // Fields
        //============================================================
        [SerializeField] private string _header;
        [SerializeField] private TextAsset _targetCsv;
        [SerializeField] private string _targetHeader;
        [SerializeField] private bool _isArray;

        //============================================================
        // Properties
        //============================================================
        public string Header => _header;
        public TextAsset TargetCsv => _targetCsv;
        public string TargetHeader => _targetHeader;
        public bool IsArray => _isArray;

        //============================================================
        // Constructors
        //============================================================
        public CsvReferenceRule(string header, TextAsset targetCsv, string targetHeader, bool isArray)
        {
            _header = header;
            _targetCsv = targetCsv;
            _targetHeader = targetHeader;
            _isArray = isArray;
        }
    }
}
