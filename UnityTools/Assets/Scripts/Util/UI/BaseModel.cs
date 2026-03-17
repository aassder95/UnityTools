using System;

namespace UnityTools.Util
{
    // Exception: type-centric file uses Types section.
    //============================================================
    //Types
    //============================================================
    public interface IModel
    {
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
        //Logic
        //============================================================
        protected void NotifyUpdated()
        {
            _onUpdated?.Invoke();
        }
    }
}
