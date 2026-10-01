using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityTools.Benchmark.Samples
{
    public class UiLabBaseline : MonoBehaviour
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private UiLabItem _item;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _rtContent;
        [SerializeField] private RectTransform _rtViewport;

        //============================================================
        // Fields
        //============================================================
        private List<UiLabItem> _items;
        private float _itemHeight;
        private long _bindCnt;
        private int _createdItemCnt;
        private int _peakItemCnt;

        //============================================================
        // Properties
        //============================================================
        public int LiveItemCnt => _items == null ? 0 : _items.Count;
        public int CreatedItemCnt => _createdItemCnt;
        public int PeakItemCnt => _peakItemCnt;
        public long BindCnt => _bindCnt;
        public float ScrollPos => _rtContent.anchoredPosition.y;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnDestroy()
        {
            Release();
        }

        //============================================================
        // Init/Register
        //============================================================
        public void Init(UiLabScenario scenario, int mutationCnt)
        {
            _items = new List<UiLabItem>(scenario.ItemCnt + mutationCnt);
            _itemHeight = ((RectTransform)_item.transform).rect.height;
            _bindCnt = 0;
            _createdItemCnt = 0;
            _peakItemCnt = scenario.ItemCnt;
            _scrollRect.StopMovement();
            _scrollRect.enabled = false;
            for (int idx = 0; idx < scenario.ItemCnt; ++idx)
            {
                UiLabItem item = Instantiate(_item, _rtContent);
                item.Init();
                item.OnGet();
                _items.Add(item);
                ++_createdItemCnt;
            }

            LayoutItems(scenario);
            ScrollTo(0);
        }

        public void Release()
        {
            if (_items == null)
                return;

            for (int idx = 0; idx < _items.Count; ++idx)
            {
                _items[idx].OnReturn();
                Destroy(_items[idx].gameObject);
            }

            _items = null;
            _scrollRect.enabled = true;
            _scrollRect.StopMovement();
        }

        //============================================================
        // Logic
        //============================================================
        public void Advance(UiLabScenario scenario)
        {
            int delta = scenario.MutationDelta;
            if (delta > 0)
            {
                for (int idx = 0; idx < delta; ++idx)
                {
                    UiLabItem item = Instantiate(_item, _rtContent);
                    item.Init();
                    item.OnGet();
                    _items.Insert(idx, item);
                    ++_createdItemCnt;
                }
            }
            else if (delta < 0)
            {
                for (int idx = 0; idx < -delta; ++idx)
                {
                    _items[idx].OnReturn();
                    Destroy(_items[idx].gameObject);
                }

                _items.RemoveRange(0, -delta);
            }

            if (delta != 0)
            {
                float anchorPos = _rtContent.anchoredPosition.y + delta * _itemHeight;
                LayoutItems(scenario);
                _rtContent.anchoredPosition = new Vector2(0.0f, Mathf.Clamp(anchorPos, 0.0f, Mathf.Max(0.0f, _rtContent.rect.height - _rtViewport.rect.height)));
                _peakItemCnt = Mathf.Max(_peakItemCnt, _items.Count);
            }
            else
            {
                ScrollTo(scenario.TargetIdx);
            }
        }

        public void ScrollTo(int idx)
        {
            float maxPos = Mathf.Max(0.0f, _rtContent.rect.height - _rtViewport.rect.height);
            _rtContent.anchoredPosition = new Vector2(0.0f, Mathf.Clamp(idx * _itemHeight, 0.0f, maxPos));
        }

        //============================================================
        // Utilities
        //============================================================
        private void LayoutItems(UiLabScenario scenario)
        {
            _rtContent.sizeDelta = new Vector2(0.0f, _items.Count * _itemHeight);
            for (int idx = 0; idx < _items.Count; ++idx)
            {
                UiLabItem item = _items[idx];
                item.SetIdx(idx);
                item.SetPos(new Vector2(0.0f, -idx * _itemHeight));
                item.Bind(scenario.RowAt(idx));
                ++_bindCnt;
            }
        }
    }
}
