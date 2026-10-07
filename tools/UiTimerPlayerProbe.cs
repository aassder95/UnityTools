using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityTools.Timer.Samples;
using UnityTools.Ui;

public class UiTimerPlayerProbe : MonoBehaviour
{
    //============================================================
    // Inspector Fields
    //============================================================
    [SerializeField] private UiRepeatButton _btnRepeat;
    [SerializeField] private UiButtonPressScale _pressScale;
    [SerializeField] private EventSystem _eventSystem;

    //============================================================
    // Fields
    //============================================================
    private Coroutine _coProbe;
    private int _clickCnt;

    //============================================================
    // Unity Methods
    //============================================================
    private void Start()
    {
        _coProbe = StartCoroutine(CoProbe());
    }

    private void OnDestroy()
    {
        if (_coProbe != null)
            StopCoroutine(_coProbe);

        _btnRepeat.onClick.RemoveListener(OnClick);
    }

    //============================================================
    // Coroutines
    //============================================================
    private IEnumerator CoProbe()
    {
        TimerLabExperiment experiment = new TimerLabExperiment(this);
        for (int idx = 0; idx <= (int)ETimerLabScenario.UnregisterRestore; ++idx)
        {
            if (!experiment.TryRun((ETimerLabScenario)idx, out TimerLabReport report) || !report.IsPassed)
            {
                Finish(false, "Timer 실험 실패: " + idx);
                yield break;
            }
        }

        if (!_btnRepeat.TryConfigure(0.0f, 0.02f, 0.01f, 1.0f) || !_pressScale.TryConfigure(0.8f, 0.0f))
        {
            Finish(false, "버튼 설정 실패");
            yield break;
        }

        _btnRepeat.onClick.AddListener(OnClick);
        PointerEventData owner = new PointerEventData(_eventSystem) { pointerId = 1, button = PointerEventData.InputButton.Left };
        PointerEventData other = new PointerEventData(_eventSystem) { pointerId = 2, button = PointerEventData.InputButton.Left };
        _btnRepeat.OnPointerDown(owner);
        _pressScale.OnPointerDown(owner);
        _btnRepeat.OnPointerUp(other);
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(0.15f);
        if (!_btnRepeat.IsHeld || _clickCnt == 0 || _btnRepeat.RepeatCnt != _clickCnt || Vector3.Distance(_btnRepeat.transform.localScale, Vector3.one * 0.8f) > 0.001f)
        {
            Finish(false, "unscaled 반복·포인터 소유권·눌림 스케일 실패");
            yield break;
        }

        _btnRepeat.OnPointerUp(owner);
        int clickCnt = _clickCnt;
        _btnRepeat.OnPointerClick(owner);
        _pressScale.OnPointerUp(owner);
        yield return new WaitForSecondsRealtime(0.05f);
        if (_clickCnt != clickCnt || _btnRepeat.IsHeld || Vector3.Distance(_btnRepeat.transform.localScale, Vector3.one) > 0.001f)
        {
            Finish(false, "release click 중복 방지·스케일 복원 실패");
            yield break;
        }

        _btnRepeat.OnPointerDown(owner);
        _pressScale.OnPointerDown(owner);
        yield return null;
        _btnRepeat.gameObject.SetActive(false);
        if (_btnRepeat.IsHeld || Vector3.Distance(_btnRepeat.transform.localScale, Vector3.one) > 0.001f)
        {
            Finish(false, "비활성화 복원 실패");
            yield break;
        }

        Finish(true, "Timer 8개 실험과 UI 버튼 반복·복원 통과");
    }

    //============================================================
    // Callbacks
    //============================================================
    private void OnClick()
    {
        ++_clickCnt;
    }

    //============================================================
    // Utilities
    //============================================================
    private void Finish(bool isPassed, string message)
    {
        Time.timeScale = 1.0f;
        _btnRepeat.onClick.RemoveListener(OnClick);
        _coProbe = null;
        string resultPath = Path.Combine(Application.persistentDataPath, "ui-timer-result.txt");
        string[] args = System.Environment.GetCommandLineArgs();
        for (int idx = 0; idx + 1 < args.Length; ++idx)
        {
            if (args[idx] == "--probe-result")
                resultPath = args[idx + 1];
        }

        File.WriteAllText(resultPath, isPassed ? "Passed" : "Failed: " + message);
        if (isPassed)
            Debug.Log(message);
        else
            Debug.LogError(message);

        Application.Quit(isPassed ? 0 : 1);
    }
}
