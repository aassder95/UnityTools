using UnityEngine;

namespace UnityTools.Ui
{
    [RequireComponent(typeof(UiCanvasTransition))]
    public abstract class TransitionView<TModel> : BaseView<TModel>, IUiInteractionControl where TModel : IModel
    {
        //============================================================
        // Fields
        //============================================================
        private UiCanvasTransition _transition;

        //============================================================
        // Properties
        //============================================================
        protected override bool IsViewVisible => _transition.IsVisible;
        public bool IsInteractionEnabled => IsInit && _transition.IsInteractionEnabled;

        //============================================================
        // Init/Register
        //============================================================
        protected override void OnInit()
        {
            base.OnInit();
            _transition = GetComponent<UiCanvasTransition>();
        }

        //============================================================
        // Logic
        //============================================================
        public void SetInteractionEnabled(bool isEnabled)
        {
            Init();
            _transition.SetInteractionEnabled(isEnabled);
        }

        protected override void ShowView()
        {
            _transition.Show();
        }

        protected override void HideView()
        {
            _transition.Hide();
        }
    }
}
