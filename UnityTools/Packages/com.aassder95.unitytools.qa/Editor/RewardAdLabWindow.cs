using UnityEditor;
using UnityEngine;

namespace UnityTools.Qa.Editor
{
    public class RewardAdLabWindow : EditorWindow
    {
        //============================================================
        // Fields
        //============================================================
        private RewardAdSimulator _simulator;
        private string _placement = "reward";
        private float _loadDelaySec = 1.0f;
        private float _watchDurationSec = 5.0f;
        private ERewardAdResult _loadResult = ERewardAdResult.Loaded;
        private ERewardAdResult _showResult = ERewardAdResult.Completed;
        private double _lastTimeSec;
        private bool _isRunning;
        private string _status;

        //============================================================
        // Init/Register
        //============================================================
        [MenuItem("Tools/UnityTools/QA/Reward Ad Lab")]
        public static void Open()
        {
            GetWindow<RewardAdLabWindow>("Reward Ad Lab");
        }

        //============================================================
        // Unity Methods
        //============================================================
        private void OnEnable()
        {
            _simulator = new RewardAdSimulator();
            _lastTimeSec = EditorApplication.timeSinceStartup;
            EditorApplication.update += Tick;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Tick;
            _simulator.Dispose();
        }

        private void OnGUI()
        {
            _placement = EditorGUILayout.TextField("Placement", _placement);
            _loadDelaySec = EditorGUILayout.FloatField("Load delay (sec)", _loadDelaySec);
            _watchDurationSec = EditorGUILayout.FloatField("Watch duration (sec)", _watchDurationSec);
            _loadResult = (ERewardAdResult)EditorGUILayout.EnumPopup("Load outcome", _loadResult);
            _showResult = (ERewardAdResult)EditorGUILayout.EnumPopup("Show outcome", _showResult);
            if (GUILayout.Button("Apply scenario"))
                _status = _simulator.TryConfigure(new RewardAdOptions(_loadDelaySec, _watchDurationSec, _loadResult, _showResult)) ? "시나리오 적용 완료" : "결과 종류·시간·진행 중 작업을 확인하세요.";

            _isRunning = EditorGUILayout.Toggle("Advance automatically", _isRunning);
            if (GUILayout.Button("Load"))
                _status = _simulator.TryLoad(_placement) ? "로드 시작" : "로드할 수 없는 상태입니다.";

            if (GUILayout.Button("Show"))
                _status = _simulator.TryShow(_placement) ? "표시 시작" : "로드 완료와 진행 중 광고를 확인하세요.";

            if (GUILayout.Button("Cancel"))
                _status = _simulator.TryCancel(_placement) ? "취소 완료" : "진행 중 작업이 없습니다.";

            if (GUILayout.Button("Advance 1 sec"))
                _simulator.Advance(1.0f);

            if (_simulator.TryGetSnapshot(_placement, out RewardAdSnapshot snapshot))
                EditorGUILayout.LabelField(snapshot.State + " / " + snapshot.Result + " / " + snapshot.ElapsedSec.ToString("F2") + " sec");

            EditorGUILayout.HelpBox(_status ?? "시나리오를 먼저 적용하세요. 이 창은 자체 시뮬레이터이며 게임의 광고 서비스는 별도로 주입해야 합니다.", MessageType.Info);
        }

        //============================================================
        // Callbacks
        //============================================================
        private void Tick()
        {
            double nowSec = EditorApplication.timeSinceStartup;
            if (_isRunning)
            {
                _simulator.Advance((float)(nowSec - _lastTimeSec));
                Repaint();
            }

            _lastTimeSec = nowSec;
        }
    }
}
