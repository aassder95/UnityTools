using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityTools.Util
{
    // Exception: common logging types are grouped in Types section.
    //============================================================
    //Types
    //============================================================
    public static class DebugLogGate
    {
        //============================================================
        //Constants
        //============================================================
        private const string DEFAULT_CATEGORY = "__default__";

        //============================================================
        //Readonly
        //============================================================
        private static readonly Dictionary<string, bool> _categoryEnabled = new(StringComparer.Ordinal);

        //============================================================
        //Fields
        //============================================================
        private static bool _defaultEnabled = true;

        //============================================================
        //Properties
        //============================================================
        public static bool DefaultEnabled
        {
            get => _defaultEnabled;
            set => _defaultEnabled = value;
        }

        //============================================================
        //Logic
        //============================================================
        public static void SetEnabled(string category, bool isEnabled)
        {
            string key = NormalizeCategory(category);
            _categoryEnabled[key] = isEnabled;
        }

        public static bool IsEnabled(string category)
        {
            string key = NormalizeCategory(category);
            if(_categoryEnabled.TryGetValue(key, out bool isEnabled))
                return isEnabled;

            return _defaultEnabled;
        }

        public static void Reset()
        {
            _categoryEnabled.Clear();
        }

        //============================================================
        //Utilities
        //============================================================
        private static string NormalizeCategory(string category)
        {
            if(string.IsNullOrWhiteSpace(category))
                return DEFAULT_CATEGORY;

            return category.Trim();
        }
    }

    public static class DebugLogger
    {
        //============================================================
        //Logic
        //============================================================
        public static void Log(bool isEnabled, string className, string method, string msg, string category = null, UnityEngine.Object context = null)
        {
            if(!CanLog(isEnabled, className, category))
                return;

            string message = FormatMessage(className, method, msg);
            if(context == null)
                Debug.Log(message);
            else
                Debug.Log(message, context);
        }

        public static void LogWarning(bool isEnabled, string className, string method, string msg, string category = null, UnityEngine.Object context = null)
        {
            if(!CanLog(isEnabled, className, category))
                return;

            string message = FormatMessage(className, method, msg);
            if(context == null)
                Debug.LogWarning(message);
            else
                Debug.LogWarning(message, context);
        }

        public static void LogError(bool isEnabled, string className, string method, string msg, string category = null, UnityEngine.Object context = null)
        {
            if(!CanLog(isEnabled, className, category))
                return;

            string message = FormatMessage(className, method, msg);
            if(context == null)
                Debug.LogError(message);
            else
                Debug.LogError(message, context);
        }

        public static void LogException(bool isEnabled, string className, string method, Exception ex, string category = null, UnityEngine.Object context = null)
        {
            if(!CanLog(isEnabled, className, category))
                return;

            if(ex == null)
            {
                LogError(isEnabled, className, method, "예외 정보가 null입니다.", category, context);
                return;
            }

            LogError(isEnabled, className, method, $"예외 발생: {ex.Message}", category, context);
            if(context == null)
                Debug.LogException(ex);
            else
                Debug.LogException(ex, context);
        }

        //============================================================
        //Utilities
        //============================================================
        private static bool CanLog(bool isEnabled, string className, string category)
        {
            if(!isEnabled)
                return false;

            string resolvedCategory = string.IsNullOrWhiteSpace(category) ? NormalizeToken(className, "UnknownClass") : category.Trim();
            return DebugLogGate.IsEnabled(resolvedCategory);
        }

        private static string FormatMessage(string className, string method, string msg)
        {
            string safeClassName = NormalizeToken(className, "UnknownClass");
            string safeMethod = NormalizeToken(method, "UnknownMethod");
            string safeMsg = msg ?? string.Empty;
            return $"[{safeClassName}:{safeMethod}] {safeMsg}";
        }

        private static string NormalizeToken(string value, string fallback)
        {
            if(string.IsNullOrWhiteSpace(value))
                return fallback;

            return value.Trim();
        }
    }

    //============================================================
    // Interface
    //============================================================
    public interface IState
    {
        void Enter();
        void Execute();
        void Exit();
    }

    public class StateMachine<TType> where TType : Enum
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly Dictionary<TType, IState> _states = new();
        private readonly bool _isEnableLog;

        //============================================================
        // Fields
        //============================================================
        private TType _curType;
        private IState _curState;
        private bool _hasCurrentState;

        //============================================================
        // Events
        //============================================================
        public event UnityAction<TType, TType> OnStateTransition { add => _onStateTransition += value; remove => _onStateTransition -= value; }
        private event UnityAction<TType, TType> _onStateTransition;

        //============================================================
        // Properties
        //============================================================
        public TType CurType => _curType;
        public bool HasCurrentState => _hasCurrentState;

        //============================================================
        // Constructor
        //============================================================
        public StateMachine(bool isEnableLog = true)
        {
            _isEnableLog = isEnableLog;
        }

        //============================================================
        // State Management
        //============================================================
        public bool Add(TType type, IState state)
        {
            string fromState = _hasCurrentState ? _curType.ToString() : "미초기화";
            if(state == null)
            {
                DebugLogger.LogWarning(_isEnableLog, nameof(StateMachine<TType>), nameof(Add), $"전이 실패: 이전={fromState}, 대상={type}, 사유=상태가 null입니다.");
                return false;
            }

            if(_states.TryAdd(type, state))
                return true;

            DebugLogger.LogWarning(_isEnableLog, nameof(StateMachine<TType>), nameof(Add), $"전이 실패: 이전={fromState}, 대상={type}, 사유=상태가 중복입니다.");
            return false;
        }

        public bool Change(TType type, bool isUpdate = false)
        {
            string fromState = _hasCurrentState ? _curType.ToString() : "미초기화";
            if(!_states.TryGetValue(type, out IState newState))
            {
                DebugLogger.LogWarning(_isEnableLog, nameof(StateMachine<TType>), nameof(Change), $"전이 실패: 이전={fromState}, 대상={type}, 사유=상태가 없습니다.");
                return false;
            }

            bool isSameState = _hasCurrentState && EqualityComparer<TType>.Default.Equals(_curType, type);
            if(isSameState)
            {
                DebugLogger.LogWarning(_isEnableLog, nameof(StateMachine<TType>), nameof(Change), $"전이 실패: 이전={fromState}, 대상={type}, 사유=동일 상태입니다.");
                return false;
            }

            TType prevType = _curType;
            _onStateTransition?.Invoke(prevType, type);

            _curState?.Exit();

            _curType = type;
            _curState = newState;
            _hasCurrentState = true;

            _curState.Enter();
            if(isUpdate)
                Update();

            return true;
        }

        public bool SetInitialState(TType type, bool isUpdate = false)
        {
            if(_hasCurrentState)
            {
                DebugLogger.LogWarning(_isEnableLog, nameof(StateMachine<TType>), nameof(SetInitialState), $"전이 실패: 이전={_curType}, 대상={type}, 사유=초기 상태가 이미 설정되었습니다.");
                return false;
            }

            return Change(type, isUpdate);
        }

        public bool HasState(TType type)
        {
            return _states.ContainsKey(type);
        }

        public void Update()
        {
            _curState?.Execute();
        }
    }
}
