namespace UnityTools.Sheets.Editor
{
    public class CsvChange
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly ECsvChange _kind;
        private readonly string _key;
        private readonly string _header;
        private readonly string _before;
        private readonly string _after;

        //============================================================
        // Properties
        //============================================================
        public ECsvChange Kind => _kind;
        public string Key => _key;
        public string Header => _header;
        public string Before => _before;
        public string After => _after;

        //============================================================
        // Constructors
        //============================================================
        public CsvChange(ECsvChange kind, string key, string header, string before, string after)
        {
            _kind = kind;
            _key = key;
            _header = header;
            _before = before;
            _after = after;
        }
    }
}
