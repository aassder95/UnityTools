using System;
using TMPro;
using UnityEngine;

namespace UnityTools.Ui.Localization
{
    [DisallowMultipleComponent]
    public class UiLocalizedText : MonoBehaviour
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Text")]
        [SerializeField] private TMP_Text _txt;
        [SerializeField] private string _key;
        [SerializeField] private string[] _args = Array.Empty<string>();
        [SerializeField] private bool _shouldApplyText = true;
        [Header("Font")]
        [SerializeField] private string _design;
        [SerializeField] private bool _shouldApplyFont = true;

        //============================================================
        // Fields
        //============================================================
        private IUiLocaleSource _source;
        private bool _isSubscribed;
        private bool _isResolved;

        //============================================================
        // Properties
        //============================================================
        public bool IsResolved => _isResolved;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnEnable()
        {
            if (_source == null)
                return;

            _source.OnLocaleChanged += HandleLocaleChanged;
            _isSubscribed = true;
            HandleLocaleChanged();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Release();
        }

        //============================================================
        // Init/Register
        //============================================================
        public void Init(IUiLocaleSource source)
        {
            if (source == null)
            {
                Debug.LogError("언어 공급자를 전달해야 합니다.", this);
                return;
            }

            Release();
            _source = source;
            if (isActiveAndEnabled)
            {
                _source.OnLocaleChanged += HandleLocaleChanged;
                _isSubscribed = true;
                HandleLocaleChanged();
            }
        }

        public void Release()
        {
            Unsubscribe();
            _source = null;
            _isResolved = false;
        }

        //============================================================
        // Logic
        //============================================================
        public void SetContent(string key, params string[] args)
        {
            _key = key;
            _args = args == null ? Array.Empty<string>() : (string[])args.Clone();
            if (_source != null && isActiveAndEnabled)
                HandleLocaleChanged();
        }

        public bool TryRefresh()
        {
            _isResolved = false;
            if (_source == null)
                return false;

            string text = null;
            TMP_FontAsset font = null;
            Material material = null;
            if (_shouldApplyText && (!_source.TryResolveText(_key, Array.AsReadOnly(_args ?? Array.Empty<string>()), out text) || text == null))
                return false;

            if (_shouldApplyFont && (!_source.TryResolveFont(_design, out font, out material) || font == null || material == null))
                return false;

            // 언어의 문구와 폰트를 모두 확인한 뒤 한 번에 적용합니다.
            if (_shouldApplyFont)
            {
                _txt.font = font;
                _txt.fontSharedMaterial = material;
            }

            if (_shouldApplyText)
                _txt.text = text;

            _isResolved = true;
            return true;
        }

        //============================================================
        // Callbacks
        //============================================================
        private void HandleLocaleChanged()
        {
            bool isRefreshed = TryRefresh();
            if (!isRefreshed)
                Debug.LogWarning("다국어 문구 또는 폰트를 찾지 못했습니다: " + _key + " / " + _source.Language, this);
        }

        //============================================================
        // Utilities
        //============================================================
        private void Unsubscribe()
        {
            if (!_isSubscribed)
                return;

            _source.OnLocaleChanged -= HandleLocaleChanged;
            _isSubscribed = false;
        }
    }
}
