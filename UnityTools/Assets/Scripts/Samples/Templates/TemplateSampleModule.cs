using UnityEngine;
using UnityTools.Samples.Modules;
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

namespace UnityTools.Samples.Templates
{
    public class TemplateSampleModule : MonoBehaviour, ISampleModule
    {
        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private string _moduleKey = "Template";
        [SerializeField] private TemplateSampleView _view;
        [SerializeField] private int _startCount;

        //============================================================
        //Fields
        //============================================================
        private TemplateSampleModel _model;
        private TemplateSamplePresenter _presenter;
        private bool _isInitialized;

        //============================================================
        //Properties
        //============================================================
        public string ModuleKey => _moduleKey;
        public bool IsInit => _isInitialized;

        //============================================================
        //Init/Register
        //============================================================
        public void Init()
        {
            if (_isInitialized)
                return;

            if (_view == null || !_view.gameObject.activeInHierarchy)
                return;

            _model = new TemplateSampleModel();
            _model.SetCount(_startCount);

            _presenter = new TemplateSamplePresenter(_model, _view);
            _presenter.Init();
            _isInitialized = _presenter.IsInit;
        }

        public void Show()
        {
            if (!_isInitialized)
                return;

            _presenter?.Show();
        }

        public void Release()
        {
            if (!_isInitialized)
                return;

            _presenter?.Release();
            _presenter = null;
            _model = null;
            _isInitialized = false;
        }
    }
}
