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
        [SerializeField] private UiLabBaseline _baseline;
        [SerializeField] private InputField _inputItemCnt;
        [SerializeField] private InputField _inputSeed;
        [SerializeField] private Dropdown _scenarioChoice;
        [Header("Controls")]
        [SerializeField] private Button _btnRun;
        [SerializeField] private Button _btnStop;
        [SerializeField] private Button _btnExport;
        [SerializeField] private Button _btnCompare;
        [SerializeField] private Dropdown _orderChoice;
        [SerializeField] private InputField _inputPairCnt;
        [SerializeField] private Text _txtResult;
        [Header("Measurement")]
        [SerializeField, Min(1)] private int _warmupFrames = 120;
        [SerializeField, Min(1)] private int _sampleFrames = 600;
        [SerializeField, Min(1.0f)] private float _timeoutSec = 120.0f;
        [SerializeField, Range(1, 10000)] private int _maxCompareItemCnt = 10000;
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
        private UiLabComparison _comparison;
        private UiLabSeries _series;
        private UiLabSeriesSummary _seriesSummary;
        private bool _isComparing;
        private bool _isBaseline;

        //============================================================
        // Properties
        //============================================================
        public bool IsRunning => _coRun != null;
        public UiLabReport Report => _report;
        public UiLabComparison Comparison => _comparison;
        public UiLabSeries Series => _series;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnEnable()
        {
            _btnRun.onClick.AddListener(Run);
            _btnStop.onClick.AddListener(Stop);
            _btnExport.onClick.AddListener(Export);
            _btnCompare.onClick.AddListener(Compare);
            _scroll.OnItemUpdated += OnItemUpdated;
        }

        private void OnDisable()
        {
            Stop();
            _btnRun.onClick.RemoveListener(Run);
            _btnStop.onClick.RemoveListener(Stop);
            _btnExport.onClick.RemoveListener(Export);
            _btnCompare.onClick.RemoveListener(Compare);
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
            _comparison = null;
            _series = null;
            _seriesSummary = null;
            _isBaseline = false;
            _baseline.Release();
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
            if (_isComparing)
            {
                ReleaseComparisonViews();
                _isComparing = false;
            }
            else
            {
                _scroll.SetInputEnabled(true);
            }
            SetControls(false);
            RefreshSummary();
            _txtResult.text = "측정을 취소했습니다. 완료되지 않은 쌍은 제외합니다." + (_seriesSummary != null ? "\n" + FormatSeries() : string.Empty);
        }

        public void Export()
        {
            if (IsRunning || (_report == null && (_series == null || _series.ComparisonCnt == 0)))
                return;

            bool hasSeries = _series != null && _series.ComparisonCnt > 0;
            string path = Path.Combine(Application.persistentDataPath, hasSeries ? "ui-performance-series-v1.csv" : "ui-performance-lab.csv");
            bool isSaved = hasSeries ? _series.TryExport(path) : _report.TryExport(path);
            string result = hasSeries ? FormatSeries() : FormatResult();
            _txtResult.text = result + (isSaved ? "\nCSV 저장: " + path : "\nCSV 저장에 실패했습니다. 저장 경로와 권한을 확인하세요.");
        }

        public void Compare()
        {
            if (IsRunning)
                return;

            if (!int.TryParse(_inputPairCnt.text, out int pairCnt) || pairCnt < 1 || pairCnt > UiLabSeries.MaxPairCnt || !int.TryParse(_inputItemCnt.text, out int itemCnt) || !int.TryParse(_inputSeed.text, out int seed) || itemCnt > _maxCompareItemCnt || itemCnt > 10000 || _warmupFrames < 1 || _sampleFrames < 1 || _timeoutSec <= 0.0f || !UiLabScenario.TryCreate(itemCnt, seed, (EUiLabScenario)_scenarioChoice.value, _itemStep, _mutationCnt, _mutationIntervalFrames, out _))
            {
                _txtResult.text = "비교 항목 수는 삽입 개수 이상, " + Mathf.Min(_maxCompareItemCnt, 10000) + " 이하, 반복 쌍은 1~20으로 입력하세요. 시드와 측정 설정도 확인하세요.";
                return;
            }

            _report = null;
            _comparison = null;
            _seriesSummary = null;
            if (!UiLabSeries.TryCreate(pairCnt, _orderChoice.value == 0, UiLabEnvironment.Capture(), out _series))
                return;

            _isComparing = true;
            SetControls(true);
            _txtResult.text = "두 방식을 같은 조건으로 순차 측정합니다. UI 초기화와 정리를 완료한 뒤 프레임을 측정합니다.";
            _coRun = StartCoroutine(CoSeries(itemCnt, seed, (EUiLabScenario)_scenarioChoice.value));
        }

        //============================================================
        // Coroutines
        //============================================================
        private IEnumerator CoSeries(int itemCnt, int seed, EUiLabScenario mode)
        {
            while (!_series.IsComplete)
            {
                _txtResult.text = "A/B 반복 " + (_series.ComparisonCnt + 1) + "/" + _series.RequestedPairCnt + " · " + (_series.IsNextBaselineFirst ? "Baseline → Virtualized" : "Virtualized → Baseline");
                _comparison = null;
                yield return CoCompare(itemCnt, seed, mode, _series.IsNextBaselineFirst);
                if (!_isComparing)
                    yield break;

                if (!_series.Environment.MatchesCurrent() || !_series.TryAdd(_comparison))
                {
                    _comparison = null;
                    CompleteComparison("비교 조건이 변경됐습니다. 완료된 이전 쌍만 집계합니다.");
                    yield break;
                }
            }

            CompleteComparison("반복 비교를 완료했습니다.");
        }

        private IEnumerator CoCompare(int itemCnt, int seed, EUiLabScenario mode, bool isBaselineFirst)
        {
            UiLabReport baselineReport = null;
            UiLabReport virtualReport = null;
            double baselineInitMs = 0.0d;
            double virtualInitMs = 0.0d;
            for (int phase = 0; phase < 2; ++phase)
            {
                if (!_series.Environment.MatchesCurrent())
                {
                    CompleteComparison("해상도 또는 품질 설정이 변경됐습니다. 완료된 이전 쌍만 집계합니다.");
                    yield break;
                }

                ReleaseComparisonViews();
                yield return null;
                System.GC.Collect();
                yield return null;
                if (!UiLabScenario.TryCreate(itemCnt, seed, mode, _itemStep, _mutationCnt, _mutationIntervalFrames, out _scenario))
                {
                    CompleteComparison("측정 조건이 유효하지 않습니다.");
                    yield break;
                }

                _isBaseline = phase == 0 ? isBaselineFirst : !isBaselineFirst;
                _bindCnt = 0;
                Canvas.ForceUpdateCanvases();
                System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
                if (_isBaseline)
                {
                    _baseline.Init(_scenario, _mutationCnt);
                }
                else
                {
                    _scroll.InitView(itemCnt);
                    _scroll.ScrollTo(0, true);
                    _scroll.SetInputEnabled(false);
                }

                Canvas.ForceUpdateCanvases();
                watch.Stop();
                double initMs = watch.Elapsed.TotalMilliseconds;
                _peakLiveItemCnt = _isBaseline ? _baseline.LiveItemCnt : _scroll.LiveItemCnt;
                AdvanceScenario();
                if (!_sampler.TryStart("UiLab." + mode + (_isBaseline ? ".Baseline" : ".Virtualized"), itemCnt, seed, _warmupFrames, _sampleFrames, MARKER_NAME))
                {
                    CompleteComparison("Profiler 카운터를 사용할 수 없습니다. Editor 또는 Development Build에서 실행하세요.");
                    yield break;
                }

                float deadlineSec = Time.realtimeSinceStartup + _timeoutSec;
                yield return null;
                while (_sampler.IsRunning)
                {
                    if (_sampler.TryCaptureFrame(out BenchmarkResult result))
                    {
                        UiLabReport report = new UiLabReport(result, _isBaseline ? _baseline.CreatedItemCnt : _scroll.CreatedItemCnt, _peakLiveItemCnt, _isBaseline ? _baseline.BindCnt : _bindCnt, _itemStep, _mutationCnt, _mutationIntervalFrames);
                        if (_isBaseline)
                        {
                            baselineReport = report;
                            baselineInitMs = initMs;
                        }
                        else
                        {
                            virtualReport = report;
                            virtualInitMs = initMs;
                        }

                        break;
                    }

                    if (Time.realtimeSinceStartup >= deadlineSec)
                    {
                        CompleteComparison("비교 측정 시간이 초과됐습니다. 완료되지 않은 쌍은 저장하지 않습니다.");
                        yield break;
                    }

                    AdvanceScenario();
                    _peakLiveItemCnt = Mathf.Max(_peakLiveItemCnt, _isBaseline ? _baseline.LiveItemCnt : _scroll.LiveItemCnt);
                    yield return null;
                }
            }

            Rect viewport = ((RectTransform)_scroll.transform).rect;
            _comparison = new UiLabComparison(baselineReport, virtualReport, baselineInitMs, virtualInitMs, isBaselineFirst, viewport.width, viewport.height);
            ReleaseComparisonViews();
        }

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

                AdvanceScenario();

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
        private void AdvanceScenario()
        {
            using (_tickMarker.Auto())
            {
                _scenario.Advance();
                if (_isBaseline)
                {
                    _baseline.Advance(_scenario);
                }
                else if (_scenario.MutationDelta > 0)
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
        }

        private void ReleaseComparisonViews()
        {
            _baseline.Release();
            if (_scroll.IsInitialized)
            {
                _scroll.SetInputEnabled(true);
                _scroll.ReleaseView();
            }
        }

        private void CompleteComparison(string message)
        {
            _sampler.Release();
            ReleaseComparisonViews();
            _coRun = null;
            _isComparing = false;
            SetControls(false);
            RefreshSummary();
            _txtResult.text = message + (_seriesSummary != null ? "\n" + FormatSeries() : string.Empty);
        }

        private void RefreshSummary()
        {
            _seriesSummary = null;
            if (_series != null && _series.TrySummarize(out UiLabSeriesSummary summary))
                _seriesSummary = summary;
        }

        private string FormatSeries()
        {
            return "A/B 완료 " + _series.ComparisonCnt + "/" + _series.RequestedPairCnt + " 쌍 · 순서 교대\n중앙값 ± 모집단 표준편차 · B / V\n" + FormatMetric("Init ms", _seriesSummary.Initialization) + FormatMetric("Frame P95 ms", _seriesSummary.FrameP95) + FormatMetric("Main P95 ms", _seriesSummary.MainThreadP95) + FormatMetric("GC B/frame", _seriesSummary.GcMean) + FormatMetric("Process peak B", _seriesSummary.PeakMemory) + "개선율은 완료된 각 쌍의 비율을 요약합니다.\n양수: 감소 / 음수: 증가 · 기준 0은 N/A\nCSV에 개별 실행과 요약을 함께 저장합니다.";
        }

        private static string FormatMetric(string label, UiLabMetricSummary summary)
        {
            string improvement = summary.HasImprovement ? summary.ImprovementMedian.ToString("F1", CultureInfo.InvariantCulture) + "% (n=" + summary.ImprovementCnt + ")" : "N/A";
            return string.Format(CultureInfo.InvariantCulture, "{0}: {1:F2} ± {2:F2} / {3:F2} ± {4:F2}\n개선율 중앙값: {5}\n", label, summary.BaselineMedian, summary.BaselineDeviation, summary.VirtualizedMedian, summary.VirtualizedDeviation, improvement);
        }

        private void SetControls(bool isRunning)
        {
            _inputItemCnt.interactable = !isRunning;
            _inputSeed.interactable = !isRunning;
            _scenarioChoice.interactable = !isRunning;
            _btnRun.interactable = !isRunning;
            _btnCompare.interactable = !isRunning;
            _orderChoice.interactable = !isRunning;
            _inputPairCnt.interactable = !isRunning;
            _btnStop.interactable = isRunning;
            _btnExport.interactable = !isRunning && (_report != null || (_series != null && _series.ComparisonCnt > 0));
        }

        private string FormatResult()
        {
            BenchmarkResult result = _report.Measurement;
            return string.Format(CultureInfo.InvariantCulture, "{0} · {1:N0} items · seed {2}\nFrame mean / P95 / max: {3:F2} / {4:F2} / {5:F2} ms\nMain Thread P95: {6:F2} ms · UiLab.Tick P95: {7:F3} ms\nGC mean: {8:F0} B/frame · peak memory: {9:F1} MiB\nCreated / peak live items: {10} / {11} · binds: {12:N0}\nCSV 내보내기로 기기 정보와 측정 조건을 함께 저장하세요.", result.ScenarioId, result.AgentCnt, result.Seed, result.MeanFrameMs, result.P95FrameMs, result.MaxFrameMs, result.P95MainThreadMs, result.P95MarkerMs, result.MeanGcBytes, result.PeakMemoryBytes / 1048576.0d, _report.CreatedItemCnt, _report.PeakLiveItemCnt, _report.BindCnt);
        }
    }
}
