using System;
using UnityEngine;

namespace UnityTools.Sheets.Editor
{
    [Serializable]
    public class CsvValueRule
    {
        //============================================================
        // Fields
        //============================================================
        [SerializeField] private string _header;
        [SerializeField] private ECsvRule _kind;
        [SerializeField] private double _min;
        [SerializeField] private double _max;

        //============================================================
        // Properties
        //============================================================
        public string Header => _header;
        public ECsvRule Kind => _kind;
        public double Min => _min;
        public double Max => _max;

        //============================================================
        // Constructors
        //============================================================
        public CsvValueRule(string header, ECsvRule kind, double min = 0.0, double max = 0.0)
        {
            _header = header;
            _kind = kind;
            _min = min;
            _max = max;
        }
    }
}
