using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using UnityTools.Manager;

namespace UnityTools.Util
{
    public class TaskTimerTestWindow : EditorWindow
    {
        //============================================================
        // Constants
        //============================================================
        private const float BUTTON_HEIGHT = 28f;
        private const float BUTTON_SPACING = 4f;

        //============================================================
        // Readonly
        //============================================================
        private readonly IStorage _storage = new PlayerPrefsStorage();

        //============================================================
        // Fields
        //============================================================
        private string _timerId = "test";
        private string _durationSec = "5";
        private string _reduceSec = "1";
        private string _status = "Ready";
        private string _savedStart = "-";
        private string _savedDuration = "-";
        private string _savedUpdated = "-";
        private string _savedState = "-";

        //============================================================
        // Init/Register
        //============================================================
        [MenuItem("Util/Tests/Task Timer")]
        public static void Open()
        {
            GetWindow<TaskTimerTestWindow>("TaskTimer Test");
        }

        //============================================================
        // Unity Methods
        //============================================================
        private void OnGUI()
        {
            EditorGUILayout.Space(5);
            GUILayout.Label("작업 타이머 테스트", EditorStyles.boldLabel);
            GUILayout.Space(5);

            _timerId = EditorGUILayout.TextField("타이머 ID", _timerId);
            _durationSec = EditorGUILayout.TextField("시작 시간(초)", _durationSec);
            _reduceSec = EditorGUILayout.TextField("차감 시간(초)", _reduceSec);

            GUILayout.Space(8);
            DrawActionButtonRow("타이머 초기화", InitTimer, "시작", StartTimer);
            DrawActionButtonRow("시간 차감", ReduceTimer, "즉시 완료", CompleteImmediately);
            DrawActionButtonRow("보상 수령", Claim, "현재 상태 알림", NotifyCurrentType);
            DrawActionButtonRow("저장값 삭제", ClearSavedData, "저장값 조회", LoadSavedData);

            GUILayout.Space(8);
            DrawCurrentState();

            GUILayout.Space(8);
            DrawSavedData();

            GUILayout.Space(8);
            EditorGUILayout.HelpBox(_status, MessageType.Info);
        }

        //============================================================
        // Persistence
        //============================================================
        private void ClearSavedData()
        {
            if(!TryGetId(out string id))
                return;

            if(!EditorUtility.DisplayDialog("확인", $"{id} 저장값을 삭제할까요?", "삭제", "취소"))
                return;

            _storage.Delete(TaskTimerStorageKeys.Start(id));
            _storage.Delete(TaskTimerStorageKeys.Duration(id));
            _storage.Delete(TaskTimerStorageKeys.Updated(id));
            _storage.Delete(TaskTimerStorageKeys.State(id));

            LoadSavedData();
            _status = $"저장값 삭제 완료: {id}";
        }

        private void LoadSavedData()
        {
            if(!TryGetId(out string id))
                return;

            _savedStart = ReadDateKey(TaskTimerStorageKeys.Start(id));
            _savedDuration = ReadDurationKey(TaskTimerStorageKeys.Duration(id));
            _savedUpdated = ReadDateKey(TaskTimerStorageKeys.Updated(id));
            _savedState = ReadStateKey(TaskTimerStorageKeys.State(id));
            _status = $"저장값 조회 완료: {id}";
        }

        //============================================================
        // Logic
        //============================================================
        private void InitTimer()
        {
            if(!TryGetManagerAndId(out TaskTimerManager manager, out string id))
                return;

            TaskTimerHandle handle = manager.GetHandle(id) ?? manager.CreateTaskTimerHandle(id);
            if(handle == null)
            {
                _status = "TaskTimerHandle 생성 실패";
                return;
            }

            manager.InitTimer(handle);
            _status = $"초기화 완료: {id}";
        }

        private void StartTimer()
        {
            if(!TryGetManagerAndId(out TaskTimerManager manager, out string id))
                return;

            if(!TryParseSeconds(_durationSec, out double durationSec) || durationSec <= 0d)
            {
                _status = $"시작 시간(초) 파싱 실패 또는 0 이하: {_durationSec}";
                return;
            }

            TaskTimerHandle handle = manager.GetHandle(id);
            if(handle == null)
            {
                _status = "핸들이 없습니다. 먼저 타이머 초기화를 실행하세요.";
                return;
            }

            manager.StartTimer(id, durationSec);
            _status = $"시작 요청 완료: {id}, duration={durationSec}초";
        }

        private void ReduceTimer()
        {
            if(!TryGetManagerAndId(out TaskTimerManager manager, out string id))
                return;

            if(!TryParseSeconds(_reduceSec, out double reduceSec) || reduceSec <= 0d)
            {
                _status = $"차감 시간(초) 파싱 실패 또는 0 이하: {_reduceSec}";
                return;
            }

            TaskTimerHandle handle = manager.GetHandle(id);
            if(handle == null)
            {
                _status = "핸들이 없습니다. 먼저 타이머 초기화를 실행하세요.";
                return;
            }

            manager.Reduce(id, reduceSec);
            _status = $"차감 요청 완료: {id}, reduce={reduceSec}초";
        }

        private void CompleteImmediately()
        {
            if(!TryGetManagerAndId(out TaskTimerManager manager, out string id))
                return;

            TaskTimerHandle handle = manager.GetHandle(id);
            if(handle == null)
            {
                _status = "핸들이 없습니다. 먼저 타이머 초기화를 실행하세요.";
                return;
            }

            manager.CompleteImmediately(id);
            _status = $"즉시 완료 요청 완료: {id}";
        }

