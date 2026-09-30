using System.Collections;
using System.Globalization;
using System.IO;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;

namespace UnityTools.Benchmark.Samples
{
    public class UiLabController : MonoBehaviour
    {
        //============================================================
        // Constants
        //============================================================
        private const string MARKER_NAME = "UiLab.Tick";

        //============================================================
        // Readonly
        //============================================================
        private static readonly ProfilerMarker _tickMarker = new ProfilerMarker(ProfilerCategory.Scripts, MARKER_NAME);
        private readonly BenchmarkSampler _sampler = new BenchmarkSampler();

        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Lab")]
        [SerializeField] private UiLabScroll _scroll;
        [SerializeField] private InputField _inputItemCnt;
        [SerializeField] private InputField _inputSeed;
        [SerializeField] private Dropdown _scenarioChoice;
        [Header("Controls")]
        [SerializeField] private Button _btnRun;
        [SerializeField] private Button _btnStop;
        [SerializeField] private Button _btnExport;
        [SerializeField] private Text _txtResult;
        [Header("Measurement")]
        [SerializeField, Min(1)] private int _warmupFrames = 120;
        [SerializeField, Min(1)] private int _sampleFrames = 600;
        [SerializeField, Min(1.0f)] private float _timeoutSec = 120.0f;
        [Header("Workload")]
        [SerializeField, Min(1)] private int _itemStep = 3;
        [SerializeField, Min(1)] private int _mutationCnt = 10;
        [SerializeField, Min(1)] private int _mutationIntervalFrames = 30;

        //============================================================
        // Fields
        //============================================================
        private UiLabScenario _scenario;
        private Coroutine _coRun;
        private UiLabReport _report;
        private int _peakLiveItemCnt;
        private long _bindCnt;

        //============================================================
        // Properties
        //============================================================
        public bool IsRunning => _coRun != null;
        public UiLabReport Report => _report;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnEnable()
        {
            _btnRun.onClick.AddListener(Run);
            _btnStop.onClick.AddListener(Stop);
            _btnExport.onClick.AddListener(Export);
            _scroll.OnItemUpdated += OnItemUpdated;
        }

        private void OnDisable()
        {
            Stop();
            _btnRun.onClick.RemoveListener(Run);
            _btnStop.onClick.RemoveListener(Stop);
            _btnExport.onClick.RemoveListener(Export);
            _scroll.OnItemUpdated -= OnItemUpdated;
        }

        //============================================================
        // Logic
        //============================================================
        public void Run()
        {
            if (IsRunning)
                return;

            if (!int.TryParse(_inputItemCnt.text, out int itemCnt) || !int.TryParse(_inputSeed.text, out int seed) || _warmupFrames < 1 || _sampleFrames < 1 || _timeoutSec <= 0.0f || !UiLabScenario.TryCreate(itemCnt, seed, (EUiLabScenario)_scenarioChoice.value, _itemStep, _mutationCnt, _mutationIntervalFrames, out UiLabScenario scenario))
            {
                _txtResult.text = "항목 수는 삽입 개수 이상, 100,000 이하로 입력하세요. 시드와 측정 설정도 확인하세요.";
                return;
            }

            _scenario = scenario;
            _report = null;
            Canvas.ForceUpdateCanvases();
            _scroll.InitView(itemCnt);
            _scroll.ScrollTo(0, true);
            _scroll.SetInputEnabled(false);
            _peakLiveItemCnt = _scroll.LiveItemCnt;
            _bindCnt = 0;
            SetControls(true);
            _txtResult.text = "측정 중 · 워밍업 " + _warmupFrames + " 프레임 / 측정 " + _sampleFrames + " 프레임\n완료 후 통계를 표시합니다. 중지 버튼으로 취소할 수 있습니다.";
            using (_tickMarker.Auto())
            {
                _scenario.Advance();
                _scroll.ScrollTo(_scenario.TargetIdx, true);
            }

            if (!_sampler.TryStart("UiLab." + ((EUiLabScenario)_scenarioChoice.value), itemCnt, seed, _warmupFrames, _sampleFrames, MARKER_NAME))
            {
                _scroll.SetInputEnabled(true);
                SetControls(false);
                _txtResult.text = "Profiler 카운터를 사용할 수 없습니다. Editor 또는 Development Build에서 실행하세요.";
                return;
            }

            _coRun = StartCoroutine(CoRun());
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            StopCoroutine(_coRun);
            _coRun = null;
            _sampler.Release();
            _scroll.SetInputEnabled(true);
            SetControls(false);
            _txtResult.text = "측정을 취소했습니다. 완료되지 않은 결과는 저장하지 않습니다.";
        }

