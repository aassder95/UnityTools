using UnityEngine;
using UnityTools.Qa;
using UnityTools.Vat;

public class NextToolsPlayerProbe : MonoBehaviour
{
    //============================================================
    // Inspector Fields
    //============================================================
    [SerializeField] private VatPlayer _player;

    //============================================================
    // Fields
    //============================================================
    private bool _hasStarted;
    private float _deadlineSec;

    //============================================================
    // Unity Methods
    //============================================================
    private void Start()
    {
#if UNITYTOOLS_PORTS_QA
        _player.Init();
        if (!_player.TrySeek(0.5f) || !_player.TryPlay(4.0f))
        {
            Debug.LogError("VAT Player 검증 시작 실패");
            Application.Quit(1);
            return;
        }

        using (var simulator = new RewardAdSimulator())
        {
            if (!simulator.TryConfigure(new RewardAdOptions(0.0f, 1.0f)) || !simulator.TryLoad("reward"))
            {
                Debug.LogError("광고 QA 로드 실패");
                Application.Quit(1);
                return;
            }

            simulator.Advance(0.0f);
            if (!simulator.TryShow("reward"))
            {
                Debug.LogError("광고 QA 표시 실패");
                Application.Quit(1);
                return;
            }

            simulator.Advance(1.0f);
            if (!simulator.TryGetSnapshot("reward", out RewardAdSnapshot snapshot) || snapshot.Result != ERewardAdResult.Completed)
            {
                Debug.LogError("광고 QA 완료 결과 실패");
                Application.Quit(1);
                return;
            }
        }

        _deadlineSec = Time.unscaledTime + 10.0f;
        _hasStarted = true;
#else
        Debug.LogError("추가 Define 빌드 검증 실패");
        Application.Quit(1);
#endif
    }

    private void Update()
    {
        if (!_hasStarted)
            return;

        if (!_player.IsPlaying && _player.TimeSec == 1.0f)
        {
            _hasStarted = false;
            Debug.Log("이식 도구 Player 검증 통과");
            Application.Quit(0);
        }
        else if (Time.unscaledTime > _deadlineSec)
        {
            _hasStarted = false;
            Debug.LogError("VAT Player 완료 시간 초과");
            Application.Quit(1);
        }
    }

    private void OnDestroy()
    {
        _player.Release();
    }
}
