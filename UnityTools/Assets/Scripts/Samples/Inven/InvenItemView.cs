using TMPro;
using UnityEngine;
using UnityTools.Util;

namespace UnityTools.Samples.Inven
{
    public class InvenItemView : BaseView<InvenItemModel>, IDynamicScrollItem, IPoolable
    {
        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private RectTransform _rtView;
        [SerializeField] private TextMeshProUGUI _txtIndex;

        //============================================================
        //Fields
        //============================================================
        private int _index;

        //============================================================
        //Properties
        //============================================================
        public int Index => _index;

        //============================================================
        //Logic
        //============================================================
        public override void Refresh(InvenItemModel model)
        {
            _txtIndex.SetText("{0}", model.Id);
        }

        //============================================================
        //Callbacks
        //============================================================
        void IDynamicScrollItem.SetIndex(int index)
        {
            _index = index;
        }

        int IDynamicScrollItem.GetIndex()
        {
            return _index;
        }

        void IDynamicScrollItem.SetPosition(Vector2 pos)
        {
            _rtView.anchoredPosition = pos;
        }

        //============================================================
        //Utilities
        //============================================================
        void IPoolable.OnGet() { }
        void IPoolable.OnReturn() { }
    }
}
