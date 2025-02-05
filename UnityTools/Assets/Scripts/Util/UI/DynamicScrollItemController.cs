using UnityEngine;
using UnityEngine.Events;

namespace UnityTools.Util
{
    public interface IDynamicScrollItem
    {
        int Index { get; set; }
        void SetPosition(Vector2 pos);
    }

    public class DynamicScrollItemController<TView> where TView : Component, IDynamicScrollItem, IPoolable
    {
        readonly DynamicScrollContext _context;
        readonly ObjectPool<TView> _pool;
        Deque<TView> _items = new();

        public int FirstIndex => _items.Peek()?.Index ?? 0;
        public int Count => _items.Count;

        public event UnityAction<TView> OnItemUpdated;

        public DynamicScrollItemController(DynamicScrollContext context, ObjectPool<TView> pool)
        {
            _context = context;
            _pool = pool;
        }

        public TView Create(int idx)
        {
            TView item = _pool.Get();
            item.Index = idx;
            item.SetPosition(_context.CalculateItemPosition(idx));
            OnItemUpdated?.Invoke(item);
            return item;
        }

        public void Update()
        {
            _items.ForEach(item => OnItemUpdated?.Invoke(item));
        }

        public void UpdatePosition()
        {
            _items.ForEach(item => item.SetPosition(_context.CalculateItemPosition(item.Index)));
        }

        void Add(int idx, bool isBack)
        {
            if (isBack)
                _items.Enqueue(Create(idx));
            else
                _items.EnqueueFront(Create(idx));
        }

        public void AddRange(int cnt, int totalCnt)
        {
            bool isBack = FirstIndex + _items.Count < totalCnt;
            int idx = isBack ? FirstIndex + _items.Count : FirstIndex - 1;
            AddRange(cnt, idx, isBack);
        }

        public void AddRange(int cnt, int idx, bool isBack)
        {
            for (int j = 0; j < cnt; j++)
            {
                Add(idx + j, isBack);
            }
        }

        void Remove(bool isBack)
        {
            if (_items.Count <= 0)
                return;

            if (isBack)
                _pool.Return(_items.DequeueBack());
            else
                _pool.Return(_items.Dequeue());
        }

        public void RemoveRange(int cnt, int lastLine) => RemoveRange(cnt, FirstIndex >= _context.CalculateFirstVisibleItemIndex(lastLine));
        public void RemoveRange(int cnt, bool isBack)
        {
            for (int j = 0; j < cnt; j++)
            {
                Remove(isBack);
            }
        }

        public TView Get(int idx)
        {
            return _items.FirstOrDefault(item => item.Index == idx);
        }
    }
}
