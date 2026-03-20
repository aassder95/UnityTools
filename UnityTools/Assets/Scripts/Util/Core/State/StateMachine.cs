using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Util.Core.State
{
    public class StateMachine<TType> where TType : Enum
    {
        //============================================================
        //Constants
        //============================================================
        private const string UNINITIALIZED_STATE = "誘몄큹湲고솕";

        //============================================================
        //Readonly
        //============================================================
        private readonly Dictionary<TType, IState> _states = new();

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
        public StateMachine() { }

        //============================================================
        //Logic
        //============================================================
        public bool Add(TType type, IState state)
        {
            string fromState = _hasCurrentState ? _curType.ToString() : UNINITIALIZED_STATE;
            if(state == null)
            {
                DebugLogger.LogWarning($"?꾩씠 ?ㅽ뙣: ?댁쟾={fromState}, ???{type}, ?ъ쑀=?곹깭媛 null?낅땲??");
                return false;
            }

            if(_states.TryAdd(type, state))
                return true;

            DebugLogger.LogWarning($"?꾩씠 ?ㅽ뙣: ?댁쟾={fromState}, ???{type}, ?ъ쑀=?곹깭媛 以묐났?낅땲??");
            return false;
        }

        public bool Change(TType type, bool isUpdate = false)
        {
            string fromState = _hasCurrentState ? _curType.ToString() : UNINITIALIZED_STATE;
            if(!_states.TryGetValue(type, out IState newState))
            {
                DebugLogger.LogWarning($"?꾩씠 ?ㅽ뙣: ?댁쟾={fromState}, ???{type}, ?ъ쑀=?곹깭媛 ?놁뒿?덈떎.");
                return false;
            }

            bool isSameState = _hasCurrentState && EqualityComparer<TType>.Default.Equals(_curType, type);
            if(isSameState)
            {
                DebugLogger.LogWarning($"?꾩씠 ?ㅽ뙣: ?댁쟾={fromState}, ???{type}, ?ъ쑀=?숈씪 ?곹깭?낅땲??");
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
                DebugLogger.LogWarning($"?꾩씠 ?ㅽ뙣: ?댁쟾={_curType}, ???{type}, ?ъ쑀=珥덇린 ?곹깭媛 ?대? ?ㅼ젙?섏뿀?듬땲??");
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
