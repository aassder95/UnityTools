using UnityEngine;
using UnityTools.Samples.Inven;
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

namespace UnityTools.Samples.Modules
{
    public class InvenSampleModule : MonoBehaviour, ISampleModule
    {
        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private InvenView _invenView;
        [SerializeField] private int _invenModelCnt = 50;

        //============================================================
        //Fields
        //============================================================
        private InvenPresenter _invenPresenter;
        private bool _isInitialized;

        //============================================================
        //Properties
        //============================================================
        public string ModuleKey => SampleModuleKeys.INVEN;
        public bool IsInit => _isInitialized;

        //============================================================
        //Init/Register
        //============================================================
        public void Init()
        {
            if (_isInitialized)
                return;

            if (_invenView == null || !_invenView.gameObject.activeInHierarchy)
                return;

            _invenPresenter = new(new(_invenModelCnt), _invenView);
            _invenPresenter.Init();
            _isInitialized = _invenPresenter.IsInit;
        }

        public void Show()
        {
            if (!_isInitialized)
                return;

            _invenPresenter?.Show();
        }

        public void Release()
        {
            if (!_isInitialized)
                return;

            _invenPresenter?.Release();
            _invenPresenter = null;
            _isInitialized = false;
        }
    }
}
