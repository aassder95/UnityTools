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

        public void IncreaseVisibleLine()
        {
            if(VisibleLineCnt >= TotalLineCnt)
                return;

            SetVisibleLineCnt(VisibleLineCnt + 1);
        }

        public void DecreaseVisibleLine()
        {
            if(VisibleLineCnt <= 1)
                return;

            SetVisibleLineCnt(VisibleLineCnt - 1);
        }
    }
}
