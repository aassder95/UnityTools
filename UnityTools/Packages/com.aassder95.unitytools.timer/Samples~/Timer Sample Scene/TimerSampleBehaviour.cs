using UnityEngine;
using UnityTools.Timer.Task;

namespace UnityTools.Timer.Samples
{
    public class TimerSampleBehaviour : MonoBehaviour
    {
        //============================================================
        // Constants
        //============================================================
        private const string TIMER_ID = "TIMER_SAMPLE";

        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField, Min(1)] private int _durationSec = 10;

        //============================================================
        // Fields
        //============================================================
        private TaskTimerService _service;
        private TaskTimerHandle _handle;
        private int _remainingSec;
        private string _actionResult = "Ready";

        //============================================================
        // Unity Methods
        //============================================================
        private void Start()
        {
            Init();
        }

        private void OnDestroy()
        {
            Release();
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(24.0f, 24.0f, 260.0f, 24.0f), "Remaining: " + _remainingSec + " sec");
            if (GUI.Button(new Rect(24.0f, 56.0f, 120.0f, 36.0f), "Start"))
                _actionResult = _service.TryStart(TIMER_ID, _durationSec) ? "Start succeeded" : "Start unavailable";
            if (GUI.Button(new Rect(152.0f, 56.0f, 120.0f, 36.0f), "Complete"))
                _actionResult = _service.TryComplete(TIMER_ID) ? "Complete succeeded" : "Complete unavailable";
            if (GUI.Button(new Rect(280.0f, 56.0f, 120.0f, 36.0f), "Claim"))
                _actionResult = _service.TryClaim(TIMER_ID) ? "Claim succeeded" : "Claim unavailable";
            GUI.Label(new Rect(24.0f, 100.0f, 376.0f, 24.0f), _actionResult);
        }

        //============================================================
        // Init/Register
        //============================================================
        public void Init()
        {
            Release();
            _service = new TaskTimerService(this);
            if (!_service.TryCreate(TIMER_ID, out _handle) || !_service.TryInit(_handle))
            {
                Debug.LogError("Timer Sample 초기화에 실패했습니다.", this);
                return;
            }

            _remainingSec = _handle.RemainingSec;
            _handle.OnRemainSecUpdated += OnRemainSecUpdated;
            _handle.OnCompleted += OnCompleted;
            _handle.OnClaimed += OnClaimed;
        }

        public void Release()
        {
            if (_handle != null)
            {
                _handle.OnRemainSecUpdated -= OnRemainSecUpdated;
                _handle.OnCompleted -= OnCompleted;
                _handle.OnClaimed -= OnClaimed;
            }

            _service?.Release();
            _handle = null;
            _service = null;
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnRemainSecUpdated(int remainingSec)
        {
            _remainingSec = remainingSec;
        }

        private void OnCompleted()
        {
            _remainingSec = 0;
            Debug.Log("Timer Sample이 완료됐습니다.", this);
        }

        private void OnClaimed()
        {
            Debug.Log("Timer Sample 보상을 수령했습니다.", this);
        }
    }
}
