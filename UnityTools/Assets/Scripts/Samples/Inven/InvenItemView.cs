using TMPro;
using UnityEngine;
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

namespace UnityTools.Samples.Inven
{
    public class InvenItemView : BaseView<InvenItemModel>, IDynamicScrollItem, IPoolable
    {
        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private RectTransform _rtView;
        [SerializeField] private TextMeshProUGUI _txtIndex;

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
        protected override void OnRefresh(InvenItemModel model)
        {
            _txtIndex.SetText("{0}", model.Id);
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
