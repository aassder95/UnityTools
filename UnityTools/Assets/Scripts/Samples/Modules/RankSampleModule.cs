using UnityEngine;
using UnityTools.Samples.Rank;
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
    public class RankSampleModule : MonoBehaviour, ISampleModule
    {
        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private RankView _rankView;
        [SerializeField] private int _rankModelCnt = 10;

        //============================================================
        //Fields
        //============================================================
        private RankPresenter _rankPresenter;
        private bool _isInitialized;

        //============================================================
        //Properties
        //============================================================
        public string ModuleKey => SampleModuleKeys.RANK;
        public bool IsInit => _isInitialized;

        //============================================================
        //Init/Register
        //============================================================
        public void Init()
        {
            if (_isInitialized)
                return;

            if (_rankView == null || !_rankView.gameObject.activeInHierarchy)
                return;

            _rankPresenter = new(new(_rankModelCnt), _rankView);
            _rankPresenter.Init();
            _isInitialized = _rankPresenter.IsInit;
        }

        public void Show()
        {
            if (!_isInitialized)
                return;

            _rankPresenter?.Show();
        }

        public void Release()
        {
            if (!_isInitialized)
                return;

            _rankPresenter?.Release();
            _rankPresenter = null;
            _isInitialized = false;
        }
    }
}
