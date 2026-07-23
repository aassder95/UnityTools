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
        private bool _hasCurState;

        //============================================================
        // Events
        //============================================================
        public event UnityAction<TType, TType> OnStateTransition { add => _onStateTransition += value; remove => _onStateTransition -= value; }
        private event UnityAction<TType, TType> _onStateTransition;

        //============================================================
        // Properties
        //============================================================
        public TType CurType => _curType;
        public bool HasCurState => _hasCurState;

        //============================================================
        // Init/Register
        //============================================================
        public bool TryAdd(TType type, IState state)
        {
            string fromState = _hasCurState ? _curType.ToString() : UNINITIALIZED_STATE;
            if(state == null)
            {
                DebugLogger.LogError($"상태 등록 실패: 이전={fromState}, 대상={type}, 사유=상태가 비어 있습니다.");
                return false;
            }

            if(_states.TryAdd(type, state))
                return true;

            DebugLogger.LogError($"상태 등록 실패: 이전={fromState}, 대상={type}, 사유=상태가 중복입니다.");
            return false;
        }

        //============================================================
        // Logic
        //============================================================
        public bool TryChange(TType type, bool shouldTick = false)
        {
            string fromState = _hasCurState ? _curType.ToString() : UNINITIALIZED_STATE;
            if(!_states.TryGetValue(type, out IState nextState))
            {
                DebugLogger.LogError($"상태 전환 실패: 이전={fromState}, 대상={type}, 사유=상태를 찾을 수 없습니다.");
                return false;
            }

            if(_hasCurState && EqualityComparer<TType>.Default.Equals(_curType, type))
                return true;

            TType prevType = _curType;
            _curState?.Exit();
            _curType = type;
            _curState = nextState;
            _hasCurState = true;
            _curState.Enter();
            _onStateTransition?.Invoke(prevType, type);

            if(shouldTick)
                Tick();

            return true;
        }

        public bool TrySetInitialState(TType type, bool shouldTick = false)
        {
            if(!_hasCurState)
                return TryChange(type, shouldTick);

            DebugLogger.LogError($"초기 상태 설정 실패: 현재={_curType}, 대상={type}, 사유=초기 상태가 이미 설정되었습니다.");
            return false;
        }

        public bool HasState(TType type)
        {
            return _states.ContainsKey(type);
        }

        public void Tick()
        {
            _curState?.Execute();
        }
    }
}
