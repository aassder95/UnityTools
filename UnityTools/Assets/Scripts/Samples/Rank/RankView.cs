using UnityEngine;
using UnityEngine.Events;
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
    public class RankView : BaseView<RankModel>
    {
        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private RankScrollView _scrollView;

        //============================================================
        //Events
        //============================================================
        public event UnityAction OnRandomScore { add => _onRandomScore += value; remove => _onRandomScore -= value; }
        public event UnityAction OnIncreaseTotalItem { add => _onIncreaseTotalItem += value; remove => _onIncreaseTotalItem -= value; }
        public event UnityAction OnDecreaseTotalItem { add => _onDecreaseTotalItem += value; remove => _onDecreaseTotalItem -= value; }
        public event UnityAction OnIncreaseVisibleLine { add => _onIncreaseVisibleLine += value; remove => _onIncreaseVisibleLine -= value; }
        public event UnityAction OnDecreaseVisibleLine { add => _onDecreaseVisibleLine += value; remove => _onDecreaseVisibleLine -= value; }
        private event UnityAction _onRandomScore;
        private event UnityAction _onIncreaseTotalItem;
        private event UnityAction _onDecreaseTotalItem;
        private event UnityAction _onIncreaseVisibleLine;
        private event UnityAction _onDecreaseVisibleLine;

        //============================================================
        //Properties
        //============================================================
        public RankScrollView ScrollView => _scrollView;

        //============================================================
        //Logic
        //============================================================
        protected override void OnRefresh(RankModel model)
        {
            if (_scrollView == null)
                return;

            _scrollView.UpdateItemView();
        }

        //============================================================
        //Callbacks
        //============================================================
        public void OnRandomScoreInspector()
        {
            _onRandomScore?.Invoke();
        }

        public void OnIncreaseTotalItemInspector()
        {
            _onIncreaseTotalItem?.Invoke();
        }

        public void OnDecreaseTotalItemInspector()
        {
            _onDecreaseTotalItem?.Invoke();
        }

        public void OnIncreaseVisibleLineInspector()
        {
            _onIncreaseVisibleLine?.Invoke();
        }

        public void OnDecreaseVisibleLineInspector()
        {
            _onDecreaseVisibleLine?.Invoke();
        }
    }
}
