using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Util
{
    public interface IDynamicScrollViewChild
    {
        void UpdateView();
        void SetIndex(int idx);
        void SetPositionY(float y);
    }

    public class DynamicScrollView<TView> : MonoBehaviour where TView : Component, IDynamicScrollViewChild, IPoolable
    {
        [SerializeField] TView _view;
        [SerializeField] RectTransform _rtContent;
        [SerializeField] RectTransform _rtView;

        int _idx;
        ObjectPool<TView> _viewPool;
        List<TView> _views = new();

        void Awake()
        {
            _viewPool = new ObjectPool<TView>(_rtContent, _view, 10);
        }

        public void Init(int maxCnt)
        {
            _rtContent.SetSizeHeight(maxCnt * _rtView.sizeDelta.y);

            for (int i = 0; i < maxCnt; i++)
            {
                TView view = _viewPool.Get();
                view.SetIndex(i);
                view.SetPositionY(_rtContent.sizeDelta.y / 2.0f - _rtView.sizeDelta.y / 2.0f - i * _rtView.sizeDelta.y);
                _views.Add(view);
            }

            UpdateView();
        }

        public void UpdateView()
        {
            for (int i = 0; i < _views.Count; i++)
            {
                _views[i].UpdateView();
            }
        }
    }
}