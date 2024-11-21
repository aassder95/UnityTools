using TMPro;
using UnityEngine;
using UnityTools.Model;
using UnityTools.Util;

namespace UnityTools.UI
{
    public class RankItemView : MonoBehaviour, IDynamicScrollItem, IPoolable
    {
        [SerializeField] TextMeshProUGUI _txtId;
        [SerializeField] TextMeshProUGUI _txtRank;
        [SerializeField] TextMeshProUGUI _txtScore;
        [SerializeField] RectTransform _rtView;

        public int Index { get; set; }

        public void UpdateView(RankItemModel model)
        {
            _txtId.SetText("{0}", model?.Id ?? Index);
            _txtRank.SetText("{0}", model?.Rank ?? 0);
            _txtScore.SetText("{0}", model?.Score ?? 0);
        }

        void IDynamicScrollItem.SetPositionY(float y)
        {
            _rtView.SetAnchoredPositionY(y);
        }

        void IPoolable.OnGet()
        {
        }

        void IPoolable.OnReturn()
        {
        }
    }
}
