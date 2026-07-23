using UnityTools.Util.Core.Logging;

namespace UnityTools.Util.Core.Timer.Task
{
    public class TaskTimerProcessingState : TaskTimerBaseState
    {
        //============================================================
        // Constructors
        //============================================================
        public TaskTimerProcessingState(TaskTimer timer) : base(timer) { }

        //============================================================
        // Logic
        //============================================================
        public override void Enter()
        {
            _timer.NotifyProcessingStarted();
        }

        public override void Execute()
        {
            if(_timer.IsPeriodExpired)
            {
                if(!_timer.TryUpdateCompletionTime())
                    if(!_timer.TryRelease())
                        DebugLogger.LogError("TaskTimer 완료 전환 실패 후 정리를 완료하지 못했습니다. ID=" + _timer.Id);
                return;
            }

            _timer.NotifyUpdate();
        }
    }
}
