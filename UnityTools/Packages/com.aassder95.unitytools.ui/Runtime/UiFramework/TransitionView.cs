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
        protected override bool IsViewVisible
        {
            get
            {
                PrepareTransition();
                return _transition.IsVisible;
            }
        }

        public bool IsInteractionEnabled
        {
            get
            {
                PrepareTransition();
                return _transition.IsInteractionEnabled;
            }
        }

        //============================================================
        // Logic
        //============================================================
        public void SetInteractionEnabled(bool isEnabled)
        {
            PrepareTransition();
            _transition.SetInteractionEnabled(isEnabled);
        }

        protected override void ShowView()
        {
            PrepareTransition();
            _transition.Show();
        }

        protected override void HideView()
        {
            PrepareTransition();
            _transition.Hide();
        }

        private void PrepareTransition()
        {
            if (_transition == null)
                _transition = GetComponent<UiCanvasTransition>();
        }
    }
}
