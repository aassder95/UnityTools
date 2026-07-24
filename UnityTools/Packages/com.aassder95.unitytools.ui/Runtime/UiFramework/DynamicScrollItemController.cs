using UnityEngine;
using UnityEngine.Events;
using UnityTools.Util.Core.Collections;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Core.Pooling;

namespace UnityTools.Util.UiFramework
{
    public class DynamicScrollItemController<TView> where TView : Component, IDynamicScrollItem, IPoolable
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly DynamicScrollContext _context;
        private readonly ObjectPool<TView> _pool;
        private readonly Deque<TView> _items = new();

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
        public DynamicScrollItemController(DynamicScrollContext context, ObjectPool<TView> pool)
        {
            _context = context;
            _pool = pool;
        }

        //============================================================
        // Logic
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

        public void UpdateItems()
        {
            foreach (TView item in _items)
            {
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
                DebugLogger.LogError("DynamicScroll Item 추가 범위가 유효하지 않습니다. 개수=" + cnt + ", 전체 개수=" + totalCnt);
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
                    DebugLogger.LogError("DynamicScroll 뒤쪽 Item 추가 범위가 전체 개수를 벗어났습니다. 시작 인덱스=" + idx + ", 개수=" + cnt + ", 전체 개수=" + totalCnt);
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
                DebugLogger.LogError("DynamicScroll 앞쪽 Item 추가 범위가 0보다 작습니다. 시작 인덱스=" + frontIdx + ", 개수=" + cnt);
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
                DebugLogger.LogError("추가할 DynamicScroll Item 수는 0 이상이어야 합니다. 개수=" + cnt);
                return;
            }
            if (cnt == 0)
                return;
            if (idx < 0)
            {
                DebugLogger.LogError("추가할 DynamicScroll Item 인덱스는 0 이상이어야 합니다. 인덱스=" + idx);
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
                DebugLogger.LogError("DynamicScroll Item 제거 범위가 유효하지 않습니다. 개수=" + cnt + ", 현재 개수=" + _items.Count + ", 마지막 Line=" + lastLine);
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
                DebugLogger.LogError("제거할 DynamicScroll Item 수가 유효하지 않습니다. 개수=" + cnt + ", 현재 개수=" + _items.Count);
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

        //============================================================
        // Utilities
        //============================================================
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
                DebugLogger.LogError("DynamicScroll Item Collection 상태가 유효하지 않습니다.");
                return;
            }

            if (!_pool.TryReturn(item))
                DebugLogger.LogError("DynamicScroll Item을 ObjectPool에 반환하지 못했습니다.");
        }
    }
}
