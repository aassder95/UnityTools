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
        public override void Refresh(TemplateSampleModel model)
        {
            if (model == null)
                return;

            base.Refresh(model);
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
