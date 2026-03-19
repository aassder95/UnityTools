using UnityTools.Util;
using UnityTools.Util.Constants;
using UnityTools.Util.Core.Collections;
using UnityTools.Util.Core.Events;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Core.Persistence;
using UnityTools.Util.Core.Pooling;
using UnityTools.Util.Core.Singleton;
using UnityTools.Util.Core.State;
using UnityTools.Util.Core.Timer.Period;
using UnityTools.Util.Core.Timer.Shared;
using UnityTools.Util.Core.Timer.Task;
using UnityTools.Util.Coroutines;
using UnityTools.Util.Extensions;
using UnityTools.Util.UIFramework;
using UnityTools.Util.Utilities;

namespace UnityTools.Samples.Rank
{
    //============================================================
    //Logic
    //============================================================
    public class RankScrollView : DynamicScrollView<RankItemView>
    {
        //============================================================
        //Logic
        //============================================================
        public void UpdateItemView()
        {
            if(ItemController == null)
                return;

            ItemController.Update();
        }

        public void IncreaseTotalItem()
        {
            int nextTotalItemCount = TotalItemCount + 1;
            if(nextTotalItemCount <= TotalItemCount)
                return;

            SetTotalItemCount(nextTotalItemCount);
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
