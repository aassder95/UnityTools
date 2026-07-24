using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityTools.Ui
{
    public class DynamicScrollItemController<TView> where TView : Component, IDynamicScrollItem
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly DynamicScrollContext _context;
        private readonly ItemPool _pool;
        private readonly ItemDeque<TView> _items = new();

        //============================================================
        // Events
        //============================================================
        private event UnityAction<TView> _onItemUpdated;
        public event UnityAction<TView> OnItemUpdated { add => _onItemUpdated += value; remove => _onItemUpdated -= value; }

        //============================================================
        // Properties
        //============================================================
        public int FirstIdx => _items.TryPeek(out TView item) ? item.Idx : 0;
        public int Cnt => _items.Count;

        //============================================================
        // Constructors
        //============================================================
        public DynamicScrollItemController(DynamicScrollContext context, int initialItemCnt, TView prefab, Transform parent)
        {
            _context = context;
            _pool = new ItemPool(initialItemCnt, prefab, parent);
        }

        //============================================================
        // Logic
        //============================================================
        public void UpdateItems()
        {
            foreach (TView item in _items)
            {
                _onItemUpdated?.Invoke(item);
            }
        }

        public void UpdateItem(int itemIdx)
        {
            foreach (TView item in _items)
            {
                if (item.Idx == itemIdx)
                {
                    _onItemUpdated?.Invoke(item);
                    return;
                }
            }
        }

        public void UpdateRange(int startIdx, int cnt)
        {
            int endIdx = startIdx + cnt;
            foreach (TView item in _items)
            {
                if (item.Idx >= startIdx && item.Idx < endIdx)
                    _onItemUpdated?.Invoke(item);
            }
        }

        public void UpdatePos()
        {
            foreach (TView item in _items)
            {
                item.SetPos(_context.GetItemPos(item.Idx));
            }
        }

        public void AddRange(int cnt, int totalCnt)
        {
            if (cnt < 0 || totalCnt < 0 || cnt > totalCnt)
            {
                Debug.LogError("DynamicScroll Item 추가 범위가 유효하지 않습니다. 개수=" + cnt + ", 전체 개수=" + totalCnt);
                return;
            }

            if (cnt == 0)
                return;

            bool isBack = FirstIdx + _items.Count < totalCnt;
            if (isBack)
            {
                int idx = FirstIdx + _items.Count;
                if (idx + cnt > totalCnt)
                {
                    Debug.LogError("DynamicScroll 뒤쪽 Item 추가 범위가 전체 개수를 벗어났습니다. 시작 인덱스=" + idx + ", 개수=" + cnt + ", 전체 개수=" + totalCnt);
                    return;
                }

                for (int i = 0; i < cnt; i++)
                {
                    Add(idx + i, true);
                }

                return;
            }

            int frontIdx = FirstIdx - 1;
            if (frontIdx - cnt + 1 < 0)
            {
                Debug.LogError("DynamicScroll 앞쪽 Item 추가 범위가 0보다 작습니다. 시작 인덱스=" + frontIdx + ", 개수=" + cnt);
                return;
            }

            for (int i = 0; i < cnt; i++)
            {
                Add(frontIdx - i, false);
            }
        }

        public void AddRange(int cnt, int idx, bool isBack)
        {
            if (cnt < 0)
            {
                Debug.LogError("추가할 DynamicScroll Item 수는 0 이상이어야 합니다. 개수=" + cnt);
                return;
            }
            if (cnt == 0)
                return;
            if (idx < 0)
            {
                Debug.LogError("추가할 DynamicScroll Item 인덱스는 0 이상이어야 합니다. 인덱스=" + idx);
                return;
            }

            if (isBack)
            {
                for (int i = 0; i < cnt; i++)
                {
                    Add(idx + i, true);
                }

                return;
            }

            for (int i = cnt - 1; i >= 0; i--)
            {
                Add(idx + i, false);
            }
        }

        public void RemoveRange(int cnt, int lastLine)
        {
            if (cnt < 0 || cnt > _items.Count || lastLine < 0)
            {
                Debug.LogError("DynamicScroll Item 제거 범위가 유효하지 않습니다. 개수=" + cnt + ", 현재 개수=" + _items.Count + ", 마지막 Line=" + lastLine);
                return;
            }

            if (cnt == 0)
                return;

            bool isBack = FirstIdx >= _context.GetFirstVisibleItemIdx(lastLine);
            for (int i = 0; i < cnt; i++)
            {
                Remove(isBack);
            }
        }

        public void RemoveRange(int cnt, bool isBack)
        {
            if (cnt < 0 || cnt > _items.Count)
            {
                Debug.LogError("제거할 DynamicScroll Item 수가 유효하지 않습니다. 개수=" + cnt + ", 현재 개수=" + _items.Count);
                return;
            }

            if (cnt == 0)
                return;

            for (int i = 0; i < cnt; i++)
            {
                Remove(isBack);
            }
        }

        public void Clear()
        {
            while (_items.Count > 0)
            {
                Remove(false);
            }
        }

        public void Release()
        {
            Clear();
            _pool.Clear();
        }

        //============================================================
        // Utilities
        //============================================================
        private TView Create(int idx)
        {
            TView item = _pool.Get();
            item.Init();
            item.SetIdx(idx);
            item.SetPos(_context.GetItemPos(idx));
            _onItemUpdated?.Invoke(item);
            return item;
        }

        private void Add(int idx, bool isBack)
        {
            TView item = Create(idx);
            if (isBack)
                _items.Enqueue(item);
            else
                _items.EnqueueFront(item);
        }

        private void Remove(bool isBack)
        {
            TView item;
            bool hasItem = isBack ? _items.TryDequeueBack(out item) : _items.TryDequeue(out item);
            if (!hasItem)
            {
                Debug.LogError("DynamicScroll Item Collection 상태가 유효하지 않습니다.");
                return;
            }

            if (!_pool.TryReturn(item))
                Debug.LogError("DynamicScroll Item을 Pool에 반환하지 못했습니다.");
        }

        //============================================================
        // Nested Types
        //============================================================
        private class ItemPool
        {
            //============================================================
            // Readonly
            //============================================================
            private readonly TView _prefab;
            private readonly Transform _parent;
            private readonly Queue<TView> _availableItems = new();
            private readonly HashSet<TView> _createdItems = new();
            private readonly HashSet<TView> _pooledItems = new();

            //============================================================
            // Constructors
            //============================================================
            public ItemPool(int initialItemCnt, TView prefab, Transform parent)
            {
                _prefab = prefab;
                _parent = parent;
                for (int i = 0; i < initialItemCnt; i++)
                {
                    TView item = CreateItem();
                    _availableItems.Enqueue(item);
                    _pooledItems.Add(item);
                }
            }

            //============================================================
            // Logic
            //============================================================
            public TView Get()
            {
                TView item;
                if (_availableItems.Count > 0)
                {
                    item = _availableItems.Dequeue();
                    _pooledItems.Remove(item);
                }
                else
                {
                    item = CreateItem();
                }

                item.gameObject.SetActive(true);
                item.OnGet();
                return item;
            }

            public bool TryReturn(TView item)
            {
                if (item == null)
                {
                    Debug.LogError("Pool에 반환할 DynamicScroll Item이 비어 있습니다.");
                    return false;
                }

                if (!_createdItems.Contains(item))
                {
                    Debug.LogError("다른 Pool이 소유한 DynamicScroll Item을 반환할 수 없습니다. 이름=" + item.name);
                    return false;
                }

                if (_pooledItems.Contains(item))
                {
                    Debug.LogError("이미 Pool에 들어 있는 DynamicScroll Item을 중복 반환했습니다. 이름=" + item.name);
                    return false;
                }

                item.OnReturn();
                item.gameObject.SetActive(false);
                _availableItems.Enqueue(item);
                _pooledItems.Add(item);
                return true;
            }

            public void Clear()
            {
                while (_availableItems.Count > 0)
                {
                    TView item = _availableItems.Dequeue();
                    _pooledItems.Remove(item);
                    _createdItems.Remove(item);
                    if (item != null)
                        Object.Destroy(item.gameObject);
                }

                _pooledItems.Clear();
            }

            //============================================================
            // Utilities
            //============================================================
            private TView CreateItem()
            {
                TView item = Object.Instantiate(_prefab, _parent);
                _createdItems.Add(item);
                item.gameObject.SetActive(false);
                return item;
            }
        }

        private class ItemDeque<TItem> : IEnumerable<TItem>
        {
            //============================================================
            // Readonly
            //============================================================
            private readonly LinkedList<TItem> _items = new();

            //============================================================
            // Properties
            //============================================================
            public int Count => _items.Count;

            //============================================================
            // Logic
            //============================================================
            public void Enqueue(TItem item)
            {
                _items.AddLast(item);
            }

            public void EnqueueFront(TItem item)
            {
                _items.AddFirst(item);
            }

            public bool TryDequeue(out TItem item)
            {
                if (_items.Count == 0)
                {
                    item = default;
                    return false;
                }

                item = _items.First.Value;
                _items.RemoveFirst();
                return true;
            }

            public bool TryDequeueBack(out TItem item)
            {
                if (_items.Count == 0)
                {
                    item = default;
                    return false;
                }

                item = _items.Last.Value;
                _items.RemoveLast();
                return true;
            }

            public bool TryPeek(out TItem item)
            {
                if (_items.Count == 0)
                {
                    item = default;
                    return false;
                }

                item = _items.First.Value;
                return true;
            }

            //============================================================
            // Utilities
            //============================================================
            public IEnumerator<TItem> GetEnumerator()
            {
                foreach (TItem item in _items)
                {
                    yield return item;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
