using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTools.Samples.Util;
using UnityTools.Util.Core.Pooling;
using UnityTools.Util.UiFramework;

namespace UnityTools.Samples.Rank
{
    public class RankItemView : BaseView<RankItemModel>, IDynamicScrollItem, IPoolable
    {
        //============================================================
        // Constants
        //============================================================
        private const float RANK_WIDTH = 112.0f;

        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Info")]
        [SerializeField] private TextMeshProUGUI _txtId;
        [SerializeField] private TextMeshProUGUI _txtRank;
        [SerializeField] private TextMeshProUGUI _txtScore;
        [Header("Visual")]
        [SerializeField] private Image _imgBackground;

        //============================================================
        // Fields
        //============================================================
        private RectTransform _rtView;
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
            ConfigureLayout();
            ConfigureTextStyle();
        }

        //============================================================
        // Logic
        //============================================================
        protected override void OnRefresh(RankItemModel model)
        {
            Color scoreColor = RankItemPalette.ResolveScoreColor(model.Score);
            _txtId.SetText("ID {0:000}", model.Id);
            _txtId.color = RankItemPalette.IdColor;
            _txtRank.SetText("#{0}", model.Rank);
            _txtRank.color = RankItemPalette.ResolveRankColor(model.Rank);
            _txtScore.SetText("{0:0000}", model.Score);
            _txtScore.color = scoreColor;
            _imgBackground.color = RankItemPalette.ResolveBgColor(model.BgColor);
        }

        void IDynamicScrollItem.SetIdx(int idx)
        {
            _idx = idx;
        }

        void IDynamicScrollItem.SetPos(Vector2 pos)
        {
            _rtView.anchoredPosition = pos;
        }

        void IPoolable.OnGet() { }
        void IPoolable.OnReturn() { }

        //============================================================
        // Utilities
        //============================================================
        private void ConfigureLayout()
        {
            SetColumnRect(_txtRank, 0.0f, RANK_WIDTH, true, false);
            SetColumnRect(_txtId, RANK_WIDTH, 88.0f, true, false);
            SetColumnRect(_txtScore, 14.0f, 120.0f, false, true);
        }

        private void ConfigureTextStyle()
        {
            ConfigureText(_txtRank, TextAlignmentOptions.MidlineLeft, 58.0f, 30.0f, 58.0f, FontStyles.Bold);
            ConfigureText(_txtId, TextAlignmentOptions.Center, 34.0f, 18.0f, 34.0f, FontStyles.Bold);
            ConfigureText(_txtScore, TextAlignmentOptions.MidlineRight, 44.0f, 24.0f, 44.0f, FontStyles.Bold);
        }

        private static void SetColumnRect(TextMeshProUGUI txtTarget, float startX, float width, bool isLeftAnchor, bool isRightAnchor)
        {
            RectTransform rt = txtTarget.rectTransform;
            if (isLeftAnchor)
            {
                rt.anchorMin = new Vector2(0.0f, 0.0f);
                rt.anchorMax = new Vector2(0.0f, 1.0f);
                rt.pivot = new Vector2(0.0f, 0.5f);
                rt.anchoredPosition = new Vector2(startX, 0.0f);
                rt.sizeDelta = new Vector2(width, 0.0f);
                return;
            }

            if (isRightAnchor)
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
            txtTarget.alignment = alignment;
            txtTarget.fontStyle = fontStyle;
            txtTarget.enableAutoSizing = true;
            txtTarget.fontSize = fontSize;
            txtTarget.fontSizeMin = fontSizeMin;
            txtTarget.fontSizeMax = fontSizeMax;
            txtTarget.enableWordWrapping = false;
            txtTarget.overflowMode = TextOverflowModes.Truncate;
        }
    }
}
