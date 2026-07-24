using System;
using System.Collections.Generic;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Util.UiFramework
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
            if (_updateDepth <= 0)
            {
                DebugLogger.LogError("Model Update 범위가 시작되지 않은 상태에서 종료되었습니다. 타입=" + GetType().Name);
                return;
            }

            _updateDepth--;
            if (_updateDepth > 0 || !_hasPendingUpdate)
                return;

            _hasPendingUpdate = false;
            _onUpdated?.Invoke();
        }

        protected void RunBatchUpdate(Action updateAction)
        {
            if (updateAction == null)
            {
                DebugLogger.LogError("Model Batch Update 작업이 비어 있습니다. 타입=" + GetType().Name);
                return;
            }

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

        protected void SetField<TValue>(ref TValue field, TValue value)
        {
            if (EqualityComparer<TValue>.Default.Equals(field, value))
                return;

            field = value;
            NotifyUpdated();
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
