using System;
using UnityEngine;

namespace UnityTools.Persistence.Samples
{
    [Serializable]
    public class SaveLabV1
    {
        //============================================================
        // Fields
        //============================================================
        [SerializeField] private int _level;

        //============================================================
        // Properties
        //============================================================
        public int Level => _level;

        //============================================================
        // Constructors
        //============================================================
        public SaveLabV1(int level)
        {
            _level = level;
        }
    }
}
