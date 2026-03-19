using UnityEngine;
using UnityTools.Samples.Inven;

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
