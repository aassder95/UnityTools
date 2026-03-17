using UnityTools.Model;
using UnityTools.UI;
using UnityTools.Util;

namespace UnityTools.Presenter
{
    public class TimerPresenter : BasePresenter<TimerModel, TimerView>
    {
        //============================================================
        //Fields
        //============================================================
        private PeriodTimer _periodTimer;

        //============================================================
        //Constructors
        //============================================================
        public TimerPresenter(TimerModel model, TimerView view) : base(model, view)
        {
        }

        //============================================================
        //Init/Register
        //============================================================
        public override void Init()
        {
            _periodTimer = new PeriodTimer("TIMER", VIEW);
            _periodTimer.Init(1.0, 1.0);
            base.Init();
        }

        public override void Release()
        {
            base.Release();
            _periodTimer?.Release();
            _periodTimer = null;
        }

        protected override void BindEvents()
        {
            base.BindEvents();

            if(_periodTimer == null)
                return;

            VIEW.OnForceOpen += _periodTimer.ForceOpen;
            VIEW.OnForceClosed += _periodTimer.ForceClosed;
            _periodTimer.OnUpdated += OnTimerUpdatedCallback;
            _periodTimer.OnStateChanged += OnStateChangedCallback;
        }

        protected override void UnbindEvents()
        {
            if(_periodTimer != null)
            {
                VIEW.OnForceOpen -= _periodTimer.ForceOpen;
                VIEW.OnForceClosed -= _periodTimer.ForceClosed;
                _periodTimer.OnUpdated -= OnTimerUpdatedCallback;
                _periodTimer.OnStateChanged -= OnStateChangedCallback;
            }

            base.UnbindEvents();
        }

        //============================================================
        //Callbacks
        //============================================================
        private void OnStateChangedCallback(EPeriodTimerType type)
        {
            MODEL.SetTimer(_periodTimer.OpenStartTime, _periodTimer.OpenUpdatedTime, _periodTimer.OpenEndTime, _periodTimer.ClosedEndTime);

            switch (type)
            {
                case EPeriodTimerType.Reset:
                    MODEL.SetState("Reset");
                    MODEL.SetSubState("Reset");
                    break;
                case EPeriodTimerType.Open:
                    MODEL.SetState("Open");
                    break;
                case EPeriodTimerType.Closed:
                    MODEL.SetState("Closed");
                    MODEL.SetSubState("Closed");
                    break;
            }
        }

        private void OnTimerUpdatedCallback(int remainMin)
        {
            MODEL.SetLoop(remainMin, _periodTimer.OpenUpdatedTime);
        }
    }
}
