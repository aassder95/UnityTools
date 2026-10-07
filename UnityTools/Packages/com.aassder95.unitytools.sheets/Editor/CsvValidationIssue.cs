namespace UnityTools.Sheets.Editor
{
    public class CsvValidationIssue
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly int _lineNum;
        private readonly string _header;
        private readonly string _message;

        //============================================================
        // Properties
        //============================================================
        public int LineNum => _lineNum;
        public string Header => _header;
        public string Message => _message;

        //============================================================
        // Constructors
        //============================================================
        public CsvValidationIssue(int lineNum, string header, string message)
        {
            _lineNum = lineNum;
            _header = header;
            _message = message;
        }
    }
}
