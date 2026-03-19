using UnityEngine;
using UnityTools.Samples.Timer;

namespace UnityTools.Samples.Modules
{
    public class TimerSampleModule : MonoBehaviour, ISampleModule
    {
        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private TimerView _timerView;

        //============================================================
        //Fields
        //============================================================
        private TimerPresenter _timerPresenter;
        private bool _isInitialized;

        //============================================================
        //Properties
        //============================================================
        public string ModuleKey => SampleModuleKeys.TIMER;
        public bool IsInitialized => _isInitialized;

        //============================================================
        //Init/Register
        //============================================================
        public void Init()
        {
            if (_isInitialized)
                return;

            if (_timerView == null || !_timerView.gameObject.activeInHierarchy)
                return;

            _timerPresenter = new(new(), _timerView);
            _timerPresenter.Init();
            _isInitialized = _timerPresenter.IsInitialized;
        }

        public void Show()
        {
            if (!_isInitialized)
                return;

            _timerPresenter?.Show();
        }

        public void Release()
        {
            if (!_isInitialized)
                return;

            _timerPresenter?.Release();
            _timerPresenter = null;
            _isInitialized = false;
        }
    }
}
