using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using UnityTools.Manager;

namespace UnityTools.Util
{
    public class PeriodTimerTestWindow : EditorWindow
    {
        //============================================================
        //Constants
        //============================================================
        private const float BUTTON_HEIGHT = 28f;
        private const float BUTTON_SPACING = 4f;

        //============================================================
        //Readonly
        //============================================================
        private readonly IStorage _storage = new PlayerPrefsStorage();
        private readonly PeriodTimerSavedDataReader _savedDataReader;

        //============================================================
        //Fields
        //============================================================
        private string _timerId = "test";
        private string _openMin = "1";
        private string _closedMin = "1";
        private string _status = "Ready";
        private string _savedOpenStart = "-";
        private string _savedOpenEnd = "-";
        private string _savedClosedEnd = "-";
        private string _savedOpenUpdated = "-";
        private string _savedTampered = "-";

        //============================================================
        //Constructors
        //============================================================
        public PeriodTimerTestWindow()
        {
            _savedDataReader = new PeriodTimerSavedDataReader(_storage);
        }

        //============================================================
        //Init/Register
        //============================================================
        [MenuItem("Util/Tests/Period Timer")]
        public static void Open()
        {
            GetWindow<PeriodTimerTestWindow>("PeriodTimer Test");
        }

        //============================================================
        //Unity Methods
        //============================================================
        private void OnGUI()
        {
            EditorGUILayout.Space(5);
            GUILayout.Label("주기 타이머 테스트", EditorStyles.boldLabel);
            GUILayout.Space(5);

            _timerId = EditorGUILayout.TextField("타이머 ID", _timerId);
            _openMin = EditorGUILayout.TextField("오픈 시간(분)", _openMin);
            _closedMin = EditorGUILayout.TextField("클로즈 시간(분)", _closedMin);

            GUILayout.Space(8);
            DrawActionButtonRow("타이머 초기화", InitTimer, "주기 변경 예약", SetPeriods);
            DrawActionButtonRow("강제 오픈", ForceOpen, "강제 클로즈", ForceClosed);
            DrawActionButtonRow("저장값 삭제", ClearSavedData, "저장값 조회", LoadSavedData);

            GUILayout.Space(8);
            DrawCurrentState();

            GUILayout.Space(8);
            DrawSavedData();

            GUILayout.Space(8);
            EditorGUILayout.HelpBox(_status, MessageType.Info);
        }

        //============================================================
        //Persistence
        //============================================================
        private void ClearSavedData()
        {
            if(!TryGetId(out string id))
                return;

            if(!EditorUtility.DisplayDialog("확인", $"{id} 저장값을 삭제할까요?", "삭제", "취소"))
                return;

            _storage.Delete(PeriodTimerStorageKeys.OpenStart(id));
            _storage.Delete(PeriodTimerStorageKeys.OpenEnd(id));
            _storage.Delete(PeriodTimerStorageKeys.ClosedEnd(id));
            _storage.Delete(PeriodTimerStorageKeys.OpenUpdated(id));
            _storage.Delete(PeriodTimerStorageKeys.Tampered(id));

            LoadSavedData();
            _status = $"저장값 삭제 완료: {id}";
        }

        private void LoadSavedData()
        {
            if(!TryGetId(out string id))
                return;

            _savedOpenStart = _savedDataReader.ReadDateKey(PeriodTimerStorageKeys.OpenStart(id));
            _savedOpenEnd = _savedDataReader.ReadDateKey(PeriodTimerStorageKeys.OpenEnd(id));
            _savedClosedEnd = _savedDataReader.ReadDateKey(PeriodTimerStorageKeys.ClosedEnd(id));
            _savedOpenUpdated = _savedDataReader.ReadDateKey(PeriodTimerStorageKeys.OpenUpdated(id));
            _savedTampered = _savedDataReader.ReadTamperedKey(PeriodTimerStorageKeys.Tampered(id));
            _status = $"저장값 조회 완료: {id}";
        }

        //============================================================
        //Logic
        //============================================================
        private void InitTimer()
        {
            if(!TryGetManagerAndId(out PeriodTimerManager manager, out string id))
                return;

            if(!TryParseMinutes(_openMin, out double openMin) || openMin <= 0d)
            {
                _status = $"오픈 시간(분) 파싱 실패 또는 0 이하: {_openMin}";
                return;
            }

            if(!TryParseMinutes(_closedMin, out double closedMin) || closedMin <= 0d)
            {
                _status = $"클로즈 시간(분) 파싱 실패 또는 0 이하: {_closedMin}";
                return;
            }

            PeriodTimerHandle handle = manager.GetHandle(id) ?? manager.CreatePeriodTimerHandle(id);
            if(handle == null)
            {
                _status = "PeriodTimerHandle 생성 실패";
                return;
            }

            manager.InitTimer(handle, openMin, closedMin);
            _status = $"초기화 완료: {id} (오픈={openMin}분, 클로즈={closedMin}분)";
        }