        public void Export()
        {
            if (IsRunning || _report == null)
                return;

            string path = Path.Combine(Application.persistentDataPath, "ui-performance-lab.csv");
            _txtResult.text = _report.TryExport(path) ? FormatResult() + "\nCSV 저장: " + path : FormatResult() + "\nCSV 저장에 실패했습니다. 저장 경로와 권한을 확인하세요.";
        }

        //============================================================
        // Coroutines
        //============================================================
        private IEnumerator CoRun()
        {
            float deadlineSec = Time.realtimeSinceStartup + _timeoutSec;
            yield return null;
            while (_sampler.IsRunning)
            {
                if (_sampler.TryCaptureFrame(out BenchmarkResult result))
                {
                    _report = new UiLabReport(result, _scroll.CreatedItemCnt, _peakLiveItemCnt, _bindCnt, _itemStep, _mutationCnt, _mutationIntervalFrames);
                    _coRun = null;
                    _scroll.SetInputEnabled(true);
                    SetControls(false);
                    _txtResult.text = FormatResult();
                    yield break;
                }

                if (Time.realtimeSinceStartup >= deadlineSec)
                {
                    _sampler.Release();
                    _coRun = null;
                    _scroll.SetInputEnabled(true);
                    SetControls(false);
                    _txtResult.text = "측정 시간이 초과됐습니다. 카운터 수집 상태와 제한 시간을 확인하세요.";
                    yield break;
                }

                using (_tickMarker.Auto())
                {
                    _scenario.Advance();
                    if (_scenario.MutationDelta > 0)
                    {
                        _scroll.InsertItems(0, _scenario.MutationDelta);
                    }
                    else if (_scenario.MutationDelta < 0)
                    {
                        _scroll.RemoveItems(0, -_scenario.MutationDelta);
                    }
                    else
                    {
                        _scroll.ScrollTo(_scenario.TargetIdx, true);
                    }
                }

                _peakLiveItemCnt = Mathf.Max(_peakLiveItemCnt, _scroll.LiveItemCnt);
                yield return null;
            }
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnItemUpdated(UiLabItem item)
        {
            item.Bind(_scenario.RowAt(item.Idx));
            ++_bindCnt;
        }

        //============================================================
        // Utilities
        //============================================================
        private void SetControls(bool isRunning)
        {
            _inputItemCnt.interactable = !isRunning;
            _inputSeed.interactable = !isRunning;
            _scenarioChoice.interactable = !isRunning;
            _btnRun.interactable = !isRunning;
            _btnStop.interactable = isRunning;
            _btnExport.interactable = !isRunning && _report != null;
        }

        private string FormatResult()
        {
            BenchmarkResult result = _report.Measurement;
            return string.Format(CultureInfo.InvariantCulture, "{0} · {1:N0} items · seed {2}\nFrame mean / P95 / max: {3:F2} / {4:F2} / {5:F2} ms\nMain Thread P95: {6:F2} ms · UiLab.Tick P95: {7:F3} ms\nGC mean: {8:F0} B/frame · peak memory: {9:F1} MiB\nCreated / peak live items: {10} / {11} · binds: {12:N0}\nCSV 내보내기로 기기 정보와 측정 조건을 함께 저장하세요.", result.ScenarioId, result.AgentCnt, result.Seed, result.MeanFrameMs, result.P95FrameMs, result.MaxFrameMs, result.P95MainThreadMs, result.P95MarkerMs, result.MeanGcBytes, result.PeakMemoryBytes / 1048576.0d, _report.CreatedItemCnt, _report.PeakLiveItemCnt, _report.BindCnt);
        }
    }
}
