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
    public class InvenView : BaseView<InvenModel>
    {
        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private InvenScrollView _scrollView;

        //============================================================
        //Properties
        //============================================================
        public InvenScrollView ScrollView => _scrollView;

        //============================================================
        //Logic
        //============================================================
        protected override void OnRefresh(InvenModel model)
        {
            if (_scrollView == null)
                return;

            _scrollView.UpdateItemView();
        }
    }
}
