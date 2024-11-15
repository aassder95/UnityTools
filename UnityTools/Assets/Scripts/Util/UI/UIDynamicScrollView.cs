using UnityEngine;

namespace UnityTools.Util
{
    public interface IDynamicScrollViewChild
    {
        void SetIndex(int index);
        void SetPositionY(float posY);
    }

    public class UIDynamicScrollView<TModel, TView, TPresenter> : MonoBehaviour
    where TModel : UIModel, new()
    where TView : UIView
    where TPresenter : UIPresenter<TModel, TView>, IDynamicScrollViewChild, new()
    {
        [SerializeField] TView _view;
        [SerializeField] int _originMaxCnt;
        [SerializeField] int _originVisibleCnt;
        [SerializeField] float _spacing;
        [SerializeField] RectTransform _rtContent;
        [SerializeField] RectTransform _rtView;

        ObjectPool<TView> _viewPool;
        int _idx = 0;
        int _maxCnt;
        int _visibleCnt;
        Deque<TPresenter> _visiblePresenters = new();

        void Awake()
        {
            _viewPool = new ObjectPool<TView>(_rtContent, _view, _originMaxCnt);

            SetMaxCount(_originMaxCnt);
            SetVisibleCount(_originVisibleCnt);
        }

        TPresenter CreatePresenter(int idx)
        {
            TView view = _viewPool.Get();
            view.gameObject.SetActive(true);

            TPresenter presenter = new TPresenter();
            presenter.Init(view);
            presenter.SetIndex(idx);
            presenter.SetPositionY(GetChildPosY(idx));
            _visiblePresenters.Enqueue(presenter);

            return presenter;
        }

        int ClampIndex(int idx) => Mathf.Clamp(idx, 0, Mathf.Max(0, _maxCnt - _originVisibleCnt + 1));
        float GetContentPosY(int idx) => idx * (_rtView.sizeDelta.y + _spacing);
        float GetChildPosY(int idx) => _rtContent.sizeDelta.y / 2.0f - _rtView.sizeDelta.y / 2.0f - GetContentPosY(idx);
        void SetContentSize(int cnt) => _rtContent.SetSizeHeight(GetContentPosY(cnt) - _spacing / 2.0f);

        void SetMaxCount(int cnt)
        {
            if (_maxCnt == cnt)
                return;

            _maxCnt = cnt;
            SetContentSize(_maxCnt);
        }

        void SetVisibleCount(int cnt)
        {
            if (_visibleCnt == cnt)
                return;

            bool isIncrease = cnt > _visibleCnt;
            _visibleCnt = cnt;

            if (isIncrease)
                IncreaseVisibleChild();
            else
                DecreaseVisibleChild();
        }

        void IncreaseVisibleChild()
        {
            for (int i = _visiblePresenters.Count; i < _visibleCnt; i++)
            {
                CreatePresenter(_idx + i);
            }
        }

        void DecreaseVisibleChild()
        {
            for (int i = _visiblePresenters.Count; i > _visibleCnt; i--)
            {
                TPresenter child = _visiblePresenters.Dequeue();
                _viewPool.Return(child.View);
            }
        }

        public void OnScrollValueChanged(Vector2 value)
        {
            int curIdx = ClampIndex(Mathf.FloorToInt(_rtContent.anchoredPosition.y / (_rtView.sizeDelta.y + _spacing)));
            if (_idx == curIdx)
                return;

            bool isDown = _idx < curIdx;
        }
    }
}