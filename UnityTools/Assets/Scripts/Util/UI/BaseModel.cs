using System;

namespace UnityTools.Util
{
    // 예외 사유: 인터페이스 중심 파일이라 Types 섹션을 사용한다.
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
