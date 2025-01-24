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

        public event UnityAction<TView> OnItemUpdated;

        public DynamicScrollItemController(DynamicScrollContext context, RectTransform rtContent, TView item, int visibleCnt)
        {
            _context = context;
            _pool = new ObjectPool<TView>(rtContent, item, visibleCnt);
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

        public void Add(int totalCnt) => Add(FirstIndex + _items.Count < totalCnt);
        public void Add(bool isBack)
        {
            if (isBack)
                _items.Enqueue(Create(FirstIndex + _items.Count));
            else
                _items.EnqueueFront(Create(FirstIndex - 1));
        }

        public void Remove(int firstVisibleIdx) => Remove(FirstIndex >= firstVisibleIdx);
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

