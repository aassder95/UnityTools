using System;
using UnityEngine;
using UnityEngine.Events;
using UnityTools.Util.Core.Collections;
using UnityTools.Util.Core.Pooling;

namespace UnityTools.Util.UIFramework
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
            foreach(TView item in _items)
                _onItemUpdated?.Invoke(item);
        }

        public void UpdatePosition()
        {
            foreach(TView item in _items)
                item.SetPosition(_context.CalculateItemPosition(item.GetIndex()));
        }

        public void AddRange(int cnt, int totalCnt)
        {
            if(cnt <= 0)
                return;

            bool isBack = FirstIndex + _items.Count < totalCnt;
            if(isBack)
            {
                int idx = FirstIndex + _items.Count;
                for(int j = 0; j < cnt; j++)
                    Add(idx + j, true);
                return;
            }

            int frontIdx = FirstIndex - 1;
            for(int j = 0; j < cnt; j++)
                Add(frontIdx - j, false);
        }

        public void AddRange(int cnt, int idx, bool isBack)
        {
            if(cnt <= 0)
                return;

            if(isBack)
            {
                for(int j = 0; j < cnt; j++)
                    Add(idx + j, true);
                return;
            }

            for(int j = cnt - 1; j >= 0; j--)
                Add(idx + j, false);
        }

        public void RemoveRange(int cnt, int lastLine)
        {
            if(cnt <= 0)
                return;

            bool isBack = FirstIndex >= _context.CalculateFirstVisibleItemIndex(lastLine);
            for(int j = 0; j < cnt; j++)
                Remove(isBack);
        }

        public void RemoveRange(int cnt, bool isBack)
        {
            if(cnt <= 0)
                return;

            for(int j = 0; j < cnt; j++)
                Remove(isBack);
        }

        public void Clear()
        {
            while(_items.Count > 0)
                _pool.Return(_items.Dequeue());
        }

        public TView Get(int idx)
        {
            foreach(TView item in _items)
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

            foreach(TView item in _items)
            {
                if(cond(item))
                    return item;
            }

            return null;
        }

        //============================================================
        //Utilities
        //============================================================
        private void Add(int idx, bool isBack)
        {
            if(idx < 0)
                return;

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
