using UnityEngine;
using UnityTools.Samples.Timer;
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
        public bool IsInit => _isInitialized;

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
            _isInitialized = _timerPresenter.IsInit;
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
