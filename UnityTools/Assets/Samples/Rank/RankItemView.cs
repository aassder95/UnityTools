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
        //Constants
        //============================================================
        private const float RANK_WIDTH = 112f;
        private const float ID_WIDTH = 88f;
        private const float SCORE_WIDTH = 120f;
        private const float SCORE_RIGHT_PADDING = 14f;

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
        //Init/Register
        //============================================================
        protected override void OnInit()
        {
            if(_rtView == null)
                _rtView = transform as RectTransform;

            ConfigureLayout();
            ConfigureTextStyle();
        }

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
                _txtScore.SetText("{0:0000}", model.Score);
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
                _rtView = transform as RectTransform;

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

        private void ConfigureLayout()
        {
            SetColumnRect(_txtRank, 0f, RANK_WIDTH, true, false);
            SetColumnRect(_txtId, RANK_WIDTH, ID_WIDTH, true, false);
            SetColumnRect(_txtScore, SCORE_RIGHT_PADDING, SCORE_WIDTH, false, true);
        }

        private void ConfigureTextStyle()
        {
            ConfigureText(_txtRank, TextAlignmentOptions.MidlineLeft, 58f, 30f, 58f, FontStyles.Bold);
            ConfigureText(_txtId, TextAlignmentOptions.Center, 34f, 18f, 34f, FontStyles.Bold);
            ConfigureText(_txtScore, TextAlignmentOptions.MidlineRight, 44f, 24f, 44f, FontStyles.Bold);
        }

        private static void SetColumnRect(TextMeshProUGUI txtTarget, float startX, float width, bool isLeftAnchor, bool isRightAnchor)
        {
            if(txtTarget == null)
                return;

            RectTransform rt = txtTarget.rectTransform;
            if(rt == null)
                return;

            if(isLeftAnchor)
            {
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 0.5f);
                rt.anchoredPosition = new Vector2(startX, 0f);
                rt.sizeDelta = new Vector2(width, 0f);
                return;
            }

            if(isRightAnchor)
            {
                rt.anchorMin = new Vector2(1f, 0f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 0.5f);
                rt.anchoredPosition = new Vector2(-startX, 0f);
                rt.sizeDelta = new Vector2(width, 0f);
            }
        }

        private static void ConfigureText(TextMeshProUGUI txtTarget, TextAlignmentOptions alignment, float fontSizeMax, float fontSizeMin, float fontSize, FontStyles fontStyle)
        {
            if(txtTarget == null)
                return;

            txtTarget.alignment = alignment;
            txtTarget.fontStyle = fontStyle;
            txtTarget.enableAutoSizing = true;
            txtTarget.fontSize = fontSize;
            txtTarget.fontSizeMin = fontSizeMin;
            txtTarget.fontSizeMax = fontSizeMax;
            txtTarget.enableWordWrapping = false;
            txtTarget.overflowMode = TextOverflowModes.Truncate;
        }

        void IPoolable.OnGet() { }
        void IPoolable.OnReturn() { }
    }
}
