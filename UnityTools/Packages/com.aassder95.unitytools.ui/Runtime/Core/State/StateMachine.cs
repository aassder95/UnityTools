using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Util.Core.State
{
    public class StateMachine<TType> where TType : Enum
    {
        //============================================================
        // Constants
        //============================================================
        private const string UNINITIALIZED_STATE = "미초기화";

        //============================================================
        // Readonly
        //============================================================
        private readonly Dictionary<TType, IState> _states = new();

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
        // Constructors
        //============================================================
        public StateMachine() { }

        //============================================================
        // Logic
        //============================================================
        public bool Add(TType type, IState state)
        {
            string fromState = _hasCurrentState ? _curType.ToString() : UNINITIALIZED_STATE;
            if(state == null)
            {
                DebugLogger.LogWarning($"상태 전환 실패: 이전={fromState}, 대상={type}, 사유=상태가 null입니다.");
                return false;
            }

            if(_states.TryAdd(type, state))
                return true;

            DebugLogger.LogWarning($"상태 전환 실패: 이전={fromState}, 대상={type}, 사유=상태가 중복입니다.");
            return false;
        }

        public bool Change(TType type, bool isUpdate = false)
        {
            string fromState = _hasCurrentState ? _curType.ToString() : UNINITIALIZED_STATE;
            if(!_states.TryGetValue(type, out IState newState))
            {
                DebugLogger.LogWarning($"상태 전환 실패: 이전={fromState}, 대상={type}, 사유=상태를 찾을 수 없습니다.");
                return false;
            }

            bool isSameState = _hasCurrentState && EqualityComparer<TType>.Default.Equals(_curType, type);
            if(isSameState)
            {
                DebugLogger.LogWarning($"상태 전환 실패: 이전={fromState}, 대상={type}, 사유=동일 상태입니다.");
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
                DebugLogger.LogWarning($"상태 전환 실패: 이전={_curType}, 대상={type}, 사유=초기 상태가 이미 설정되었습니다.");
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
