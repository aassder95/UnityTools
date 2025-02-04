using TMPro;
using UnityEngine;
using UnityTools.Util;

namespace UnityTools.UI
{
    public class InvenItemView : MonoBehaviour, IDynamicScrollItem, IPoolable
    {
        [SerializeField] RectTransform _rtView;
        [SerializeField] TextMeshProUGUI _txtIndex;

        public int Index { get; set; }

        public void UpdateView()
        {
            _txtIndex.SetText("{0}", Index);
        }

        void IDynamicScrollItem.SetPosition(Vector2 pos)
        {
            _rtView.anchoredPosition = pos;
        }

        void IPoolable.OnGet()
        {
        }

        void IPoolable.OnReturn()
        {
        }
    }
}
