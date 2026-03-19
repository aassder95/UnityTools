using System;
using System.Collections.Generic;
using UnityTools.Util.Constants;
using UnityTools.Util.Core.Collections;
using UnityTools.Util.Core.Events;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Core.Persistence;
using UnityTools.Util.Core.Pooling;
using UnityTools.Util.Core.Singleton;
using UnityTools.Util.Core.State;
using UnityTools.Util.Core.Timer.Period;
using UnityTools.Util.Core.Timer.Shared;
using UnityTools.Util.Core.Timer.Task;
using UnityTools.Util.Coroutines;
using UnityTools.Util.Extensions;
using UnityTools.Util.UIFramework;
using UnityTools.Util.Utilities;

namespace UnityTools.Util.UIFramework
{
    // Exception: type-centric file uses Types section.
    //============================================================
    //Types
    //============================================================
    public interface IModel
    {
        //============================================================
        //Events
        //============================================================
        event Action OnUpdated;
    }

    public abstract class BaseModel : IModel
    {
        //============================================================
        //Events
        //============================================================
        public event Action OnUpdated { add => _onUpdated += value; remove => _onUpdated -= value; }
        private event Action _onUpdated;

        //============================================================
        //Fields
        //============================================================
        private int _updateDepth;
        private bool _hasPendingUpdate;

        //============================================================
        //Logic
        //============================================================
        protected void BeginUpdate()
        {
            _updateDepth++;
        }

        protected void EndUpdate()
        {
            if (_updateDepth <= 0)
                return;

            _updateDepth--;

            if (_updateDepth > 0 || !_hasPendingUpdate)
                return;

            _hasPendingUpdate = false;
            _onUpdated?.Invoke();
        }

        protected void RunBatchUpdate(Action updateAction)
        {
            if (updateAction == null)
                return;

            BeginUpdate();

            try
            {
                updateAction.Invoke();
            }
            finally
            {
                EndUpdate();
            }
        }

        protected bool SetField<TValue>(ref TValue field, TValue value)
        {
            if (EqualityComparer<TValue>.Default.Equals(field, value))
                return false;

            field = value;
            NotifyUpdated();
            return true;
        }

        protected void NotifyUpdated()
        {
            if (_updateDepth > 0)
            {
                _hasPendingUpdate = true;
                return;
            }

            _onUpdated?.Invoke();
        }
    }
}
