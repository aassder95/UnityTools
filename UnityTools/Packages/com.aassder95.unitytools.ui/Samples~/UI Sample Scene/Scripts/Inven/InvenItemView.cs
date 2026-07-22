using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTools.Util.Core.Pooling;
using UnityTools.Util.UIFramework;

namespace UnityTools.Samples.Inven
{
    [RequireComponent(typeof(Image))]
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
        private Image _imgBg;
        private int _index;

        //============================================================
        // Properties
        //============================================================
        public int Index => _index;

        //============================================================
        // Init/Register
        //============================================================
        protected override void OnInit()
        {
            _imgBg = GetComponent<Image>();
        }

        //============================================================
        // Logic
        //============================================================
        protected override void OnRefresh(InvenItemModel model)
        {
            if(model == null || _txtIndex == null)
                return;

            _txtIndex.SetText($"{model.ItemName}\n{model.Grade}\nx{model.Count}");
            _txtIndex.color = ResolveGradeTextColor(model.Grade);
            _imgBg.color = ResolveGradeBgColor(model.Grade);
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
        private static Color ResolveGradeTextColor(EInvenGrade grade)
        {
            switch(grade)
            {
                case EInvenGrade.Common:
                    return new Color(0.24f, 0.31f, 0.43f, 1.0f);
                case EInvenGrade.Rare:
                    return new Color(0.02f, 0.34f, 0.64f, 1.0f);
                case EInvenGrade.Epic:
                    return new Color(0.48f, 0.12f, 0.62f, 1.0f);
                case EInvenGrade.Legendary:
                    return new Color(0.57f, 0.31f, 0.02f, 1.0f);
            }

            return new Color(0.16f, 0.2f, 0.28f, 1.0f);
        }

        private static Color ResolveGradeBgColor(EInvenGrade grade)
        {
            switch(grade)
            {
                case EInvenGrade.Common:
                    return new Color(0.93f, 0.95f, 0.98f, 1.0f);
                case EInvenGrade.Rare:
                    return new Color(0.88f, 0.95f, 1.0f, 1.0f);
                case EInvenGrade.Epic:
                    return new Color(0.97f, 0.91f, 1.0f, 1.0f);
                case EInvenGrade.Legendary:
                    return new Color(1.0f, 0.95f, 0.84f, 1.0f);
            }

            return Color.white;
        }

        void IPoolable.OnGet() { }
        void IPoolable.OnReturn() { }
    }
}
