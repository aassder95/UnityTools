using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityTools.Util
{
    //============================================================
    //Types
    //============================================================
    public interface IState
    {
        //============================================================
        //Logic
        //============================================================
        void Enter();
        void Execute();
        void Exit();
    }

    public class StateMachine<TType> where TType : Enum
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly Dictionary<TType, IState> _states = new();
        private readonly bool _isEnableLog;

        //============================================================
        //Fields
        //============================================================
        private TType _curType;
        private IState _curState;
        private bool _hasCurrentState;

        //============================================================
        //Events
        //============================================================
        public event UnityAction<TType, TType> OnStateTransition { add => _onStateTransition += value; remove => _onStateTransition -= value; }
        private event UnityAction<TType, TType> _onStateTransition;

        //============================================================
        //Properties
        //============================================================
        public TType CurType => _curType;
        public bool HasCurrentState => _hasCurrentState;

        //============================================================
        //Constructors
        //============================================================
        public StateMachine(bool isEnableLog = true)
        {
            _isEnableLog = isEnableLog;
        }

        //============================================================
        //Logic
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
