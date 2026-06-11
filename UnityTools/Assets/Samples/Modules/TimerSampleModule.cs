using UnityEngine;
using UnityTools.Samples.Timer;
using UnityTools.Util.Core;

namespace UnityTools.Samples.Modules
{
    public class TimerSampleModule : SampleModuleBase
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private TimerView _timerView;

        //============================================================
        // Fields
        //============================================================
        private TimerPresenter _timerPresenter;

        //============================================================
        // Properties
        //============================================================
        public override string ModuleKey => SampleModuleKeys.TIMER;

        //============================================================
        // Logic
        //============================================================
        protected override bool OnInitModule()
        {
            if(!TryResolveView(ref _timerView))
                return false;

            _timerPresenter = new TimerPresenter(new TimerModel(), _timerView);
            _timerPresenter.Init();
            return _timerPresenter.IsInit;
        }

        protected override void OnShowModule()
        {
            _timerPresenter?.Show();
        }

        protected override void OnHideModule()
        {
            _timerPresenter?.Hide();
        }

        protected override void OnReleaseModule()
        {
            _timerPresenter?.Release();
            _timerPresenter = null;
        }
    }
}

