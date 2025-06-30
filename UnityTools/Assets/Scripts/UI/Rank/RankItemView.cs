using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTools.Model;
using UnityTools.Util;

namespace UnityTools.UI
{
    public class RankItemView : MonoBehaviour, IDynamicScrollItem, IPoolable, IView<RankItemModel>
    {
        [SerializeField] Color[] _colorTmp;
        [SerializeField] TextMeshProUGUI _txtId;
        [SerializeField] TextMeshProUGUI _txtRank;
        [SerializeField] TextMeshProUGUI _txtScore;
        [SerializeField] Image _imgTmp;
        [SerializeField] RectTransform _rtView;

        public int Index { get; set; }

        public void InitView(RankItemModel model) { }

        public void ShowView(RankItemModel model) { }

        public void HideView() { }

        public void UpdateView(RankItemModel model)
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
