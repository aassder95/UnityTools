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

        //============================================================
        // Properties
        //============================================================
        public string Header => _header;
        public string Name => _name;
        public ECsvColumnType Type => _type;

        //============================================================
        // Constructors
        //============================================================
        public CsvColumn(string header, string name, ECsvColumnType type)
        {
            _header = header;
            _name = name;
            _type = type;
        }
    }
}
