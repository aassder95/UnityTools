using TMPro;
using UnityEngine;
using UnityTools.Model;
using UnityTools.Util;

namespace UnityTools.UI
{
    public class RankItemView : MonoBehaviour, IDynamicScrollItem<RankItemModel>, IPoolable
    {
        [SerializeField] TextMeshProUGUI _txtId;
        [SerializeField] TextMeshProUGUI _txtRank;
        [SerializeField] TextMeshProUGUI _txtScore;
        [SerializeField] RectTransform _rtView;

        void IDynamicScrollItem<RankItemModel>.SetData(RankItemModel model)
        {
            _txtId.SetText("{0}", model.Id);
            _txtRank.SetText("{0}", model.Rank);
            _txtScore.SetText("{0}", model.Score);
        }

        void IDynamicScrollItem<RankItemModel>.SetPositionY(float y)
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
