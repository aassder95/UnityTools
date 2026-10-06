using UnityEngine;
using UnityTools.Ui;

namespace UnityTools.Ui.Tests.Lifecycle
{
    public class ScrollTestView : DynamicScrollView<ScrollTestItem>
    {
        //============================================================
        // Properties
        //============================================================
        public int ItemCnt => TotalItemCnt;
        public Vector2 ContentPos => RtContent.anchoredPosition;
        public float ContentHeight => RtContent.sizeDelta.y;

        //============================================================
        // Logic
        //============================================================
        public void SetContentPos(Vector2 pos)
        {
            RtContent.anchoredPosition = pos;
            ScrollRect.onValueChanged.Invoke(Vector2.zero);
        }
    }
}
