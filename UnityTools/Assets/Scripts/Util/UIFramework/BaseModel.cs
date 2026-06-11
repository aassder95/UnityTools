using System;
using System.Collections.Generic;

namespace UnityTools.Util.UIFramework
{
    public abstract class BaseModel : IModel
    {
        //============================================================
        // Fields
        //============================================================
        private int _updateDepth;
        private bool _hasPendingUpdate;

        //============================================================
        // Events
        //============================================================
        public event Action OnUpdated { add => _onUpdated += value; remove => _onUpdated -= value; }
        private event Action _onUpdated;

        //============================================================
        // Logic
        //============================================================
        protected void BeginUpdate()
        {
            _updateDepth++;
        }

        protected void EndUpdate()
        {
            if(_updateDepth <= 0)
                return;

            _updateDepth--;
            if(_updateDepth > 0 || !_hasPendingUpdate)
                return;

            _hasPendingUpdate = false;
            _onUpdated?.Invoke();
        }

        protected void RunBatchUpdate(Action updateAction)
        {
            if(updateAction == null)
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
            if(EqualityComparer<TValue>.Default.Equals(field, value))
                return false;

            field = value;
            NotifyUpdated();
            return true;
        }

        protected void NotifyUpdated()
        {
            if(_updateDepth > 0)
            {
                _hasPendingUpdate = true;
                return;
            }

            _onUpdated?.Invoke();
        }
    }
}
