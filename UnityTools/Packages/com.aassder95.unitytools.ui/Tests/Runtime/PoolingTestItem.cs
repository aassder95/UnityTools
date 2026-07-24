using UnityEngine;
using UnityTools.Util.Core.Pooling;

namespace UnityTools.Util.Tests.Lifecycle
{
    public class PoolingTestItem : MonoBehaviour, IPoolable
    {
        //============================================================
        // Fields
        //============================================================
        private int _getCnt;
        private int _returnCnt;

        //============================================================
        // Properties
        //============================================================
        public int GetCnt => _getCnt;
        public int ReturnCnt => _returnCnt;

        //============================================================
        // Callbacks
        //============================================================
        public void OnGet()
        {
            _getCnt++;
        }

        public void OnReturn()
        {
            _returnCnt++;
        }
    }
}
