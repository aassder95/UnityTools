using UnityEngine;
using UnityEngine.UI;

namespace UnityTools.Persistence.Samples
{
    public class SaveLabController : MonoBehaviour
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly SaveLabExperiment _experiment = new SaveLabExperiment();

        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Scenarios")]
        [SerializeField] private Button _btnMigration;
        [SerializeField] private Button _btnRecovery;
        [SerializeField] private Button _btnFuture;
        [SerializeField] private Button _btnMissing;
        [SerializeField] private Button _btnCorrupt;
        [SerializeField] private Button _btnRejected;
        [Header("Results")]
        [SerializeField] private Text _txtBefore;
        [SerializeField] private Text _txtAfter;
        [SerializeField] private Text _txtResult;

        //============================================================
        // Fields
        //============================================================
        private SaveLabReport _report;

        //============================================================
        // Properties
        //============================================================
        public SaveLabReport Report => _report;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnEnable()
        {
            _btnMigration.onClick.AddListener(RunMigration);
            _btnRecovery.onClick.AddListener(RunRecovery);
            _btnFuture.onClick.AddListener(RunFuture);
            _btnMissing.onClick.AddListener(RunMissing);
            _btnCorrupt.onClick.AddListener(RunCorrupt);
            _btnRejected.onClick.AddListener(RunRejected);
        }

        private void OnDisable()
        {
            _btnMigration.onClick.RemoveListener(RunMigration);
            _btnRecovery.onClick.RemoveListener(RunRecovery);
            _btnFuture.onClick.RemoveListener(RunFuture);
            _btnMissing.onClick.RemoveListener(RunMissing);
            _btnCorrupt.onClick.RemoveListener(RunCorrupt);
            _btnRejected.onClick.RemoveListener(RunRejected);
        }

        //============================================================
        // Logic
        //============================================================
        public void Run(ESaveLabScenario scenario)
        {
            if (!_experiment.TryRun(scenario, out _report))
            {
                _txtBefore.text = "";
                _txtAfter.text = "";
                _txtResult.text = "Experiment could not finish. Check temporary storage permissions and platform file support.";
                return;
            }

            _txtBefore.text = _report.Before;
            _txtAfter.text = _report.After;
            _txtResult.text = (_report.IsPassed ? "PASS" : "FAIL") + " / " + scenario + "\n\n" + _report.Result;
        }

        //============================================================
        // Callbacks
        //============================================================
        private void RunMigration() => Run(ESaveLabScenario.Migration);
        private void RunRecovery() => Run(ESaveLabScenario.BackupRecovery);
        private void RunFuture() => Run(ESaveLabScenario.FutureVersion);
        private void RunMissing() => Run(ESaveLabScenario.MissingFile);
        private void RunCorrupt() => Run(ESaveLabScenario.CorruptFile);
        private void RunRejected() => Run(ESaveLabScenario.RejectedMigration);
    }
}
