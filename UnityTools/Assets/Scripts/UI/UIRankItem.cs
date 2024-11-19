using TMPro;
using UnityEngine;
using UnityTools.Manager;
using UnityTools.Model;
using UnityTools.Util;

namespace UnityTools.UI
{
    public class UIRankItem : MonoBehaviour, IDynamicScrollViewChild, IPoolable
    {
        [SerializeField] TextMeshProUGUI _txtId;
        [SerializeField] TextMeshProUGUI _txtRank;
        [SerializeField] TextMeshProUGUI _txtScore;
        [SerializeField] RectTransform _rtItem;

        RankItemModel _model;
        int _idx;

        void IDynamicScrollViewChild.UpdateView()
        {
            _model = RankManager.Instance.GetModel(_idx);
            if (_model == null)
                return;

            _txtId.SetText("{0}", _model.Id);
            _txtRank.SetText("{0}", _model.Rank);
            _txtScore.SetText("{0}", _model.Score);
        }

        void IDynamicScrollViewChild.SetIndex(int idx)
        {
            _idx = idx;
        }

        void IDynamicScrollViewChild.SetPositionY(float y)
        {
            _rtItem.SetAnchoredPositionY(y);
        }

        void IPoolable.OnGet()
        {
        }

        void IPoolable.OnReturn()
        {
        }
    }
}