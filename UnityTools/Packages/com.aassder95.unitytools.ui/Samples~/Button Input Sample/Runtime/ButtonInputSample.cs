using UnityEngine;
using UnityEngine.UI;

namespace UnityTools.Ui.Samples.Buttons
{
    public class ButtonInputSample : MonoBehaviour
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Inputs")]
        [SerializeField] private UiRepeatButton _btnRepeat;
        [SerializeField] private Button _btnSingle;
        [Header("Controls")]
        [SerializeField] private Button _btnToggle;
        [SerializeField] private Button _btnPause;
        [SerializeField] private Text _txtStatus;

        //============================================================
        // Fields
        //============================================================
        private int _repeatCnt;
        private int _singleCnt;
        private float _prevTimeScale;
        private bool _isPaused;

        //============================================================
        // Properties
        //============================================================
        public int RepeatCnt => _repeatCnt;
        public int SingleCnt => _singleCnt;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnEnable()
        {
            _prevTimeScale = Time.timeScale;
            _btnRepeat.onClick.AddListener(CountRepeat);
            _btnSingle.onClick.AddListener(CountSingle);
            _btnToggle.onClick.AddListener(ToggleEnabled);
            _btnPause.onClick.AddListener(TogglePause);
            RefreshStatus();
        }

        private void OnDisable()
        {
            _btnRepeat.onClick.RemoveListener(CountRepeat);
            _btnSingle.onClick.RemoveListener(CountSingle);
            _btnToggle.onClick.RemoveListener(ToggleEnabled);
            _btnPause.onClick.RemoveListener(TogglePause);
            _btnRepeat.CancelPress();
            if (_isPaused)
                Time.timeScale = _prevTimeScale;

            _isPaused = false;
        }

        //============================================================
        // Callbacks
        //============================================================
        private void CountRepeat()
        {
            _repeatCnt++;
            RefreshStatus();
        }

        private void CountSingle()
        {
            _singleCnt++;
            RefreshStatus();
        }

        private void ToggleEnabled()
        {
            _btnRepeat.interactable = !_btnRepeat.interactable;
            RefreshStatus();
        }

        private void TogglePause()
        {
            _isPaused = !_isPaused;
            Time.timeScale = _isPaused ? 0.0f : _prevTimeScale;
            RefreshStatus();
        }

        //============================================================
        // Utilities
        //============================================================
        private void RefreshStatus()
        {
            _txtStatus.text = "SINGLE: " + _singleCnt + "    REPEAT: " + _repeatCnt + "\nREPEAT " + (_btnRepeat.interactable ? "ENABLED" : "DISABLED") + "    GAME " + (_isPaused ? "PAUSED" : "RUNNING");
        }
    }
}
