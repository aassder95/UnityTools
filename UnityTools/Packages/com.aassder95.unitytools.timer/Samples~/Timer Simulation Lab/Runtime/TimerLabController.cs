using UnityEngine;
using UnityEngine.UI;

namespace UnityTools.Timer.Samples
{
    public class TimerLabController : MonoBehaviour
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Scenarios")]
        [SerializeField] private Button _btnForwardTime;
        [SerializeField] private Button _btnOffline;
        [SerializeField] private Button _btnRollback;
        [SerializeField] private Button _btnFailure;
        [SerializeField] private Button _btnClaim;
        [SerializeField] private Button _btnPause;
        [SerializeField] private Button _btnCancel;
        [SerializeField] private Button _btnUnregister;
        [Header("Results")]
        [SerializeField] private Text _txtBefore;
        [SerializeField] private Text _txtAfter;
        [SerializeField] private Text _txtResult;

        //============================================================
        // Fields
        //============================================================
        private TimerLabExperiment _experiment;
        private TimerLabReport _report;

        //============================================================
        // Properties
        //============================================================
        public TimerLabReport Report => _report;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnEnable()
        {
            _experiment = new TimerLabExperiment(this);
            _btnForwardTime.onClick.AddListener(RunForwardTime);
            _btnOffline.onClick.AddListener(RunOffline);
            _btnRollback.onClick.AddListener(RunRollback);
            _btnFailure.onClick.AddListener(RunFailure);
            _btnClaim.onClick.AddListener(RunClaim);
            _btnPause.onClick.AddListener(RunPause);
            _btnCancel.onClick.AddListener(RunCancel);
            _btnUnregister.onClick.AddListener(RunUnregister);
        }

        private void OnDisable()
        {
            _btnForwardTime.onClick.RemoveListener(RunForwardTime);
            _btnOffline.onClick.RemoveListener(RunOffline);
            _btnRollback.onClick.RemoveListener(RunRollback);
            _btnFailure.onClick.RemoveListener(RunFailure);
            _btnClaim.onClick.RemoveListener(RunClaim);
            _btnPause.onClick.RemoveListener(RunPause);
            _btnCancel.onClick.RemoveListener(RunCancel);
            _btnUnregister.onClick.RemoveListener(RunUnregister);
        }

        //============================================================
        // Logic
        //============================================================
        public void Run(ETimerLabScenario scenario)
        {
            if (!_experiment.TryRun(scenario, out _report))
            {
                _txtBefore.text = "";
                _txtAfter.text = "";
                _txtResult.text = "Experiment could not finish. Check runner lifecycle and timer initialization.";
                return;
            }

            _txtBefore.text = _report.Before;
            _txtAfter.text = _report.After;
            _txtResult.text = (_report.IsPassed ? "PASS" : "FAIL") + " / " + scenario + "\n\n" + _report.Result;
        }

        //============================================================
        // Callbacks
        //============================================================
        private void RunForwardTime() => Run(ETimerLabScenario.ForwardTime);
        private void RunOffline() => Run(ETimerLabScenario.OfflineRestore);
        private void RunRollback() => Run(ETimerLabScenario.ClockRollback);
        private void RunFailure() => Run(ETimerLabScenario.SaveFailure);
        private void RunClaim() => Run(ETimerLabScenario.DuplicateClaim);
        private void RunPause() => Run(ETimerLabScenario.PauseResume);
        private void RunCancel() => Run(ETimerLabScenario.CancelRestart);
        private void RunUnregister() => Run(ETimerLabScenario.UnregisterRestore);
    }
}
