using TMPro;
using UnityEngine;
using UnityTools.Util;

namespace UnityTools.UI
{
    public class InvenItemView : MonoBehaviour, IDynamicScrollItem, IPoolable, IView<int>
    {
        [SerializeField] RectTransform _rtView;
        [SerializeField] TextMeshProUGUI _txtIndex;

        int _idx;
        public int Index { get; set; }

        public void InitView(int model)
        {
            _idx = model;
        }

        public void UpdateView(int model)
        {
            _idx = model;
            _txtIndex.SetText("{0}", Index);
        }

        void IDynamicScrollItem.SetPosition(Vector2 pos)
        {
            _rtView.anchoredPosition = pos;
        }

        void IPoolable.OnGet() { }
        void IPoolable.OnReturn() { }
    }
}
