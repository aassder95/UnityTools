using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityTools.UI;

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
        readonly Deque<TView> _items;

        public int FirstIndex => _items.Peek()?.Index ?? 0;

        public event UnityAction<TView> OnItemUpdated;

        public DynamicScrollItemController(DynamicScrollContext context, ObjectPool<TView> pool, Deque<TView> items)
        {
            _context = context;
            _pool = pool;
            _items = items;
        }

        public TView CreateItem(int idx)
        {
            TView item = _pool.Get();
            item.Index = idx;
            item.SetPosition(_context.CalculateItemPosition(idx));
            OnItemUpdated?.Invoke(item);
            return item;
        }

        public void UpdateAllItems()
        {
            _items.ForEach(item => OnItemUpdated?.Invoke(item));
        }

        public void UpdateAllItemsPosition()
        {
            _items.ForEach(item => item.SetPosition(_context.CalculateItemPosition(item.Index)));
        }

        public void AddItem(bool isBack)
        {
            if (isBack)
                _items.Enqueue(CreateItem(FirstIndex + _items.Count));
            else
                _items.EnqueueFront(CreateItem(FirstIndex - 1));
        }

        public void RemoveItem(bool isBack)
        {
            if (_items.Count <= 0)
                return;

            if (isBack)
                _pool.Return(_items.DequeueBack());
            else
                _pool.Return(_items.Dequeue());
        }

        public TView GetItem(int idx)
        {
            foreach (var item in _items)
            {
                if (item.Index == idx)
                    return item;
            }

            return null;
        }
    }
}

