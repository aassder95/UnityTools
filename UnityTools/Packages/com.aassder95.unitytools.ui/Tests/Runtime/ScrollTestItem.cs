using UnityEngine;
using UnityTools.Util.Core.Pooling;
using UnityTools.Util.UIFramework;

namespace UnityTools.Util.Tests.Lifecycle
{
    public class ScrollTestItem : MonoBehaviour, IDynamicScrollItem, IPoolable
    {
        //============================================================
        // Fields
        //============================================================
        private int _idx;

        //============================================================
        // Properties
        //============================================================
        public int Idx => _idx;

        //============================================================
        // Init/Register
        //============================================================
        public void Init() { }

        //============================================================
        // Logic
        //============================================================
        public void SetIdx(int idx)
        {
            _idx = idx;
        }

        public void SetPos(Vector2 pos)
        {
            RectTransform rt = transform as RectTransform;
            rt.anchoredPosition = pos;
        }

        //============================================================
        // Callbacks
        //============================================================
        public void OnGet() { }
        public void OnReturn() { }
    }
}
