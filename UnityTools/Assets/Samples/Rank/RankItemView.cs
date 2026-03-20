using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTools.Util.Core.Pooling;
using UnityTools.Util.UIFramework;

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
        protected override void OnRefresh(RankItemModel model)
        {
            if(model == null)
                return;

            if(_txtId != null)
                _txtId.SetText("ID {0:000}", model.Id);

            if(_txtRank != null)
            {
                _txtRank.SetText("#{0}", model.Rank);
                _txtRank.color = ResolveRankColor(model.Rank);
            }

            if(_txtScore != null)
            {
                _txtScore.SetText("{0}", model.Score);
                _txtScore.color = ResolveScoreColor(model.Score);
            }

            if(_imgTmp != null)
                _imgTmp.color = Color.Lerp(model.BgColor, ResolveScoreColor(model.Score), 0.32f);
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
            if(_rtView == null)
                return;

            _rtView.anchoredPosition = pos;
        }

        //============================================================
        //Utilities
        //============================================================
        private static Color ResolveRankColor(int rank)
        {
            if(rank <= 1)
                return new Color(1f, 0.84f, 0.37f, 1f);

            if(rank <= 3)
                return new Color(0.56f, 0.89f, 1f, 1f);

            return new Color(0.88f, 0.92f, 1f, 1f);
        }

        private static Color ResolveScoreColor(int score)
        {
            float normalized = Mathf.InverseLerp(1f, 5000f, score);
            return Color.Lerp(new Color(0.66f, 0.8f, 1f, 1f), new Color(1f, 0.4f, 0.46f, 1f), normalized);
        }

        void IPoolable.OnGet() { }
        void IPoolable.OnReturn() { }
    }
}
