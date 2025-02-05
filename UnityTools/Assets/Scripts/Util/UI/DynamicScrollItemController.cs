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
        ObjectPool<TView> _pool;
        Deque<TView> _items = new();

        public int FirstIndex => _items.Peek()?.Index ?? 0;
        public int Count => _items.Count;

        public event UnityAction<TView> OnItemUpdated;

        public DynamicScrollItemController(DynamicScrollContext context, TView item, int visibleCnt, RectTransform rtContent)
        {
            _context = context;
            _pool = new ObjectPool<TView>(visibleCnt, item, rtContent);
        }

        public TView Create(int idx)
        {
            TView item = _pool.Get();
            item.Index = idx;
            item.SetPosition(_context.CalculateItemPosition(idx));
            OnItemUpdated?.Invoke(item);
            return item;
        }

        public void Clear()
        {
            _pool?.Clear();
        }

        public void Update()
        {
            _items.ForEach(item => OnItemUpdated?.Invoke(item));
        }

        public void UpdatePosition()
        {
            _items.ForEach(item => item.SetPosition(_context.CalculateItemPosition(item.Index)));
        }

        public void Add(bool isBack) => Add(isBack ? FirstIndex + _items.Count : FirstIndex - 1, isBack);
        public void Add(int idx, bool isBack)
        {
            if (isBack)
                _items.Enqueue(Create(idx));
            else
                _items.EnqueueFront(Create(idx));
        }

        public void Remove(bool isBack)
        {
            if (_items.Count <= 0)
                return;

            if (isBack)
                _pool.Return(_items.DequeueBack());
            else
                _pool.Return(_items.Dequeue());
        }

        public TView Get(int idx)
        {
            return _items.FirstOrDefault(item => item.Index == idx);
        }
    }
}

