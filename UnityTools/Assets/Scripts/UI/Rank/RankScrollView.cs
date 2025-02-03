using UnityTools.Util;

namespace UnityTools.UI
{
    public class RankScrollView : DynamicScrollView<RankItemView>
    {
        public void IncreaseTotalItem() => SetTotalItemCount(TotalItemCount + 1);
        public void DecreaseTotalItem() => SetTotalItemCount(TotalItemCount - 1);
        public void IncreaseVisibleLine() => SetVisibleLineCount(VisibleLineCount + 1);
        public void DecreaseVisibleLine() => SetVisibleLineCount(VisibleLineCount - 1);
    }
}