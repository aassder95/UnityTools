using UnityTools.Util.UIFramework;

namespace UnityTools.Util.Tests.Lifecycle
{
    public class ScrollTestView : DynamicScrollView<ScrollTestItem>
    {
        //============================================================
        // Properties
        //============================================================
        public int ItemCnt => TotalItemCnt;
    }
}
