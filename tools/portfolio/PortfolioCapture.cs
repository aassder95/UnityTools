using System.Collections;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityTools.Benchmark.Samples;

public class PortfolioCapture : MonoBehaviour
{
    //============================================================
    // Inspector Fields
    //============================================================
    [SerializeField] private UiLabController _controller;
    [SerializeField] private InputField _inputItemCnt;
    [SerializeField] private InputField _inputPairCnt;
    [SerializeField] private Dropdown _scenarioChoice;
    [SerializeField] private Dropdown _orderChoice;
    [SerializeField] private Camera _camCapture;
    [SerializeField] private Canvas _canvas;

    //============================================================
    // Fields
    //============================================================
    private Coroutine _coCapture;

    //============================================================
    // Unity Methods
    //============================================================
    private void OnEnable()
    {
        _coCapture = StartCoroutine(CoCapture());
    }

    private void OnDisable()
    {
        if (_coCapture != null)
            StopCoroutine(_coCapture);

        _coCapture = null;
        _controller.Stop();
    }

    //============================================================
    // Coroutines
    //============================================================
    private IEnumerator CoCapture()
    {
        string output = System.Environment.GetEnvironmentVariable("UNITYTOOLS_PORTFOLIO_OUTPUT");
        if (string.IsNullOrWhiteSpace(output))
        {
            Debug.LogError("포트폴리오 출력 환경 변수를 지정하세요.");
            Application.Quit(1);
            yield break;
        }

        Directory.CreateDirectory(output);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        yield return new WaitForSecondsRealtime(2.0f);
        File.WriteAllText(Path.Combine(output, "environment.txt"), "UTC=" + System.DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture) + "\nUnity=" + Application.unityVersion + "\nOS=" + SystemInfo.operatingSystem + "\nCPU=" + SystemInfo.processorType.Trim() + "\nGPU=" + SystemInfo.graphicsDeviceName + "\nGraphicsAPI=" + SystemInfo.graphicsDeviceType + "\nRAM_MB=" + SystemInfo.systemMemorySize + "\nScreen=" + Screen.width + "x" + Screen.height + "\nQuality=" + QualitySettings.names[QualitySettings.GetQualityLevel()] + "\nVSync=0\nTargetFPS=60\nWarmupFrames=120\nSampleFrames=600\nSeed=42\nScenario=Sweep\nPairs=4\nVideoExcludedFromCSV=true\n");
        _scenarioChoice.value = 0;
        _orderChoice.value = 0;
        _inputPairCnt.text = "4";
        int workloadCnt = System.Environment.GetEnvironmentVariable("UNITYTOOLS_PORTFOLIO_CAPTURE_ONLY") == "1" ? 0 : 2;
        for (int workloadIdx = 0; workloadIdx < workloadCnt; ++workloadIdx)
        {
            int itemCnt = workloadIdx == 0 ? 1000 : 10000;
            _inputItemCnt.text = itemCnt.ToString(CultureInfo.InvariantCulture);
            _controller.Compare();
            while (_controller.IsRunning)
            {
                yield return null;
            }

            if (_controller.Series == null || !_controller.Series.IsComplete || !_controller.Series.TryExport(Path.Combine(output, "items-" + itemCnt + ".csv")))
            {
                Debug.LogError("포트폴리오 반복 측정 또는 CSV 저장 실패");
                Application.Quit(2);
                yield break;
            }

            _camCapture.enabled = true;
            yield return null;
            yield return CoScreenshot(Path.Combine(output, "items-" + itemCnt + ".png"));
            yield return new WaitForSecondsRealtime(2.0f);
            _camCapture.enabled = false;
        }

        // Screen capture allocates. This separate run is never exported as measurement evidence.
        Debug.LogWarning($"[TEST][{nameof(PortfolioCapture)}:{nameof(CoCapture)}] 캡처 상태: 카메라 수={Camera.allCamerasCount}, 그래픽={SystemInfo.graphicsDeviceType}, 화면={Screen.width}x{Screen.height}");
        _camCapture.enabled = true;
        yield return null;
        string frames = Path.Combine(output, "frames");
        Directory.CreateDirectory(frames);
        _inputItemCnt.text = "1000";
        _inputPairCnt.text = "1";
        _controller.Compare();
        int frameIdx = 0;
        do
        {
            yield return CoScreenshot(Path.Combine(frames, frameIdx.ToString("D4", CultureInfo.InvariantCulture) + ".png"));
            ++frameIdx;
            yield return new WaitForSecondsRealtime(0.5f);
        } while (_controller.IsRunning);

        for (int idx = 0; idx < 6; ++idx)
        {
            yield return CoScreenshot(Path.Combine(frames, frameIdx.ToString("D4", CultureInfo.InvariantCulture) + ".png"));
            ++frameIdx;
            yield return new WaitForSecondsRealtime(0.5f);
        }

        File.WriteAllText(Path.Combine(output, "capture-complete.txt"), frameIdx.ToString(CultureInfo.InvariantCulture));
        _coCapture = null;
        Application.Quit();
    }

    private IEnumerator CoScreenshot(string path)
    {
        yield return new WaitForEndOfFrame();
        Debug.LogWarning($"[TEST][{nameof(PortfolioCapture)}:{nameof(CoScreenshot)}] 캡처 요청: 카메라 수={Camera.allCamerasCount}, 파일={path}");
        RenderTexture target = new RenderTexture(Screen.width, Screen.height, 24);
        RenderTexture prevTarget = RenderTexture.active;
        _canvas.renderMode = RenderMode.ScreenSpaceCamera;
        _canvas.worldCamera = _camCapture;
        _canvas.planeDistance = 1.0f;
        _camCapture.targetTexture = target;
        Canvas.ForceUpdateCanvases();
        _camCapture.Render();
        RenderTexture.active = target;
        Texture2D texture = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0.0f, 0.0f, target.width, target.height), 0, 0);
        texture.Apply(false);
        File.WriteAllBytes(path, texture.EncodeToPNG());
        RenderTexture.active = prevTarget;
        _camCapture.targetTexture = null;
        _canvas.worldCamera = null;
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        target.Release();
        Destroy(target);
        Destroy(texture);
    }
}
