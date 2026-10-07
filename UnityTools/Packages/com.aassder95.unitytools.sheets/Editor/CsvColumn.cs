namespace UnityTools.Sheets.Editor
{
    public class CsvColumn
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly string _header;
        private readonly string _name;
        private readonly ECsvColumnType _type;
        private readonly System.Type _enumType;
        private readonly bool _isArray;

        //============================================================
        // Properties
        //============================================================
        public string Header => _header;
        public string Name => _name;
        public ECsvColumnType Type => _type;
        public System.Type EnumType => _enumType;
        public bool IsArray => _isArray;

        //============================================================
        // Constructors
        //============================================================
        public CsvColumn(string header, string name, ECsvColumnType type)
            : this(header, name, type, null)
        {
        }

        public CsvColumn(string header, string name, ECsvColumnType type, System.Type enumType)
            : this(header, name, type, enumType, false)
        {
        }

        public CsvColumn(string header, string name, ECsvColumnType type, System.Type enumType, bool isArray)
        {
            _header = header;
            _name = name;
            _type = type;
            _enumType = enumType;
            _isArray = isArray;
        }
    }
}
