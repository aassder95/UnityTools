using UnityEngine;
using UnityTools.Ui;

namespace UnityTools.Samples.Features
{
    public class FeaturePopupPresenter : IPresenter
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly GameObject _goPanel;

        //============================================================
        // Fields
        //============================================================
        private bool _isInit;

        //============================================================
        // Properties
        //============================================================
        public bool IsInit => _isInit;
        public bool IsVisible => _goPanel.activeSelf;

        //============================================================
        // Constructors
        //============================================================
        public FeaturePopupPresenter(GameObject goPanel) => _goPanel = goPanel;

        //============================================================
        // Init/Register
        //============================================================
        public void Init() => _isInit = true;
        public void Release()
        {
            Hide();
            _isInit = false;
        }

        //============================================================
        // Logic
        //============================================================
        public void Show()
        {
            Init();
            _goPanel.SetActive(true);
        }

        public void Hide() => _goPanel.SetActive(false);
    }
}
