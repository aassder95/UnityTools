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
        [Header("Timer View")] [SerializeField] private TimerView _timerView;

        //============================================================
        // Fields
        //============================================================
        private TimerPresenter _timerPresenter;

        //============================================================
        // Properties
        //============================================================
        public override string ModuleKey => SampleModuleKeys.Timer;

        //============================================================
        // Logic
        //============================================================
        protected override bool OnInitModule()
        {
            TimerPresenter presenter = new TimerPresenter(new TimerModel(), _timerView);
            if(!presenter.TryInit())
                return false;

            _timerPresenter = presenter;
            return true;
        }

        protected override bool OnShowModule()
        {
            return _timerPresenter.TryShow();
        }

        protected override bool OnHideModule()
        {
            return _timerPresenter.TryHide();
        }

        protected override bool OnReleaseModule()
        {
            bool isSuccess = _timerPresenter.TryRelease();
            _timerPresenter = null;
            return isSuccess;
        }
    }
}
