using System;
using System.Collections.Generic;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Util.UIFramework
{
    public abstract class BasePresenter<TModel, TView> : IPresenter where TModel : IModel where TView : IView<TModel>
    {
        //============================================================
        // Readonly
        //============================================================
        protected readonly TModel _model;
        protected readonly TView _view;

        //============================================================
        // Fields
        //============================================================
        private bool _isInit;

        //============================================================
        // Properties
        //============================================================
        public bool IsInit => _isInit;
        public bool IsVisible => _view.IsVisible;

        //============================================================
        // Constructors
        //============================================================
        protected BasePresenter(TModel model, TView view)
        {
            _model = model;
            _view = view;
        }

        //============================================================
        // Init/Register
        //============================================================
        public bool TryInit()
        {
            if(_isInit)
                return true;

            bool shouldReleaseView = false;
            if(!_view.IsInit)
            {
                if(!_view.TryInit())
                    return false;

                shouldReleaseView = true;
            }

            if(!OnInit())
            {
                RollbackInit(shouldReleaseView);
                DebugLogger.LogError("Presenter 초기화에 실패했습니다. 타입=" + GetType().Name);
                return false;
            }

            BindEvents();
            _isInit = true;
            return true;
        }

        public bool TryRelease()
        {
            if(!_isInit)
                return true;

            _isInit = false;
            List<string> failures = new();
            ExecuteCleanup(UnbindEvents, "이벤트 해제", failures);
            ExecuteCleanup(OnRelease, "Presenter 해제", failures);
            ExecuteCleanup(_view.TryRelease, "View 해제", failures);
            if(failures.Count == 0)
                return true;

            LogFailures("Presenter 해제에 실패했습니다.", failures);
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

        protected virtual void BindEvents()
        {
            _model.OnUpdated += OnModelUpdated;
        }

        protected virtual void UnbindEvents()
        {
            _model.OnUpdated -= OnModelUpdated;
        }

        //============================================================
        // Logic
        //============================================================
        public bool TryShow()
        {
            if(!TryInit() || !_view.TryShow())
                return false;

            if(!_view.TryRefresh(_model))
            {
                if(!_view.TryHide())
                    DebugLogger.LogError("Presenter 표시 실패 후 View를 숨기지 못했습니다. 타입=" + GetType().Name);

                return false;
            }

            if(OnShow())
                return true;

            if(!_view.TryHide())
                DebugLogger.LogError("Presenter 후처리 실패 후 View를 숨기지 못했습니다. 타입=" + GetType().Name);

            DebugLogger.LogError("Presenter 표시 후 처리에 실패했습니다. 타입=" + GetType().Name);
            return false;
        }

        public bool TryHide()
        {
            if(!_isInit || !_view.IsVisible)
                return true;

            if(!_view.TryHide())
                return false;

            if(OnHide())
                return true;

            DebugLogger.LogError("Presenter 숨김 후 처리에 실패했습니다. 타입=" + GetType().Name);
            return false;
        }

        protected void StopAfterFailure()
        {
            if(!TryRelease())
                DebugLogger.LogError("Presenter 실패 중단 후 해제를 완료하지 못했습니다. 타입=" + GetType().Name);
        }

        protected virtual bool OnShow()
        {
            return true;
        }

        protected virtual bool OnHide()
        {
            return true;
        }

        //============================================================
        // Callbacks
        //============================================================
        protected virtual void OnModelUpdated()
        {
            if(!_isInit || _view.TryRefresh(_model))
                return;

            StopAfterFailure();
        }

        //============================================================
        // Utilities
        //============================================================
        private void RollbackInit(bool shouldReleaseView)
        {
            List<string> failures = new();
            if(shouldReleaseView)
                ExecuteCleanup(_view.TryRelease, "View 롤백", failures);

            if(failures.Count > 0)
                LogFailures("Presenter 초기화 롤백 중 오류가 발생했습니다.", failures);
        }

        private static void ExecuteCleanup(Action action, string step, List<string> failures)
        {
            try
            {
                action.Invoke();
            }
            catch(Exception exception)
            {
                failures.Add(step + ": " + exception.Message);
            }
        }

        private static void ExecuteCleanup(Func<bool> action, string step, List<string> failures)
        {
            try
            {
                if(!action.Invoke())
                    failures.Add(step + ": 실패 반환");
            }
            catch(Exception exception)
            {
                failures.Add(step + ": " + exception.Message);
            }
        }

        private void LogFailures(string message, List<string> failures)
        {
            DebugLogger.LogError(message + " 타입=" + GetType().Name + ", 첫 오류=" + failures[0] + ", 오류 수=" + failures.Count);
        }
    }
}