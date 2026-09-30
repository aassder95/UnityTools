using UnityEngine;
using UnityTools.Ui;

namespace UnityTools.Benchmark.Samples
{
    public class UiLabScroll : DynamicScrollView<UiLabItem>
    {
        //============================================================
        // Properties
        //============================================================
        public int LiveItemCnt => ItemController.Cnt;
        public int CreatedItemCnt => RtContent.childCount;

        //============================================================
        // Logic
        //============================================================
        public void SetInputEnabled(bool isEnabled)
        {
            ScrollRect.StopMovement();
            ScrollRect.enabled = isEnabled;
        }
    }
}
