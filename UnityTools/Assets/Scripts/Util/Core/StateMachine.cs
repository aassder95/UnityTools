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
        #region Fields
        TType _curType;
        IState _curState;
        readonly Dictionary<TType, IState> _states = new();
        #endregion //Fields

        #region Properties
        public TType CurType => _curType;
        #endregion //Properties

        #region Events
        public event UnityAction<TType> OnStateChanged;
        #endregion //Events

        #region State Management
        public void Add(TType type, IState state)
        {
            if (!_states.ContainsKey(type))
                _states.Add(type, state);
        }

        public void Change(TType type, bool isUpdate = false)
        {
            if (!_states.TryGetValue(type, out IState newState))
                return;

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
            if (_curState != null)
                _curState.Execute();
        }
        #endregion //State Management
    }
}