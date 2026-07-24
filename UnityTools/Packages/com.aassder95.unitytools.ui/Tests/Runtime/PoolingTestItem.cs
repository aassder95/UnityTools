using UnityEngine;
using UnityTools.Util.Core.Pooling;

namespace UnityTools.Util.Tests.Lifecycle
{
    public class PoolingTestItem : MonoBehaviour, IPoolable
    {
        //============================================================
        // Fields
        //============================================================
        private int _getCallCnt;
        private int _returnCallCnt;

        //============================================================
        // Properties
        //============================================================
        public int GetCallCnt => _getCallCnt;
        public int ReturnCallCnt => _returnCallCnt;

        //============================================================
        // Callbacks
        //============================================================
        public void OnGet()
        {
            _getCallCnt++;
        }

        public void OnReturn()
        {
            _returnCallCnt++;
        }
    }
}
