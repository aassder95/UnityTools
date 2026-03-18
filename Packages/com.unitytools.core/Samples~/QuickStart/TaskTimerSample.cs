using UnityEngine;
using UnityTools.Util;

namespace UnityTools.Samples
{
    public class TaskTimerSample : MonoBehaviour
    {
        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private string _timerId = "sample";
        [SerializeField] private float _durationSec = 10f;

        //============================================================
        //Fields
        //============================================================
        private TaskTimerHandle _handle;

        //============================================================
        //Unity Methods
        //============================================================
        private void Awake()
        {
            TaskTimer timer = new TaskTimer(_timerId, this);
            _handle = new TaskTimerHandle(timer);
            _handle.OnRemainSecUpdated += OnRemainSecUpdatedCallback;
            _handle.OnCompleted += OnCompletedCallback;
            _handle.Init();
        }

        private void Start()
        {
            _handle?.Start(_durationSec);
        }

        private void OnDestroy()
        {
            if(_handle == null)
                return;

            _handle.OnRemainSecUpdated -= OnRemainSecUpdatedCallback;
            _handle.OnCompleted -= OnCompletedCallback;
            _handle.Release();
            _handle = null;
        }

        //============================================================
        //Callbacks
        //============================================================
        private void OnRemainSecUpdatedCallback(int remainingSec)
        {
            DebugLogger.Log($"Remaining: {remainingSec}");
        }

        private void OnCompletedCallback()
        {
            DebugLogger.Log("Completed");
        }
    }
}
