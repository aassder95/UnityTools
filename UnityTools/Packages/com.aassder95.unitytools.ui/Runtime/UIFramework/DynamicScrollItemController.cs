using System;
using UnityEngine;
using UnityEngine.Events;
using UnityTools.Util.Core.Collections;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Core.Pooling;

namespace UnityTools.Util.UIFramework
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
        public int FirstIdx
        {
            get
            {
                return _items.IsEmpty ? 0 : _items.Peek().Idx;
            }
        }

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
        public bool TryCreate(int idx, out TView item)
        {
            item = null;
            if(idx < 0)
            {
                DebugLogger.LogError("DynamicScroll Item 인덱스는 0 이상이어야 합니다. 인덱스=" + idx);
                return false;
            }

            if(!_pool.TryGet(out TView newItem))
                return false;

            if(!newItem.Init())
            {
                if(!_pool.TryReturn(newItem))
                    DebugLogger.LogError("초기화에 실패한 DynamicScroll Item을 풀로 반환하지 못했습니다. 인덱스=" + idx);

                return false;
            }

            newItem.SetIdx(idx);
            newItem.SetPos(_context.GetItemPos(idx));
            if(TryNotifyUpdated(newItem))
            {
                item = newItem;
                return true;
            }

            if(!_pool.TryReturn(newItem))
                DebugLogger.LogError("갱신에 실패한 DynamicScroll Item을 풀로 반환하지 못했습니다. 인덱스=" + idx);

            return false;
        }

        public bool TryUpdateItems()
        {
            foreach(TView item in _items)
            {
                if(!TryNotifyUpdated(item))
                    return false;
            }

            return true;
        }

        public void UpdatePos()
        {
            foreach(TView item in _items)
            {
                item.SetPos(_context.GetItemPos(item.Idx));
            }
        }

        public bool TryAddRange(int cnt, int totalCnt)
        {
            if(cnt < 0)
            {
                DebugLogger.LogError("추가할 DynamicScroll Item 수는 0 이상이어야 합니다. 개수=" + cnt);
                return false;
            }
            if(cnt == 0)
                return true;

            bool isBack = FirstIdx + _items.Count < totalCnt;
            if(isBack)
            {
                int idx = FirstIdx + _items.Count;
                for(int i = 0; i < cnt; i++)
                {
                    if(!TryAdd(idx + i, true))
                    {
                        if(!TryRollbackAdded(i, true))
                            DebugLogger.LogError("DynamicScroll 뒤쪽 Item 추가 롤백에 실패했습니다. 추가 수=" + i);
                        return false;
                    }
                }

                return true;
            }

            int frontIdx = FirstIdx - 1;
            for(int i = 0; i < cnt; i++)
            {
                if(!TryAdd(frontIdx - i, false))
                {
                    if(!TryRollbackAdded(i, false))
                        DebugLogger.LogError("DynamicScroll 앞쪽 Item 추가 롤백에 실패했습니다. 추가 수=" + i);
                    return false;
                }
            }

            return true;
        }

        public bool TryAddRange(int cnt, int idx, bool isBack)
        {
            if(cnt < 0)
            {
                DebugLogger.LogError("추가할 DynamicScroll Item 수는 0 이상이어야 합니다. 개수=" + cnt);
                return false;
            }
            if(cnt == 0)
                return true;

            if(isBack)
            {
                for(int i = 0; i < cnt; i++)
                {
                    if(!TryAdd(idx + i, true))
                    {
                        if(!TryRollbackAdded(i, true))
                            DebugLogger.LogError("DynamicScroll 뒤쪽 Item 추가 롤백에 실패했습니다. 추가 수=" + i);
                        return false;
                    }
                }

                return true;
            }

            int addedCnt = 0;
            for(int i = cnt - 1; i >= 0; i--)
            {
                if(!TryAdd(idx + i, false))
                {
                    if(!TryRollbackAdded(addedCnt, false))
                        DebugLogger.LogError("DynamicScroll 앞쪽 Item 추가 롤백에 실패했습니다. 추가 수=" + addedCnt);
                    return false;
                }

                addedCnt++;
            }

            return true;
        }

        public bool TryRemoveRange(int cnt, int lastLine)
        {
            if(cnt < 0)
            {
                DebugLogger.LogError("제거할 DynamicScroll Item 수는 0 이상이어야 합니다. 개수=" + cnt);
                return false;
            }
            if(cnt == 0)
                return true;

            bool isBack = FirstIdx >= _context.GetFirstVisibleItemIdx(lastLine);
            for(int i = 0; i < cnt; i++)
            {
                if(!TryRemove(isBack))
                    return false;
            }

            return true;
        }

        public bool TryRemoveRange(int cnt, bool isBack)
        {
            if(cnt < 0)
            {
                DebugLogger.LogError("제거할 DynamicScroll Item 수는 0 이상이어야 합니다. 개수=" + cnt);
                return false;
            }
            if(cnt == 0)
                return true;

            for(int i = 0; i < cnt; i++)
            {
                if(!TryRemove(isBack))
                    return false;
            }

            return true;
        }

        public bool TryClear()
        {
            while(_items.Count > 0)
            {
                if(!TryRemove(false))
                    return false;
            }

            return true;
        }

        //============================================================
        // Utilities
        //============================================================
        private bool TryAdd(int idx, bool isBack)
        {
            if(idx < 0)
            {
                DebugLogger.LogError("추가할 DynamicScroll Item 인덱스가 범위를 벗어났습니다. 인덱스=" + idx);
                return false;
            }

            if(!TryCreate(idx, out TView item))
                return false;

            if(isBack)
                _items.Enqueue(item);
            else
                _items.EnqueueFront(item);

            return true;
        }

        private bool TryRollbackAdded(int addedCnt, bool isBack)
        {
            bool isSuccess = true;
            for(int i = 0; i < addedCnt; i++)
            {
                if(!TryRemove(isBack))
                    isSuccess = false;
            }

            return isSuccess;
        }

        private bool TryRemove(bool isBack)
        {
            if(_items.Count <= 0)
                return true;

            TView item = isBack ? _items.PeekBack() : _items.Peek();
            if(!_pool.TryReturn(item))
                return false;

            if(isBack)
                _items.DequeueBack();
            else
                _items.Dequeue();

            return true;
        }

        private bool TryNotifyUpdated(TView item)
        {
            if(_onItemUpdated == null)
                return true;

            Delegate[] listeners = _onItemUpdated.GetInvocationList();
            for(int i = 0; i < listeners.Length; i++)
            {
                try
                {
                    ((UnityAction<TView>)listeners[i]).Invoke(item);
                }
                catch(Exception exception)
                {
                    DebugLogger.LogError("DynamicScroll Item 갱신 Listener 실행에 실패했습니다. 원인=" + exception.Message);
                    return false;
                }
            }

            return true;
        }
    }
}
