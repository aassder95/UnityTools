using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTools.Model;
using UnityTools.Util;

namespace UnityTools.UI
{
    public class RankItemView : BaseView<RankItemModel>, IDynamicScrollItem, IPoolable 
    {
        [SerializeField] private Color[] _colorTmp;
        [SerializeField] private TextMeshProUGUI _txtId;
        [SerializeField] private TextMeshProUGUI _txtRank;
        [SerializeField] private TextMeshProUGUI _txtScore;
        [SerializeField] private Image _imgTmp;
        [SerializeField] private RectTransform _rtView;

        public int Index { get; set; }

        public override void Refresh(RankItemModel model)
        {
            _txtId.SetText("{0}", model?.Id ?? Index);
            _txtRank.SetText("{0}", model?.Rank ?? 0);
            _txtScore.SetText("{0}", model?.Score ?? 0);
            _imgTmp.color = model.BgColor;
        }

        void IDynamicScrollItem.SetPosition(Vector2 pos)
        {
            _rtView.anchoredPosition = pos;
        }

        void IPoolable.OnGet() { }
        void IPoolable.OnReturn() { }
    }
}
