using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityTools.Util
{
    public interface IState
    {
        void Enter();
        void Execute();
        void Exit();
    }

    public class StateMachine<TType> where TType : Enum
    {
        TType _curType;
        IState _curState;
        readonly Dictionary<TType, IState> _states = new();
        public TType CurType => _curType;
        public event UnityAction<TType> OnStateChanged;

        public void Add(TType type, IState state)
        {
            if (!_states.ContainsKey(type))
                _states.Add(type, state);
        }

        public void Change(TType type)
        {
            if (!_states.TryGetValue(type, out IState newState))
                return;

            _curState?.Exit();
            _curType = type;
            _curState = newState;
            _curState.Enter();
            OnStateChanged?.Invoke(_curType);
        }

        public void Update()
        {
            if (_curState != null)
                _curState.Execute();
        }
    }
}