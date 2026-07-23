using UnityTools.Util.UIFramework;

namespace UnityTools.Samples.Rank
{
    public class RankScrollView : DynamicScrollView<RankItemView>
    {
        //============================================================
        // Logic
        //============================================================
        public void RefreshItems()
        {
            ItemController.UpdateItems();
        }

        public bool IncreaseVisibleLine()
        {
            if(VisibleLineCnt >= TotalLineCnt)
                return true;

            return TrySetVisibleLineCnt(VisibleLineCnt + 1);
        }

        public bool DecreaseVisibleLine()
        {
            if(VisibleLineCnt <= 1)
                return true;

            return TrySetVisibleLineCnt(VisibleLineCnt - 1);
        }
    }
}
