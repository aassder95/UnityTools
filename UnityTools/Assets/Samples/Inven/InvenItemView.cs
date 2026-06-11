using TMPro;
using UnityEngine;
using UnityTools.Util.Core.Pooling;
using UnityTools.Util.UIFramework;

namespace UnityTools.Samples.Inven
{
    public class InvenItemView : BaseView<InvenItemModel>, IDynamicScrollItem, IPoolable
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private RectTransform _rtView;
        [SerializeField] private TextMeshProUGUI _txtIndex;

        //============================================================
        // Fields
        //============================================================
        private int _index;

        //============================================================
        // Properties
        //============================================================
        public int Index => _index;

        //============================================================
        // Logic
        //============================================================
        protected override void OnRefresh(InvenItemModel model)
        {
            if(model == null || _txtIndex == null)
                return;

            _txtIndex.SetText($"#{model.Id:000} [{model.Grade}] x{model.Count}  {model.ItemName}");
            _txtIndex.color = ResolveGradeColor(model.Grade);
        }

        //============================================================
        // Callbacks
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
            if(_rtView == null)
                return;

            _rtView.anchoredPosition = pos;
        }

        //============================================================
        // Utilities
        //============================================================
        private static Color ResolveGradeColor(InvenItemModel.EInvenGrade grade)
        {
            switch(grade)
            {
                case InvenItemModel.EInvenGrade.Common:
                    return new Color(0.78f, 0.82f, 0.9f, 1.0f);
                case InvenItemModel.EInvenGrade.Rare:
                    return new Color(0.47f, 0.76f, 1.0f, 1.0f);
                case InvenItemModel.EInvenGrade.Epic:
                    return new Color(0.92f, 0.53f, 1.0f, 1.0f);
                case InvenItemModel.EInvenGrade.Legendary:
                    return new Color(1.0f, 0.78f, 0.38f, 1.0f);
            }

            return Color.white;
        }

        void IPoolable.OnGet() { }
        void IPoolable.OnReturn() { }
    }
}
