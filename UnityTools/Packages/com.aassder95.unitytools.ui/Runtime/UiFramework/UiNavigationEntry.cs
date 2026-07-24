namespace UnityTools.Ui
{
    public class UiNavigationEntry
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly IPresenter _presenter;
        private readonly IUiInteractionControl _interactionControl;
        private readonly IUiFocusControl _focusControl;
        private readonly bool _isModal;

        //============================================================
        // Properties
        //============================================================
        public IPresenter Presenter => _presenter;
        public IUiInteractionControl InteractionControl => _interactionControl;
        public IUiFocusControl FocusControl => _focusControl;
        public bool IsModal => _isModal;

        //============================================================
        // Constructors
        //============================================================
        public UiNavigationEntry(IPresenter presenter, IUiInteractionControl interactionControl = null, IUiFocusControl focusControl = null, bool isModal = true)
        {
            _presenter = presenter;
            _interactionControl = interactionControl;
            _focusControl = focusControl;
            _isModal = isModal;
        }
    }
}
