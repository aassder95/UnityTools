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
    }
}
