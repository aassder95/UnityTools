using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTools.Samples.Util;
using UnityTools.Util.Core.Pooling;
using UnityTools.Util.UIFramework;

namespace UnityTools.Samples.Rank
{
    public class RankItemView : BaseView<RankItemModel>, IDynamicScrollItem, IPoolable
    {
        //============================================================
        // Constants
        //============================================================
        private const float RANK_WIDTH = 112.0f;
        private const float ID_WIDTH = 88.0f;
        private const float SCORE_WIDTH = 120.0f;
        private const float SCORE_RIGHT_PADDING = 14.0f;

        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private TextMeshProUGUI _txtId;
        [SerializeField] private TextMeshProUGUI _txtRank;
        [SerializeField] private TextMeshProUGUI _txtScore;
        [SerializeField] private Image _imgTmp;
        [SerializeField] private RectTransform _rtView;

        //============================================================
        // Fields
        //============================================================
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
            if(_rtView == null)
                _rtView = transform as RectTransform;

            ConfigureLayout();
            ConfigureTextStyle();
        }

        //============================================================
        // Logic
        //============================================================
        protected override void OnRefresh(RankItemModel model)
        {
            if(model == null)
                return;

            Color scoreColor = RankItemPalette.ResolveScoreColor(model.Score);
            if(_txtId != null)
            {
                _txtId.SetText("ID {0:000}", model.Id);
                _txtId.color = RankItemPalette.IdColor;
            }

            if(_txtRank != null)
            {
                _txtRank.SetText("#{0}", model.Rank);
                _txtRank.color = RankItemPalette.ResolveRankColor(model.Rank);
            }

            if(_txtScore != null)
            {
                _txtScore.SetText("{0:0000}", model.Score);
                _txtScore.color = scoreColor;
            }

            if(_imgTmp != null)
                _imgTmp.color = RankItemPalette.ResolveBgColor(model.BgColor);
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
                _rtView = transform as RectTransform;

            if(_rtView == null)
                return;

            _rtView.anchoredPosition = pos;
        }

        //============================================================
        // Utilities
        //============================================================
        private void ConfigureLayout()
        {
            SetColumnRect(_txtRank, 0.0f, RANK_WIDTH, true, false);
            SetColumnRect(_txtId, RANK_WIDTH, ID_WIDTH, true, false);
            SetColumnRect(_txtScore, SCORE_RIGHT_PADDING, SCORE_WIDTH, false, true);
        }

        private void ConfigureTextStyle()
        {
            ConfigureText(_txtRank, TextAlignmentOptions.MidlineLeft, 58.0f, 30.0f, 58.0f, FontStyles.Bold);
            ConfigureText(_txtId, TextAlignmentOptions.Center, 34.0f, 18.0f, 34.0f, FontStyles.Bold);
            ConfigureText(_txtScore, TextAlignmentOptions.MidlineRight, 44.0f, 24.0f, 44.0f, FontStyles.Bold);
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
                rt.anchorMin = new Vector2(0.0f, 0.0f);
                rt.anchorMax = new Vector2(0.0f, 1.0f);
                rt.pivot = new Vector2(0.0f, 0.5f);
                rt.anchoredPosition = new Vector2(startX, 0.0f);
                rt.sizeDelta = new Vector2(width, 0.0f);
                return;
            }

            if(isRightAnchor)
            {
                rt.anchorMin = new Vector2(1.0f, 0.0f);
                rt.anchorMax = new Vector2(1.0f, 1.0f);
                rt.pivot = new Vector2(1.0f, 0.5f);
                rt.anchoredPosition = new Vector2(-startX, 0.0f);
                rt.sizeDelta = new Vector2(width, 0.0f);
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
