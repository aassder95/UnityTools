using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityTools.Ui;

namespace UnityTools.Samples.Features
{
    public class UiFeatureDemo : MonoBehaviour
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Variable List")]
        [SerializeField] private FeatureScroll _scroll;
        [SerializeField] private float[] _heights;
        [SerializeField] private Button _btnResize;
        [SerializeField] private Button _btnJump;
        [SerializeField] private Text _txtList;
        [Header("Transition")]
        [SerializeField] private UiCanvasTransition _transition;
        [SerializeField] private Button _btnShow;
        [SerializeField] private Button _btnHide;
        [SerializeField] private Button _btnCancel;
        [SerializeField] private Text _txtTransition;
        [Header("Popup Stack")]
        [SerializeField] private UiCanvasTransition _lowerPopup;
        [SerializeField] private UiCanvasTransition _topPopup;
        [SerializeField] private Button _btnOpen;
        [SerializeField] private Button _btnCloseLower;
        [SerializeField] private Button _btnBack;
        [SerializeField] private Text _txtPopup;

        //============================================================
        // Fields
        //============================================================
        private UiNavigator _navigator;
        private UiNavigationEntry _lowerEntry;
        private UiNavigationEntry _topEntry;
        private float[] _curHeights;
        private bool _isExpanded;
        private Coroutine _coTransition;

        //============================================================
        // Properties
        //============================================================
        public int PopupCnt => _navigator.PopupCnt;
        public bool IsTransitioning => _transition.IsTransitioning;
        public int CreatedItemCnt => _scroll.CreatedItemCnt;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnEnable() => Init();
        private void OnDisable() => Release();

        //============================================================
        // Init/Register
        //============================================================
        public void Init()
        {
            _curHeights = (float[])_heights.Clone();
            if (!_scroll.TryInit(_curHeights))
            {
                _txtList.text = "Invalid list heights. Check the sample configuration.";
                return;
            }

            _isExpanded = false;
            _navigator = new UiNavigator();
            _lowerEntry = new UiNavigationEntry(new FeaturePopupPresenter(_lowerPopup.gameObject), _lowerPopup, null, true);
            _topEntry = new UiNavigationEntry(new FeaturePopupPresenter(_topPopup.gameObject), _topPopup, null, true);
            _btnResize.onClick.AddListener(ResizeFirst);
            _btnJump.onClick.AddListener(JumpToMiddle);
            _btnShow.onClick.AddListener(ShowTransition);
            _btnHide.onClick.AddListener(HideTransition);
            _btnCancel.onClick.AddListener(CancelTransition);
            _btnOpen.onClick.AddListener(OpenPopups);
            _btnCloseLower.onClick.AddListener(CloseLower);
            _btnBack.onClick.AddListener(Back);
            _navigator.OnChanged += RefreshPopups;
            _txtList.text = "Scroll the list or jump to its middle, then resize row 1. The visible anchor stays in place.";
            _txtTransition.text = "Show / Hide awaits the fade. Cancel ends the current request as Cancelled.";
            RefreshPopups();
        }

        public void Release()
        {
            _btnResize.onClick.RemoveListener(ResizeFirst);
            _btnJump.onClick.RemoveListener(JumpToMiddle);
            _btnShow.onClick.RemoveListener(ShowTransition);
            _btnHide.onClick.RemoveListener(HideTransition);
            _btnCancel.onClick.RemoveListener(CancelTransition);
            _btnOpen.onClick.RemoveListener(OpenPopups);
            _btnCloseLower.onClick.RemoveListener(CloseLower);
            _btnBack.onClick.RemoveListener(Back);
            if (_coTransition != null)
            {
                StopCoroutine(_coTransition);
                _coTransition = null;
            }

            _transition.CancelTransition();
            if (_navigator != null)
            {
                _navigator.OnChanged -= RefreshPopups;
                _navigator.Clear();
                _navigator = null;
            }

            _scroll.Release();
        }

        //============================================================
        // Coroutines
        //============================================================
        private IEnumerator CoTransition(Task<EUiTransitionResult> task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }

            _txtTransition.text = "Transition result: " + task.Result;
            _coTransition = null;
        }

        //============================================================
        // Callbacks
        //============================================================
        private void ResizeFirst()
        {
            float height = _isExpanded ? _heights[0] : _heights[0] * 3.0f;
            _curHeights[0] = height;
            if (!_scroll.TrySetItemHeight(0, height))
                return;

            _isExpanded = !_isExpanded;
            _txtList.text = $"Row 1: {height:0} px / {_heights.Length} rows / {_scroll.CreatedItemCnt} created objects";
        }

        private void JumpToMiddle() => _scroll.ScrollTo(_heights.Length / 2, true, alignment: EDynamicScrollAlignment.Center);
        private void ShowTransition() => ObserveTransition(_transition.ShowAsync());
        private void HideTransition() => ObserveTransition(_transition.HideAsync());
        private void CancelTransition() => _transition.CancelTransition();

        private void OpenPopups()
        {
            _navigator.Clear();
            bool isLowerOpened = _navigator.TryOpenPopup(_lowerEntry);
            if (isLowerOpened && !_navigator.TryOpenPopup(_topEntry))
                _txtPopup.text = "The upper popup could not be opened.";
        }

        private void CloseLower()
        {
            if (!_navigator.TryClosePopup(_lowerEntry))
                _txtPopup.text = "The lower popup is already closed. The upper popup is unchanged.";
        }

        private void Back()
        {
            if (!_navigator.TryHandleBack())
                _txtPopup.text = "No popup remains to close.";
        }

        private void RefreshPopups() => _txtPopup.text = $"Open popups: {_navigator.PopupCnt}. Close lower removes only the lower panel; Back closes the top.";

        //============================================================
        // Utilities
        //============================================================
        private void ObserveTransition(Task<EUiTransitionResult> task)
        {
            if (_coTransition != null)
                StopCoroutine(_coTransition);

            _txtTransition.text = "Transition running. Press Cancel before the fade ends.";
            if (task.IsCompleted)
            {
                _txtTransition.text = "Transition result: " + task.Result;
                _coTransition = null;
                return;
            }

            _coTransition = StartCoroutine(CoTransition(task));
        }
    }
}