        private void SetPeriods()
        {
            if(!Application.isPlaying)
            {
                _status = "플레이 모드에서만 주기 변경 예약이 가능합니다.";
                return;
            }

            PeriodTimerHandle handle = GetHandle();
            if(handle == null)
            {
                _status = "핸들이 없습니다. 먼저 타이머 초기화를 실행하세요.";
                return;
            }

            if(!TryParseMinutes(_openMin, out double openMin) || openMin <= 0d)
            {
                _status = $"오픈 시간(분) 파싱 실패 또는 0 이하: {_openMin}";
                return;
            }

            if(!TryParseMinutes(_closedMin, out double closedMin) || closedMin <= 0d)
            {
                _status = $"클로즈 시간(분) 파싱 실패 또는 0 이하: {_closedMin}";
                return;
            }

            handle.SetPeriods(openMin, closedMin);
            _status = $"다음 주기 반영 예약: 오픈={openMin}분, 클로즈={closedMin}분";
        }

        private void ForceOpen()
        {
            if(!Application.isPlaying)
            {
                _status = "플레이 모드에서만 강제 오픈이 가능합니다.";
                return;
            }

            PeriodTimerHandle handle = GetHandle();
            if(handle == null)
            {
                _status = "핸들이 없습니다. 먼저 타이머 초기화를 실행하세요.";
                return;
            }

            handle.ForceOpen();
            _status = "강제 오픈 실행 완료";
        }

        private void ForceClosed()
        {
            if(!Application.isPlaying)
            {
                _status = "플레이 모드에서만 강제 클로즈가 가능합니다.";
                return;
            }

            PeriodTimerHandle handle = GetHandle();
            if(handle == null)
            {
                _status = "핸들이 없습니다. 먼저 타이머 초기화를 실행하세요.";
                return;
            }

            handle.ForceClosed();
            _status = "강제 클로즈 실행 완료";
        }

        private bool TryGetManagerAndId(out PeriodTimerManager manager, out string id)
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

            manager = PeriodTimerManager.Instance;
            if(manager != null)
                return true;

            _status = "PeriodTimerManager를 찾을 수 없습니다.";
            return false;
        }

        private PeriodTimerHandle GetHandle()
        {
            if(!TryGetId(out string id))
                return null;

            PeriodTimerManager manager = PeriodTimerManager.Instance;
            if(manager == null)
                return null;

            return manager.GetHandle(id);
        }

        private bool TryGetId(out string id)
        {
            id = (_timerId ?? string.Empty).Trim();
            if(!string.IsNullOrEmpty(id))
                return true;

            _status = "타이머 ID를 입력하세요.";
            return false;
        }

        private static bool TryParseMinutes(string input, out double value)
        {
            value = 0d;
            if(double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                return true;

            if(double.TryParse(input, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                return true;

            return false;
        }

        //============================================================
        //Utilities
        //============================================================
        private void DrawCurrentState()
        {
            if(!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("현재 에디터 모드입니다. 상태 조회는 플레이 모드에서 확인하세요.", MessageType.None);
                return;
            }

            PeriodTimerHandle handle = GetHandle();
            if(handle == null)
            {
                EditorGUILayout.HelpBox("현재 핸들이 없습니다.", MessageType.None);
                return;
            }

            EditorGUILayout.LabelField("현재 상태", handle.CurType.ToString());
            EditorGUILayout.LabelField("남은 시간(분)", handle.RemainingMin.ToString());
            EditorGUILayout.LabelField("남은 시간(초)", handle.RemainingSec.ToString());
        }

        private void DrawSavedData()
        {
            GUILayout.Label("저장값 디버깅", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("오픈 시작(OPEN_START)", _savedOpenStart);
            EditorGUILayout.LabelField("오픈 종료(OPEN_END)", _savedOpenEnd);
            EditorGUILayout.LabelField("클로즈 종료(CLOSED_END)", _savedClosedEnd);
            EditorGUILayout.LabelField("오픈 갱신(OPEN_UPDATED)", _savedOpenUpdated);
            EditorGUILayout.LabelField("탬퍼 플래그(TAMPERED)", _savedTampered);
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

    // Exception: editor window delegates saved-data parsing to a dedicated helper type.
    //============================================================
    //Types
    //============================================================
    public class PeriodTimerSavedDataReader
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly IStorage _storage;

        //============================================================
        //Constructors
        //============================================================
        public PeriodTimerSavedDataReader(IStorage storage)
        {
            _storage = storage;
        }

        //============================================================
        //Logic
        //============================================================
        public string ReadDateKey(string key)
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

        public string ReadTamperedKey(string key)
        {
            if(!_storage.HasKey(key))
                return "(없음)";

            string raw = _storage.Load(key);
            return raw == "1" ? "1 (참)" : $"{raw} (거짓)";
        }
    }
}
