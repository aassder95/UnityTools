using System;
using UnityEngine;
using UnityEngine.Events;

namespace UnityTools.Util
{
    public class DynamicScrollItemController<TView> where TView : Component, IDynamicScrollItem, IPoolable
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly DynamicScrollContext _context;
        private readonly ObjectPool<TView> _pool;
        private readonly Deque<TView> _items = new();

        //============================================================
        //Events
        //============================================================
        public event UnityAction<TView> OnItemUpdated { add => _onItemUpdated += value; remove => _onItemUpdated -= value; }
        private event UnityAction<TView> _onItemUpdated;

        //============================================================
        //Properties
        //============================================================
        public int FirstIndex
        {
            get
            {
                TView item = _items.Peek();
                return item == null ? 0 : item.GetIndex();
            }
        }
        public int Count => _items.Count;

        //============================================================
        //Constructors
        //============================================================
        public DynamicScrollItemController(DynamicScrollContext context, ObjectPool<TView> pool)
        {
            _context = context;
            _pool = pool;
        }

        //============================================================
        //Logic
        //============================================================
        public TView Create(int idx)
        {
            TView item = _pool.Get();
            item.SetIndex(idx);
            item.SetPosition(_context.CalculateItemPosition(idx));
            _onItemUpdated?.Invoke(item);
            return item;
        }

        public void Update()
        {
            foreach (TView item in _items)
                _onItemUpdated?.Invoke(item);
        }

        public void UpdatePosition()
        {
            foreach (TView item in _items)
                item.SetPosition(_context.CalculateItemPosition(item.GetIndex()));
        }

        public void AddRange(int cnt, int totalCnt)
        {
            bool isBack = FirstIndex + _items.Count < totalCnt;
            int idx = isBack ? FirstIndex + _items.Count : FirstIndex - 1;
            for(int j = 0; j < cnt; j++)
                Add(idx + j, isBack);
        }

        public void AddRange(int cnt, int idx, bool isBack)
        {
            for(int j = 0; j < cnt; j++)
                Add(idx + j, isBack);
        }

        public void RemoveRange(int cnt, int lastLine)
        {
            bool isBack = FirstIndex >= _context.CalculateFirstVisibleItemIndex(lastLine);
            for(int j = 0; j < cnt; j++)
                Remove(isBack);
        }

        public void RemoveRange(int cnt, bool isBack)
        {
            for(int j = 0; j < cnt; j++)
                Remove(isBack);
        }

        public TView Get(int idx)
        {
            foreach (TView item in _items)
            {
                if(item.GetIndex() == idx)
                    return item;
            }

            return null;
        }

        public TView Get(Func<TView, bool> cond)
        {
            if(cond == null)
                return null;

            foreach (TView item in _items)
            {
                if(cond(item))
                    return item;
            }

            return null;
        }

        private void Add(int idx, bool isBack)
        {
            if(isBack)
            {
                _items.Enqueue(Create(idx));
                return;
            }

            _items.EnqueueFront(Create(idx));
        }

        private void Remove(bool isBack)
        {
            if(_items.Count <= 0)
                return;

            if(isBack)
            {
                _pool.Return(_items.DequeueBack());
                return;
            }

            _pool.Return(_items.Dequeue());
        }
    }
}
