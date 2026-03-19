using System;
using UnityEngine;

namespace UnityTools.Samples.Templates
{
    public class TemplateSampleView : UnityTools.Util.BaseView<TemplateSampleModel>
    {
        //============================================================
        //Events
        //============================================================
        public event Action OnIncreaseClicked { add => _onIncreaseClicked += value; remove => _onIncreaseClicked -= value; }
        private event Action _onIncreaseClicked;

        //============================================================
        //Logic
        //============================================================
        protected override void OnRefresh(TemplateSampleModel model)
        {
        }

        //============================================================
        //Callbacks
        //============================================================
        public void OnIncreaseClickedInspector()
        {
            _onIncreaseClicked?.Invoke();
        }
    }
}
