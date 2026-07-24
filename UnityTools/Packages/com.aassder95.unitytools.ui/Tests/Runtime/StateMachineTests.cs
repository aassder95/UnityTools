using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityTools.Util.Core.State;

namespace UnityTools.Util.Tests.State
{
    public class StateMachineTests
    {
        //============================================================
        // Type Declarations
        //============================================================
        private enum ETestState
        {
            None = 0,
            First = 1,
            Second = 2,
        }

        //============================================================
        // Init/Register
        //============================================================
        [Test]
        public void AddRejectsInvalidStateWithoutReplacingRegisteredState()
        {
            StateMachine<ETestState> stateMachine = new();
            TrackingState registeredState = new();
            TrackingState duplicateState = new();

            LogAssert.Expect(LogType.Error, "[StateMachine.Add] State 등록 대상이 비어 있습니다. Type=None");
            stateMachine.Add(ETestState.None, null);
            Assert.That(stateMachine.HasState(ETestState.None), Is.False);

            stateMachine.Add(ETestState.First, registeredState);
            LogAssert.Expect(LogType.Error, "[StateMachine.Add] 같은 Type의 State가 이미 등록되어 있습니다. Type=First");
            stateMachine.Add(ETestState.First, duplicateState);
            stateMachine.Change(ETestState.First);

            Assert.That(registeredState.EnterCnt, Is.EqualTo(1));
            Assert.That(duplicateState.EnterCnt, Is.Zero);
        }

        //============================================================
        // Logic
        //============================================================
        [Test]
        public void ChangeMissingStatePreservesCurrentState()
        {
            StateMachine<ETestState> stateMachine = new();
            TrackingState state = new();
            stateMachine.Add(ETestState.First, state);
            stateMachine.Change(ETestState.First);

            LogAssert.Expect(LogType.Error, "[StateMachine.Change] 등록되지 않은 State로 전환할 수 없습니다. Type=Second");
            stateMachine.Change(ETestState.Second);

            Assert.That(stateMachine.CurType, Is.EqualTo(ETestState.First));
            Assert.That(state.ExitCnt, Is.Zero);
        }

        [Test]
        public void ChangeRunsLifecycleAndOptionalTickInOrder()
        {
            List<string> calls = new();
            StateMachine<ETestState> stateMachine = new();
            TrackingState firstState = new(calls, "First");
            TrackingState secondState = new(calls, "Second");
            stateMachine.Add(ETestState.First, firstState);
            stateMachine.Add(ETestState.Second, secondState);
            stateMachine.OnStateTransition += (prevType, nextType) => calls.Add(prevType + ">" + nextType);

            stateMachine.Change(ETestState.First);
            stateMachine.Change(ETestState.Second, true);

            Assert.That(calls, Is.EqualTo(new[]
            {
                "First.Enter",
                "None>First",
                "First.Exit",
                "Second.Enter",
                "First>Second",
                "Second.Execute",
            }));
        }

        //============================================================
        // Nested Types
        //============================================================
        private class TrackingState : IState
        {
            //============================================================
            // Readonly
            //============================================================
            private readonly List<string> _calls;
            private readonly string _name;

            //============================================================
            // Fields
            //============================================================
            private int _enterCnt;
            private int _executeCnt;
            private int _exitCnt;

            //============================================================
            // Properties
            //============================================================
            public int EnterCnt => _enterCnt;
            public int ExecuteCnt => _executeCnt;
            public int ExitCnt => _exitCnt;

            //============================================================
            // Constructors
            //============================================================
            public TrackingState(List<string> calls = null, string name = null)
            {
                _calls = calls;
                _name = name;
            }

            //============================================================
            // Logic
            //============================================================
            public void Enter()
            {
                _enterCnt++;
                _calls?.Add(_name + ".Enter");
            }

            public void Execute()
            {
                _executeCnt++;
                _calls?.Add(_name + ".Execute");
            }

            public void Exit()
            {
                _exitCnt++;
                _calls?.Add(_name + ".Exit");
            }
        }
    }
}
