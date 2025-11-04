using System.Collections.Generic;
using UnityEngine;
using UnityTools.UI;
using UnityTools.Util;

namespace UnityTools.Manager
{
    public class TaskTimerManager : MonoBehaviour
    {
        [SerializeField] private string _rootKey = "TASK_TIMER_TEST";
        [SerializeField] private List<string> _ids = new() { "A", "B", "C" };
        [SerializeField] private Transform _trParentCanvas;
        [SerializeField] private TaskTimerView _view;

        private Dictionary<string, TaskTimer> _timers = new();
        private Dictionary<string, TaskTimerView> _views = new();

        private void Start()
        {
            InitViews();
        }

        private void OnDestroy()
        {
            foreach (TaskTimer timer in _timers.Values)
                timer.Release();

            _timers.Clear();
            _views.Clear();
        }

        private void InitViews()
        {
            foreach (string id in _ids)
            {
                TaskTimer timer = new(_rootKey, id, this);
                timer.OnUpdated += seconds => OnTimerUpdated(id, seconds);
                timer.OnCompleted += () => OnTimerCompleted(id);
                timer.OnClaimed += () => OnTimerClaimed(id);
                timer.OnStateChanged += _ => RefreshView(id);
                timer.Init();
                _timers[id] = timer;

                TaskTimerView view = Instantiate(_view, _trParentCanvas);
                view.SetId(id);
                view.OnStartClicked += () => timer.Start(view.Duration);
                view.OnCompleteClicked += () => timer.CompleteImmediately();
                view.OnClaimClicked += () => timer.Claim();
                view.OnReduce1MinClicked += () => timer.Reduce(1.0);
                _views[id] = view;

                RefreshView(id);
            }
        }

        private void RefreshView(string id)
        {
            if(!_timers.TryGetValue(id, out TaskTimer timer) || !_views.TryGetValue(id, out TaskTimerView view))
                return;

            view.SetState(timer.CurType.ToString());

            switch (timer.CurType)
            {
                case ETaskTimerType.None:
                    view.SetTimer("대기 중");
                    view.SetBtnActive(true, false, false, false);
                    break;
                case ETaskTimerType.Processing:
                    OnTimerUpdated(id, timer.RemainingSec);
                    view.SetBtnActive(false, true, false, true);
                    break;
                case ETaskTimerType.Completed:
                    view.SetTimer("완료!");
                    view.SetBtnActive(false, false, true, false);
                    break;
            }
        }

        private void OnTimerUpdated(string id, int remainingSec)
        {
            if(_views.TryGetValue(id, out TaskTimerView view))
                view.SetTimer($"{remainingSec / 60:D2}:{remainingSec % 60:D2}");
        }

        private void OnTimerCompleted(string id)
        {
            if(_views.TryGetValue(id, out TaskTimerView view))
                view.SetTimer("완료!");
        }

        private void OnTimerClaimed(string id)
        {
            if(_views.TryGetValue(id, out TaskTimerView view))
            {
                view.SetState("None");
                view.SetTimer("대기 중");
                view.SetBtnActive(true, false, false, false);
            }
        }
    }
}
