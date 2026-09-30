using System;
using UnityEngine;

namespace UnityTools.Persistence.Samples
{
    [Serializable]
    public class SaveLabV2
    {
        //============================================================
        // Fields
        //============================================================
        [SerializeField] private int _level;
        [SerializeField] private int _branchCnt;

        //============================================================
        // Properties
        //============================================================
        public int Level => _level;
        public int BranchCnt => _branchCnt;

        //============================================================
        // Constructors
        //============================================================
        public SaveLabV2(int level, int branchCnt)
        {
            _level = level;
            _branchCnt = branchCnt;
        }
    }
}
