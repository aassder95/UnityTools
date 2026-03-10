using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityTools.Util
{
    public interface IState
    {
        void Enter();
        void Execute();
        void Exit();
    }

    public enum ESameStateTransitionPolicy
    {
        ReEnter,
        Ignore
    }

    public enum EStateTransitionFailReason
    {
        None,
        DuplicateState,
        MissingState,
        AlreadyInitialized,
        SameStateIgnored,
        NullState
    }

    public class EnumStateMachine<TType> where TType : Enum
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
        public event UnityAction<TType> OnStateChanged { add => _onStateChanged += value; remove => _onStateChanged -= value; }
        public event UnityAction<TType, TType> OnStateChanging { add => _onStateChanging += value; remove => _onStateChanging -= value; }
        public event UnityAction<TType, TType, EStateTransitionFailReason> OnTransitionFailed { add => _onTransitionFailed += value; remove => _onTransitionFailed -= value; }
        private event UnityAction<TType> _onStateChanged;
        private event UnityAction<TType, TType> _onStateChanging;
        private event UnityAction<TType, TType, EStateTransitionFailReason> _onTransitionFailed;

        //============================================================
        // Properties
        //============================================================
        public TType CurType => _curType;
        public bool HasCurrentState => _hasCurrentState;
        public ESameStateTransitionPolicy SameStateTransitionPolicy { get; set; } = ESameStateTransitionPolicy.ReEnter;

        //============================================================
        // Constructors
        //============================================================
        public EnumStateMachine(bool isEnableLog = true)
        {
            _isEnableLog = isEnableLog;
        }

        //============================================================
        // Logic
        //============================================================
        public bool Add(TType type, IState state)
        {
            if(state == null)
            {
                LogTransitionFailure("Add", type, EStateTransitionFailReason.NullState);
                return false;
            }

            if(_states.TryAdd(type, state))
                return true;

            LogTransitionFailure("Add", type, EStateTransitionFailReason.DuplicateState);
            return false;
        }

        public bool Change(TType type, bool isUpdate = false)
        {
            return Change(type, out _, isUpdate);
        }

        public bool Change(TType type, out EStateTransitionFailReason failReason, bool isUpdate = false)
        {
            failReason = EStateTransitionFailReason.None;
            if(!_states.TryGetValue(type, out IState newState))
            {
                failReason = EStateTransitionFailReason.MissingState;
                LogTransitionFailure("Change", type, failReason);
                return false;
            }

            bool isSameState = _hasCurrentState && EqualityComparer<TType>.Default.Equals(_curType, type);
            if(isSameState && SameStateTransitionPolicy == ESameStateTransitionPolicy.Ignore)
            {
                failReason = EStateTransitionFailReason.SameStateIgnored;
                LogTransitionFailure("Change", type, failReason);
                return false;
            }

            TType prevType = _curType;
            _onStateChanging?.Invoke(prevType, type);

            _curState?.Exit();
            _curType = type;
            _curState = newState;
            _hasCurrentState = true;

            _curState.Enter();
            _onStateChanged?.Invoke(_curType);

            if(isUpdate)
                Update();

            return true;
        }

        public bool SetInitialState(TType type, bool isUpdate = false)
        {
            if(_hasCurrentState)
            {
                LogTransitionFailure("SetInitialState", type, EStateTransitionFailReason.AlreadyInitialized);
                return false;
            }

            return Change(type, out _, isUpdate);
        }

        public bool HasState(TType type)
        {
            return _states.ContainsKey(type);
        }

        public void Update()
        {
            _curState?.Execute();
        }

        //============================================================
        // Utilities
        //============================================================
        private void LogTransitionFailure(string method, TType targetType, EStateTransitionFailReason reason)
        {
            _onTransitionFailed?.Invoke(_curType, targetType, reason);
            if(!_isEnableLog)
                return;

            string fromState = _hasCurrentState ? _curType.ToString() : "<none>";
            Debug.LogWarning($"[EnumStateMachine:{method}] 상태 전이 실패: {fromState} -> {targetType}, reason={reason}");
        }
    }
}
