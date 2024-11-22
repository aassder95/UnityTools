using UnityTools.Util;

namespace UnityTools.UI
{
    public class RankScrollView : DynamicScrollView<RankItemView>
    {
        public void IncreaseTotalItem() => SetTotalCount(_totalCnt + 1);
        public void DecreaseTotalItem() => SetTotalCount(_totalCnt - 1);
        public void IncreaseVisibleItem() => SetVisibleCount(_visibleCnt + 1);
        public void DecreaseVisibleItem() => SetVisibleCount(_visibleCnt - 1);
    }
}