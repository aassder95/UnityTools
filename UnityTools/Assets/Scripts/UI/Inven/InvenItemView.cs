using TMPro;
using UnityEngine;
using UnityTools.Model;
using UnityTools.Util;

namespace UnityTools.UI
{
    public class InvenItemView : BaseView<InvenItemModel>, IDynamicScrollItem, IPoolable
    {
        [SerializeField] private RectTransform _rtView;
        [SerializeField] private TextMeshProUGUI _txtIndex;

        public int Index { get; set; }

        public override void Refresh(InvenItemModel model)
        {
            _txtIndex.SetText("{0}", model.Id);
        }

        void IDynamicScrollItem.SetPosition(Vector2 pos)
        {
            _rtView.anchoredPosition = pos;
        }

        void IPoolable.OnGet() { }
        void IPoolable.OnReturn() { }
    }
}
