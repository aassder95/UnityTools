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
        private const bool IS_DEBUG_LOG = true;

        private TType _curType;
        private IState _curState;
        private readonly Dictionary<TType, IState> _states = new();

        public TType CurType => _curType;

        public event UnityAction<TType> OnStateChanged;

        public void Add(TType type, IState state)
        {
            _states.TryAdd(type, state);
        }

        public void Change(TType type, bool isUpdate = false)
        {
            if (!_states.TryGetValue(type, out IState newState))
                return;

            if (IS_DEBUG_LOG)
                UnityEngine.Debug.Log($"[StateMachine:Change] {_curType} → {type}");

            _curState?.Exit();
            _curType = type;
            _curState = newState;
            _curState.Enter();
            OnStateChanged?.Invoke(_curType);

            if (isUpdate)
                Update();
        }

        public void Update()
        {
            _curState?.Execute();
        }
    }
}