        private void Claim()
        {
            if(!TryGetManagerAndId(out TaskTimerManager manager, out string id))
                return;

            TaskTimerHandle handle = manager.GetHandle(id);
            if(handle == null)
            {
                _status = "핸들이 없습니다. 먼저 타이머 초기화를 실행하세요.";
                return;
            }

            manager.Claim(id);
            _status = $"보상 수령 요청 완료: {id}";
        }

        private void NotifyCurrentType()
        {
            if(!TryGetManagerAndId(out TaskTimerManager manager, out string id))
                return;

            TaskTimerHandle handle = manager.GetHandle(id);
            if(handle == null)
            {
                _status = "핸들이 없습니다. 먼저 타이머 초기화를 실행하세요.";
                return;
            }

            handle.NotifyCurType();
            _status = $"현재 상태 알림 호출 완료: {id}, {handle.CurType}";
        }

        private bool TryGetManagerAndId(out TaskTimerManager manager, out string id)
        {
            manager = null;
            id = string.Empty;

            if(!Application.isPlaying)
            {
                _status = "플레이 모드에서만 가능합니다.";
                return false;
            }

            if(!TryGetId(out id))
                return false;

            manager = TaskTimerManager.Instance;
            if(manager != null)
                return true;

            _status = "TaskTimerManager를 찾을 수 없습니다.";
            return false;
        }

        private bool TryGetId(out string id)
        {
            id = (_timerId ?? string.Empty).Trim();
            if(!string.IsNullOrEmpty(id))
                return true;

            _status = "타이머 ID를 입력하세요.";
            return false;
        }

        private static bool TryParseSeconds(string input, out double value)
        {
            value = 0d;
            if(double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                return true;

            if(double.TryParse(input, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                return true;

            return false;
        }

        //============================================================
        // Utilities
        //============================================================
        private void DrawCurrentState()
        {
            if(!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("현재 에디터 모드입니다. 상태 조회는 플레이 모드에서 확인하세요.", MessageType.None);
                return;
            }

            if(!TryGetId(out string id))
            {
                EditorGUILayout.HelpBox("타이머 ID를 입력하세요.", MessageType.None);
                return;
            }

            TaskTimerManager manager = TaskTimerManager.Instance;
            if(manager == null)
            {
                EditorGUILayout.HelpBox("TaskTimerManager를 찾을 수 없습니다.", MessageType.None);
                return;
            }

            TaskTimerHandle handle = manager.GetHandle(id);
            if(handle == null)
            {
                EditorGUILayout.HelpBox("현재 핸들이 없습니다.", MessageType.None);
                EditorGUILayout.LabelField("Claimed(저장 기준)", manager.IsClaimed(id).ToString());
                return;
            }

            TaskTimerData data = handle.ToData();
            EditorGUILayout.LabelField("현재 상태", handle.CurType.ToString());
            EditorGUILayout.LabelField("남은 시간(초)", handle.RemainingSec.ToString());
            EditorGUILayout.LabelField("총 시간(초)", data.DurationSec.ToString());
            EditorGUILayout.LabelField("진행률", data.Progress.ToString("P1", CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("Claimed", handle.IsClaimed.ToString());
        }

        private void DrawSavedData()
        {
            GUILayout.Label("저장값 디버깅", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("시작 시간(START)", _savedStart);
            EditorGUILayout.LabelField("지속 시간(DURATION)", _savedDuration);
            EditorGUILayout.LabelField("갱신 시간(UPDATED)", _savedUpdated);
            EditorGUILayout.LabelField("상태(STATE)", _savedState);
        }

        private string ReadDateKey(string key)
        {
            if(!_storage.HasKey(key))
                return "(없음)";

            string raw = _storage.Load(key);
            if(!long.TryParse(raw, out long ticks))
                return $"잘못된 ticks 값: {raw}";

            if(ticks == DateTime.MinValue.Ticks)
                return $"{raw} (DateTime.MinValue)";

            if(ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks)
                return $"범위 초과 ticks: {raw}";

            DateTime time = new DateTime(ticks, DateTimeKind.Utc);
            return $"{raw} ({time:yyyy-MM-dd HH:mm:ss} UTC)";
        }

        private string ReadDurationKey(string key)
        {
            if(!_storage.HasKey(key))
                return "(없음)";

            string raw = _storage.Load(key);
            if(double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) ||
               double.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
            {
                return $"{raw} ({value:F2} sec)";
            }

            return $"잘못된 duration 값: {raw}";
        }

        private string ReadStateKey(string key)
        {
            if(!_storage.HasKey(key))
                return "(없음)";

            string raw = _storage.Load(key);
            if(!int.TryParse(raw, out int intState))
                return $"잘못된 state 값: {raw}";

            ETaskTimerType type = (ETaskTimerType)intState;
            return Enum.IsDefined(typeof(ETaskTimerType), type) ? $"{raw} ({type})" : $"{raw} (정의되지 않은 상태)";
        }

        private static void DrawActionButtonRow(string leftLabel, Action leftAction, string rightLabel, Action rightAction)
        {
            Rect rowRect = EditorGUILayout.GetControlRect(false, BUTTON_HEIGHT);
            float width = Mathf.Max(10f, (rowRect.width - BUTTON_SPACING) * 0.5f);

            Rect leftRect = new Rect(rowRect.x, rowRect.y, width, BUTTON_HEIGHT);
            Rect rightRect = new Rect(rowRect.x + width + BUTTON_SPACING, rowRect.y, width, BUTTON_HEIGHT);

            if(GUI.Button(leftRect, leftLabel))
                leftAction?.Invoke();

            if(GUI.Button(rightRect, rightLabel))
                rightAction?.Invoke();
        }
    }
}

