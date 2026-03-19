using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTools.Util;

namespace UnityTools.Samples.Rank
{
    public class RankItemView : BaseView<RankItemModel>, IDynamicScrollItem, IPoolable 
    {
        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private TextMeshProUGUI _txtId;
        [SerializeField] private TextMeshProUGUI _txtRank;
        [SerializeField] private TextMeshProUGUI _txtScore;
        [SerializeField] private Image _imgTmp;
        [SerializeField] private RectTransform _rtView;

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
        public override void Refresh(RankItemModel model)
        {
            _txtId.SetText("{0}", model?.Id ?? Index);
            _txtRank.SetText("{0}", model?.Rank ?? 0);
            _txtScore.SetText("{0}", model?.Score ?? 0);
            _imgTmp.color = model != null ? model.BgColor : Color.white;
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
