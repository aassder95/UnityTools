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

namespace UnityTools.Util.Core.Timer.Period
{
    public class PeriodTimerStates
    {
        //============================================================
        //Logic
        //============================================================
        public class BaseState : IState
        {
            //============================================================
            //Readonly
            //============================================================
            protected readonly PeriodTimer _timer;

            //============================================================
            //Constructors
            //============================================================
            protected BaseState(PeriodTimer timer)
            {
                _timer = timer;
            }

            //============================================================
            //Logic
            //============================================================
            public virtual void Enter() { }
            public virtual void Execute() { }
            public virtual void Exit() { }
        }

        public class ResetState : BaseState
        {
            //============================================================
            //Constructors
            //============================================================
            public ResetState(PeriodTimer timer) : base(timer) { }

            //============================================================
            //Logic
            //============================================================
            public override void Enter()
            {
                _timer.NotifyOpenPeriodPreparing();
                _timer.ApplyPeriodTime();
                _timer.NotifyOpenPeriodStarted();
            }

            public override void Execute()
            {
                _timer.TryChangeState(EPeriodTimerType.Open, false, "ResetState.Execute");
            }
        }

        public class OpenState : BaseState
        {
            //============================================================
            //Constructors
            //============================================================
            public OpenState(PeriodTimer timer) : base(timer) { }

            //============================================================
            //Logic
            //============================================================
            public override void Enter()
            {
                _timer.NotifyOpenRemainMinUpdated();
            }

            public override void Execute()
            {
                if(_timer.IsTampered)
                {
                    _timer.HandleTampered();
                    return;
                }

                if(!_timer.IsOpenPeriod)
                {
                    _timer.TryChangeState(EPeriodTimerType.Closed, false, "OpenState.Execute");
                    return;
                }

                _timer.NotifyOpenRemainMinUpdated();
            }
        }

        public class ClosedState : BaseState
        {
            //============================================================
            //Constructors
            //============================================================
            public ClosedState(PeriodTimer timer) : base(timer) { }

            //============================================================
            //Logic
            //============================================================
            public override void Enter()
            {
                if(_timer.IsTamperedFlag)
                    _timer.ClearTampered();

                _timer.NotifyClosedPeriodStarted();
                _timer.NotifyClosedRemainMinUpdated();
            }

            public override void Execute()
            {
                if(!_timer.IsClosedPeriod)
                {
                    _timer.TryChangeState(EPeriodTimerType.Reset, true, "ClosedState.Execute");
                    return;
                }

                _timer.NotifyClosedRemainMinUpdated();
            }
        }
    }
}
