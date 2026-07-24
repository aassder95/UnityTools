using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityTools.Timer
{
    public class StateMachine<TType> where TType : Enum
    {
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
        public void Add(TType type, IState state)
        {
            if (state == null)
            {
                Debug.LogError("State 등록 대상이 비어 있습니다. Type=" + type);
                return;
            }

            if (_states.ContainsKey(type))
            {
                Debug.LogError("같은 Type의 State가 이미 등록되어 있습니다. Type=" + type);
                return;
            }

            _states.Add(type, state);
        }

        //============================================================
        // Logic
        //============================================================
        public void Change(TType type, bool shouldTick = false)
        {
            if (!_states.TryGetValue(type, out IState nextState))
            {
                Debug.LogError("등록되지 않은 State로 전환할 수 없습니다. Type=" + type);
                return;
            }

            if (_hasCurState && EqualityComparer<TType>.Default.Equals(_curType, type))
                return;

            TType prevType = _curType;
            _curState?.Exit();
            _curType = type;
            _curState = nextState;
            _hasCurState = true;
            _curState.Enter();
            _onStateTransition?.Invoke(prevType, type);

            if (shouldTick)
                Tick();
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
