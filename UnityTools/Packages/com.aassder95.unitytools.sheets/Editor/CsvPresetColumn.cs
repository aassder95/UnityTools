using System;
using UnityEngine;

namespace UnityTools.Sheets.Editor
{
    [Serializable]
    public class CsvPresetColumn
    {
        //============================================================
        // Fields
        //============================================================
        [SerializeField] private string _header;
        [SerializeField] private string _name;
        [SerializeField] private ECsvColumnType _type;
        [SerializeField] private string _enumName;
        [SerializeField] private bool _isArray;

        //============================================================
        // Properties
        //============================================================
        public string Header => _header;
        public string EnumName => _enumName;

        //============================================================
        // Constructors
        //============================================================
        public CsvPresetColumn(CsvColumn column, string enumName)
        {
            _header = column.Header;
            _name = column.Name;
            _type = column.Type;
            _enumName = enumName;
            _isArray = column.IsArray;
        }

        //============================================================
        // Logic
        //============================================================
        public CsvColumn Restore()
        {
            return new CsvColumn(_header, _name, _type, CsvEnumResolver.Resolve(_enumName), _isArray);
        }
    }
}
