using UnityEngine;
using UnityTools.Ui;

namespace UnityTools.Ui.Tests.Lifecycle
{
    public class ScrollTestItem : MonoBehaviour, IDynamicScrollItem
    {
        //============================================================
        // Fields
        //============================================================
        private int _idx;
        private int _updateCnt;
        private int _getCnt;
        private int _returnCnt;

        //============================================================
        // Properties
        //============================================================
        public int Idx => _idx;
        public int UpdateCnt => _updateCnt;
        public int GetCnt => _getCnt;
        public int ReturnCnt => _returnCnt;

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

        public void MarkUpdated()
        {
            _updateCnt++;
        }

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
