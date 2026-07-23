
using UnityTools.Util.UIFramework;

namespace UnityTools.Samples.Rank
{
    public class RankScrollView : DynamicScrollView<RankItemView>
    {
        //============================================================
        // Logic
        //============================================================
        public bool TryRefreshItems()
        {
            return ItemController.TryUpdateItems();
        }

        public bool TryIncreaseVisibleLine()
        {
            if(VisibleLineCnt >= TotalLineCnt)
                return true;

            return TrySetVisibleLineCnt(VisibleLineCnt + 1);
        }

        public bool TryDecreaseVisibleLine()
        {
            if(VisibleLineCnt <= 1)
                return true;

            return TrySetVisibleLineCnt(VisibleLineCnt - 1);
        }
    }
}
