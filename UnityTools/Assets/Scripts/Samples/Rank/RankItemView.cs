using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
    public class RankItemView : BaseView<RankItemModel>, IDynamicScrollItem, IPoolable 
    {
        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private TextMeshProUGUI _txtId;
        [SerializeField] private TextMeshProUGUI _txtRank;
        [SerializeField] private TextMeshProUGUI _txtScore;
        [SerializeField] private Image _imgTmp;
        [SerializeField] private RectTransform _rtView;

        //============================================================
        //Fields
        //============================================================
        private int _index;

        //============================================================
        //Properties
        //============================================================
        public int Index => _index;

        //============================================================
        //Logic
        //============================================================
        protected override void OnRefresh(RankItemModel model)
        {
            _txtId.SetText("{0}", model.Id);
            _txtRank.SetText("{0}", model.Rank);
            _txtScore.SetText("{0}", model.Score);
            _imgTmp.color = model.BgColor;
        }

        //============================================================
        //Callbacks
        //============================================================
        void IDynamicScrollItem.SetIndex(int index)
        {
            _index = index;
        }

        int IDynamicScrollItem.GetIndex()
        {
            return _index;
        }

        void IDynamicScrollItem.SetPosition(Vector2 pos)
        {
            _rtView.anchoredPosition = pos;
        }

        //============================================================
        //Utilities
        //============================================================
        void IPoolable.OnGet() { }
        void IPoolable.OnReturn() { }
    }
}
