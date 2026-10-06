using System.Collections.Generic;

namespace UnityTools.Sheets
{
    public class CsvTable
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly IReadOnlyList<string> _headers;
        private readonly IReadOnlyList<CsvRow> _rows;

        //============================================================
        // Properties
        //============================================================
        public IReadOnlyList<string> Headers => _headers;
        public IReadOnlyList<CsvRow> Rows => _rows;

        //============================================================
        // Constructors
        //============================================================
        public CsvTable(IEnumerable<string> headers, IEnumerable<CsvRow> rows)
        {
            _headers = new List<string>(headers).AsReadOnly();
            _rows = new List<CsvRow>(rows).AsReadOnly();
        }
    }
}
