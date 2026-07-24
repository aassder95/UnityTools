using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTools.Ui;

namespace UnityTools.Samples.Inven
{
    [RequireComponent(typeof(Image))]
    public class InvenItemView : BaseView<InvenItemModel>, IDynamicScrollItem
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private TextMeshProUGUI _txtIdx;

        //============================================================
        // Fields
        //============================================================
        private RectTransform _rtView;
        private Image _imgBg;
        private int _idx;

        //============================================================
        // Properties
        //============================================================
        public int Idx => _idx;

        //============================================================
        // Init/Register
        //============================================================
        protected override void OnInit()
        {
            _rtView = transform as RectTransform;
            _imgBg = GetComponent<Image>();
        }

        //============================================================
        // Logic
        //============================================================
        protected override void OnRefresh(InvenItemModel model)
        {
            _txtIdx.SetText($"{model.ItemName}\n{model.Grade}\nx{model.Cnt}");
            _txtIdx.color = ResolveGradeTextColor(model.Grade);
            _imgBg.color = ResolveGradeBgColor(model.Grade);
        }

        void IDynamicScrollItem.SetIdx(int idx)
        {
            _idx = idx;
        }

        void IDynamicScrollItem.SetPos(Vector2 pos)
        {
            _rtView.anchoredPosition = pos;
        }

        void IDynamicScrollItem.OnGet() { }
        void IDynamicScrollItem.OnReturn() { }

        //============================================================
        // Utilities
        //============================================================
        private static Color ResolveGradeTextColor(EInvenGrade grade)
        {
            switch (grade)
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
            switch (grade)
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
    }
}
