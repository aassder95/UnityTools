using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UnityTools.Sheets
{
    public class CsvRow
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly int _lineNum;
        private readonly IReadOnlyDictionary<string, string> _cells;

        //============================================================
        // Properties
        //============================================================
        public int LineNum => _lineNum;
        public IReadOnlyDictionary<string, string> Cells => _cells;

        //============================================================
        // Constructors
        //============================================================
        public CsvRow(int lineNum, IDictionary<string, string> cells)
        {
            _lineNum = lineNum;
            _cells = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(cells, System.StringComparer.Ordinal));
        }

        //============================================================
        // Logic
        //============================================================
        public bool TryGetCell(string header, out string value)
        {
            value = null;
            return header != null && _cells.TryGetValue(header, out value);
        }
    }
}
