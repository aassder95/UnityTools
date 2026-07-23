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
        protected override void OnInitModule()
        {
            _timerPresenter = new TimerPresenter(new TimerModel(), _timerView);
            _timerPresenter.Init();
        }

        protected override void OnShowModule()
        {
            _timerPresenter.Show();
        }

        protected override void OnHideModule()
        {
            _timerPresenter.Hide();
        }

        protected override void OnReleaseModule()
        {
            _timerPresenter.Release();
            _timerPresenter = null;
        }
    }
}
