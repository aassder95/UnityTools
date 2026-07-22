using System;
using UnityTools.Util.UIFramework;

namespace UnityTools.Samples.Templates
{
    public class TemplateSampleView : BaseView<TemplateSampleModel>
    {
        //============================================================
        // Events
        //============================================================
        public event Action OnIncreaseClicked { add => _onIncreaseClicked += value; remove => _onIncreaseClicked -= value; }
        private event Action _onIncreaseClicked;

        //============================================================
        // Logic
        //============================================================
        protected override void OnRefresh(TemplateSampleModel model)
        {
        }

        //============================================================
        // Callbacks
        //============================================================
        public void OnIncreaseClickedInspector()
        {
            _onIncreaseClicked?.Invoke();
        }
    }
}
