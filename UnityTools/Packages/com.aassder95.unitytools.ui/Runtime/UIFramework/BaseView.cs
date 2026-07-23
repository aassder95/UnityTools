using UnityEngine;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Util.UIFramework
{
    public abstract class BaseView<TModel> : MonoBehaviour, IView<TModel> where TModel : IModel
    {
        //============================================================
        // Fields
        //============================================================
        private bool _isInit;

        //============================================================
        // Properties
        //============================================================
        public bool IsInit => _isInit;
        public bool IsVisible => gameObject.activeSelf;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnDestroy()
        {
            if(!TryRelease())
                DebugLogger.LogError("View 파괴 중 해제를 완료하지 못했습니다. 타입=" + GetType().Name, this);
        }

        //============================================================
        // Init/Register
        //============================================================
        public bool TryInit()
        {
            if(_isInit)
                return true;

            if(!OnInit())
            {
                DebugLogger.LogError("View 초기화에 실패했습니다. 타입=" + GetType().Name, this);
                enabled = false;
                return false;
            }

            _isInit = true;
            return true;
        }

        public bool TryRelease()
        {
            if(!_isInit)
                return true;

            _isInit = false;
            if(OnRelease())
                return true;

            DebugLogger.LogError("View 해제에 실패했습니다. 타입=" + GetType().Name, this);
            enabled = false;
            return false;
        }

        protected virtual bool OnInit()
        {
            return true;
        }

        protected virtual bool OnRelease()
        {
            return true;
        }

        //============================================================
        // Logic
        //============================================================
        public bool TryShow()
        {
            if(!TryInit())
                return false;

            if(gameObject.activeSelf)
                return true;

            gameObject.SetActive(true);
            return true;
        }

        public bool TryHide()
        {
            if(!gameObject.activeSelf)
                return true;

            gameObject.SetActive(false);
            return true;
        }

        public bool TryRefresh(TModel model)
        {

            if(OnRefresh(model))
                return true;

            DebugLogger.LogError("View 갱신에 실패했습니다. 타입=" + GetType().Name, this);
            enabled = false;
            return false;
        }

        protected abstract bool OnRefresh(TModel model);
    }
}