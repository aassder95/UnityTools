using UnityTools.Util;

namespace UnityTools.UI
{
    //============================================================
    //Logic
    //============================================================
    public class RankScrollView : DynamicScrollView<RankItemView>
    {
        public void UpdateItemView()
        {
            if(ItemController == null)
                return;

            ItemController.Update();
        }

        public void IncreaseTotalItem()
        {
            SetTotalItemCount(TotalItemCount + 1);
        }

        public void DecreaseTotalItem()
        {
            if(TotalItemCount <= 0)
                return;

            SetTotalItemCount(TotalItemCount - 1);
        }

        public void IncreaseVisibleLine()
        {
            if(VisibleLineCount >= TotalLineCount)
                return;

            SetVisibleLineCount(VisibleLineCount + 1);
        }

        public void DecreaseVisibleLine()
        {
            if(VisibleLineCount <= 1)
                return;

            SetVisibleLineCount(VisibleLineCount - 1);
        }
    }
}
