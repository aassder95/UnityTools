using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityTools.Util.Constants;
using UnityTools.Util.Core.Collections;
using UnityTools.Util.Core.Events;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Core.Persistence;
using UnityTools.Util.Core.Pooling;
using UnityTools.Util.Core.Singleton;
using UnityTools.Util.Core.State;
using UnityTools.Util.Core.Timer.Period;
using UnityTools.Util.Core.Timer.Shared;
using UnityTools.Util.Core.Timer.Task;
using UnityTools.Util.Coroutines;
using UnityTools.Util.Extensions;
using UnityTools.Util.UIFramework;
using UnityTools.Util.Utilities;

namespace UnityTools.Util.UIFramework
{
    [RequireComponent(typeof(ScrollRect))]
    public class DynamicScrollView<TView> : MonoBehaviour where TView : Component, IDynamicScrollItem, IPoolable
    {
        //============================================================
        //Constants
        //============================================================
        private const int MIN_VISIBLE_LINE_CNT = 1;
        private const int MIN_FIXED_CELLS_PER_GROUP = 1;
        private const int DEFAULT_EXTRA_VISIBLE_LINE_CNT = 1;
        private const float SCROLL_REFRESH_EPSILON = 0.01f;
        private const float MIN_SCROLL_DURATION_SEC = 0.0001f;

        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private TView _item;
        [SerializeField] private EDynamicScrollAxisType _axisType = EDynamicScrollAxisType.Vertical;
        [SerializeField] private EDynamicScrollLayoutMode _layoutMode = EDynamicScrollLayoutMode.Single;
        [SerializeField] private int _fixedCellsPerGroup = 1;
        [SerializeField] private EDynamicScrollMovementType _movementType = EDynamicScrollMovementType.Clamped;
        [SerializeField] private bool _inertia = true;
        [SerializeField] [Range(0.001f, 0.3f)] private float _decelerationRate = 0.135f;
        [SerializeField] [Range(0.1f, 1.0f)] private float _elasticity = 0.1f;
        [SerializeField] private EDynamicScrollContentAlignment _contentAlignment = EDynamicScrollContentAlignment.TopLeft;
        [SerializeField] private Vector2 _spacing;
        [SerializeField] private RectOffset _padding;

        //============================================================
        //Fields
        //============================================================
        private DynamicScrollContext _context;
        private DynamicScrollItemController<TView> _itemCtrl;
        private ObjectPool<TView> _pool;
        private int _totalItemCnt;
        private int _totalLineCnt;
        private int _visibleLineCnt = MIN_VISIBLE_LINE_CNT;
        private int _itemCntPerLine = MIN_FIXED_CELLS_PER_GROUP;
        private RectTransform _rt;
        private RectTransform _rtContent;
        private RectTransform _rtItem;
        private ScrollRect _scrollRect;
        private UnityAction<TView> _onControllerItemUpdated;
        private bool _isInitialized;
        private float _lastScrollPos = float.MinValue;
        private Coroutine _coSmoothScroll;

        //============================================================
        //Events
        //============================================================
        public event UnityAction<TView> OnItemUpdated { add => _onItemUpdated.AddListener(value); remove => _onItemUpdated.RemoveListener(value); }
        private readonly UnityEvent<TView> _onItemUpdated = new();

        //============================================================
        //Properties
        //============================================================
        protected DynamicScrollContext Context => _context;
        protected DynamicScrollItemController<TView> ItemController => _itemCtrl;
        protected int TotalLineCount => _totalLineCnt;
        protected int VisibleLineCount => _visibleLineCnt;
        protected int TotalItemCount => _totalItemCnt;
        protected int ItemCountPerLine => _itemCntPerLine;
        protected RectTransform RectTransform => _rt;
        protected RectTransform RtContent => _rtContent;
        protected ScrollRect ScrollRect => _scrollRect;
        protected bool IsVertical => _axisType == EDynamicScrollAxisType.Vertical;

        //============================================================
        //Unity Methods
        //============================================================
        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
            _scrollRect = GetComponent<ScrollRect>();
            _rtContent = _scrollRect.content;
            if(!ValidateInspectorFields())
            {
                enabled = false;
                return;
            }

            ApplyScrollRectSettings();
            ApplyContentAlignment();

            _itemCntPerLine = ResolveItemCountPerLine();
            _context = new DynamicScrollContext(_itemCntPerLine, _spacing, _padding, _rtItem, _scrollRect);
            UpdateAutoVisibleLineCount();

            int initialItemCnt = Mathf.Max(_visibleLineCnt * _itemCntPerLine, _itemCntPerLine);
            _pool = new ObjectPool<TView>(initialItemCnt, _item, _rtContent);
            _itemCtrl = new DynamicScrollItemController<TView>(_context, _pool);
            _onControllerItemUpdated = itemView => _onItemUpdated?.Invoke(itemView);

            _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            _itemCtrl.OnItemUpdated += _onControllerItemUpdated;
        }

        private void OnRectTransformDimensionsChange()
        {
            if(!_isInitialized || _context == null)
                return;

            int prevItemCntPerLine = _itemCntPerLine;
            int prevVisibleLineCnt = _visibleLineCnt;

            UpdateLayoutConfig();
            if(prevItemCntPerLine == _itemCntPerLine && prevVisibleLineCnt == _visibleLineCnt)
                return;

            UpdateContentLayout();
            RebuildVisibleItems();
        }

        private void OnDestroy()
        {
            StopSmoothScroll();

            if(_scrollRect != null)
                _scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);

            if(_itemCtrl != null)
                _itemCtrl.OnItemUpdated -= _onControllerItemUpdated;

            _pool?.Clear();
        }

        //============================================================
        //Logic
        //============================================================
        public void InitView(int totalItemCnt)
        {
            _isInitialized = true;
            UpdateLayoutConfig();

            int nextTotalItemCnt = Mathf.Max(0, totalItemCnt);
            bool isSameTotalItemCnt = nextTotalItemCnt == _totalItemCnt;
            SetTotalItemCount(nextTotalItemCnt);
            if(isSameTotalItemCnt)
                RebuildVisibleItems();
        }

        public void RefreshView()
        {
            if(!_isInitialized)
                return;

            RebuildVisibleItems();
        }

        public void ScrollTo(int itemIdx, bool immediate = false, float durationSec = 0.3f)
        {
            if(_context == null || _totalItemCnt <= 0)
                return;

            int targetIdx = Mathf.Clamp(itemIdx, 0, _totalItemCnt - 1);
            Vector2 targetPos = _context.CalculateContentPosition(targetIdx);
            targetPos = _context.ClampContentPosition(targetPos, _totalLineCnt, _visibleLineCnt);

            if(immediate || durationSec <= MIN_SCROLL_DURATION_SEC)
            {
                StopSmoothScroll();
                _rtContent.anchoredPosition = targetPos;
                OnScrollValueChanged(Vector2.zero);
                return;
            }

            StartSmoothScroll(targetPos, durationSec);
        }

        protected void SetTotalItemCount(int totalItemCnt)
        {
            int nextTotalItemCnt = Mathf.Max(0, totalItemCnt);
            if(nextTotalItemCnt == _totalItemCnt)
                return;

            _totalItemCnt = nextTotalItemCnt;
            UpdateContentLayout();
            if(_isInitialized)
            {
                RebuildVisibleItems();
                return;
            }

            _itemCtrl.UpdatePosition();
        }

        protected void SetVisibleLineCount(int visibleLineCnt)
        {
            int nextVisibleLineCnt = Mathf.Max(MIN_VISIBLE_LINE_CNT, visibleLineCnt);
            if(nextVisibleLineCnt == _visibleLineCnt)
                return;

            _visibleLineCnt = nextVisibleLineCnt;
            if(_isInitialized)
                RebuildVisibleItems();
        }

        protected void SetContentPosition(int itemIdx, float offset = 0.0f)
        {
            if(_totalItemCnt <= 0 || _context == null)
                return;

            int targetIdx = Mathf.Clamp(itemIdx, 0, _totalItemCnt - 1);
            Vector2 targetPos = _context.CalculateContentPosition(targetIdx, offset);
            _rtContent.anchoredPosition = _context.ClampContentPosition(targetPos, _totalLineCnt, _visibleLineCnt);
            OnScrollValueChanged(Vector2.zero);
        }

        private void OnScrollValueChanged(Vector2 value)
        {
            if(_totalItemCnt <= 0 || _itemCtrl.Count <= 0)
            {
                HandleScrollValueChanged(value);
                return;
            }

            float currentScrollPos = IsVertical ? _rtContent.anchoredPosition.y : -_rtContent.anchoredPosition.x;
            if(Mathf.Abs(currentScrollPos - _lastScrollPos) <= SCROLL_REFRESH_EPSILON)
            {
                HandleScrollValueChanged(value);
                return;
            }

            _lastScrollPos = currentScrollPos;
            int line = _itemCtrl.FirstIndex / _itemCntPerLine;
            int visibleLine = _context.CalculateFirstVisibleLine(Mathf.Max(0, _totalLineCnt - _visibleLineCnt));
            if(line != visibleLine)
            {
                bool isDown = visibleLine > line;
                int moveLineCnt = Mathf.Min(Mathf.Abs(line - visibleLine), _visibleLineCnt);
                for(int i = 0; i < moveLineCnt; i++)
                {
                    int addLine = isDown ? visibleLine + _visibleLineCnt - moveLineCnt + i : visibleLine + moveLineCnt - i - 1;
                    int removeLine = isDown ? line + i : line + _visibleLineCnt - i - 1;

                    _itemCtrl.AddRange(_context.GetItemCountForLine(addLine, _totalItemCnt), addLine * _itemCntPerLine, isDown);
                    _itemCtrl.RemoveRange(_context.GetItemCountForLine(removeLine, _totalItemCnt), !isDown);
                }
            }

            HandleScrollValueChanged(value);
        }

        private bool ValidateInspectorFields()
        {
            if(_padding == null)
            {
                DebugLogger.LogWarning("?®Îî© Ï∞∏Ï°∞Í∞Ä ÎπÑÏñ¥ ?àÏñ¥ Í∏∞Î≥∏Í∞íÏúºÎ°?Î≥¥Ï†ï?©Îãà??");
                _padding = new RectOffset();
            }

            if(_fixedCellsPerGroup < MIN_FIXED_CELLS_PER_GROUP)
            {
                DebugLogger.LogWarning($"Í≥†Ï†ï Í∑∏Î£π ?ÑÏù¥????{_fixedCellsPerGroup})Í∞Ä ?òÎ™ª?òÏñ¥ 1Î°?Î≥¥Ï†ï?©Îãà??");
                _fixedCellsPerGroup = MIN_FIXED_CELLS_PER_GROUP;
            }

            if(_scrollRect == null)
            {
                DebugLogger.LogError("ScrollRect Ï∞∏Ï°∞Î•?Ï∞æÏùÑ ???ÜÏäµ?àÎã§.");
                return false;
            }

            if(_rtContent == null)
            {
                DebugLogger.LogError("ScrollRect Content Ï∞∏Ï°∞Í∞Ä ÎπÑÏñ¥ ?àÏäµ?àÎã§.");
                return false;
            }

            if(_item == null)
            {
                DebugLogger.LogError("?ôÏ†Å ?§ÌÅ¨Î°??ÑÏù¥???ÑÎ¶¨??Ï∞∏Ï°∞Í∞Ä ÎπÑÏñ¥ ?àÏäµ?àÎã§.");
                return false;
            }

            _rtItem = _item.GetComponent<RectTransform>();
            if(_rtItem == null)
            {
                DebugLogger.LogError("?ôÏ†Å ?§ÌÅ¨Î°??ÑÏù¥?úÏóê RectTransform???ÜÏäµ?àÎã§.");
                return false;
            }

            return true;
        }

        private void ApplyScrollRectSettings()
        {
            _scrollRect.vertical = IsVertical;
            _scrollRect.horizontal = !IsVertical;
            _scrollRect.movementType = ConvertMovementType(_movementType);
            _scrollRect.inertia = _inertia;
            _scrollRect.decelerationRate = _decelerationRate;
            _scrollRect.elasticity = _elasticity;
        }

        private void ApplyContentAlignment()
        {
            if(IsVertical)
            {
                float crossAlignX = ResolveCrossAlignX(_contentAlignment);
                _rtContent.anchorMin = new Vector2(crossAlignX, 1.0f);
                _rtContent.anchorMax = new Vector2(crossAlignX, 1.0f);
                _rtContent.pivot = new Vector2(crossAlignX, 1.0f);
            }
            else
            {
                float crossAlignY = ResolveCrossAlignY(_contentAlignment);
                _rtContent.anchorMin = new Vector2(0.0f, crossAlignY);
                _rtContent.anchorMax = new Vector2(0.0f, crossAlignY);
                _rtContent.pivot = new Vector2(0.0f, crossAlignY);
            }

            _rtContent.anchoredPosition = Vector2.zero;
        }

        private void UpdateLayoutConfig()
        {
            int nextItemCntPerLine = ResolveItemCountPerLine();
            if(nextItemCntPerLine != _itemCntPerLine)
            {
                _itemCntPerLine = nextItemCntPerLine;
                _context?.SetItemCountPerLine(_itemCntPerLine);
            }

            UpdateAutoVisibleLineCount();
        }

        private void UpdateAutoVisibleLineCount()
        {
            if(_context == null)
                return;

            _visibleLineCnt = Mathf.Max(MIN_VISIBLE_LINE_CNT, _context.CalculateAutoVisibleLineCount(DEFAULT_EXTRA_VISIBLE_LINE_CNT));
        }

        private int ResolveItemCountPerLine()
        {
            switch(_layoutMode)
            {
                case EDynamicScrollLayoutMode.Single:
                    return MIN_FIXED_CELLS_PER_GROUP;
                case EDynamicScrollLayoutMode.FixedCount:
                    return Mathf.Max(MIN_FIXED_CELLS_PER_GROUP, _fixedCellsPerGroup);
                case EDynamicScrollLayoutMode.AutoFit:
                    return CalculateAutoItemCountPerLine();
            }

            return MIN_FIXED_CELLS_PER_GROUP;
        }

        private int CalculateAutoItemCountPerLine()
        {
            RectTransform rtViewport = _scrollRect.viewport != null ? _scrollRect.viewport : _scrollRect.GetComponent<RectTransform>();
            if(rtViewport == null)
                return MIN_FIXED_CELLS_PER_GROUP;

            float viewportCrossSize = IsVertical ? rtViewport.rect.width : rtViewport.rect.height;
            float crossPaddingSize = IsVertical ? _padding.left + _padding.right : _padding.top + _padding.bottom;
            float crossSpacingSize = IsVertical ? _spacing.x : _spacing.y;
            float cellCrossSize = IsVertical ? _rtItem.sizeDelta.x : _rtItem.sizeDelta.y;

            float availableCrossSize = Mathf.Max(0.0f, viewportCrossSize - crossPaddingSize);
            float cellSpan = cellCrossSize + crossSpacingSize;
            if(cellSpan <= 0.0f)
                return MIN_FIXED_CELLS_PER_GROUP;

            int fitCnt = Mathf.FloorToInt((availableCrossSize + crossSpacingSize) / cellSpan);
            return Mathf.Max(MIN_FIXED_CELLS_PER_GROUP, fitCnt);
        }

        private void UpdateContentLayout()
        {
            _totalLineCnt = _itemCntPerLine > 0 ? Mathf.CeilToInt((float)_totalItemCnt / _itemCntPerLine) : 0;
            _rtContent.sizeDelta = _context.CalculateContentSize(_totalLineCnt);
        }

        private void RebuildVisibleItems()
        {
            if(_itemCtrl == null)
                return;

            _itemCtrl.Clear();
            if(_totalItemCnt <= 0)
                return;

            _rtContent.anchoredPosition = _context.ClampContentPosition(_rtContent.anchoredPosition, _totalLineCnt, _visibleLineCnt);
            int firstLine = _context.CalculateFirstVisibleLine(Mathf.Max(0, _totalLineCnt - _visibleLineCnt));
            int firstIdx = firstLine * _itemCntPerLine;
            int visibleItemCnt = Mathf.Min(_visibleLineCnt * _itemCntPerLine, _totalItemCnt - firstIdx);
            if(visibleItemCnt <= 0)
                return;

            _itemCtrl.AddRange(visibleItemCnt, firstIdx, true);
        }

        private void StartSmoothScroll(Vector2 targetPos, float durationSec)
        {
            StopSmoothScroll();
            _coSmoothScroll = StartCoroutine(CoSmoothScroll(targetPos, durationSec));
        }

        private void StopSmoothScroll()
        {
            if(_coSmoothScroll == null)
                return;

            StopCoroutine(_coSmoothScroll);
            _coSmoothScroll = null;
        }

        //============================================================
        //Coroutines
        //============================================================
        private IEnumerator CoSmoothScroll(Vector2 targetPos, float durationSec)
        {
            Vector2 startPos = _rtContent.anchoredPosition;
            float elapsedSec = 0.0f;
            while(elapsedSec < durationSec)
            {
                elapsedSec += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedSec / durationSec);
                float smoothT = t * t * (3.0f - 2.0f * t);
                _rtContent.anchoredPosition = Vector2.Lerp(startPos, targetPos, smoothT);
                OnScrollValueChanged(Vector2.zero);
                yield return null;
            }

            _rtContent.anchoredPosition = targetPos;
            OnScrollValueChanged(Vector2.zero);
            _coSmoothScroll = null;
        }

        //============================================================
        //Utilities
        //============================================================
        private static ScrollRect.MovementType ConvertMovementType(EDynamicScrollMovementType movementType)
        {
            switch(movementType)
            {
                case EDynamicScrollMovementType.Unrestricted:
                    return ScrollRect.MovementType.Unrestricted;
                case EDynamicScrollMovementType.Elastic:
                    return ScrollRect.MovementType.Elastic;
                case EDynamicScrollMovementType.Clamped:
                    return ScrollRect.MovementType.Clamped;
            }

            return ScrollRect.MovementType.Clamped;
        }

        private static float ResolveCrossAlignX(EDynamicScrollContentAlignment alignment)
        {
            switch(alignment)
            {
                case EDynamicScrollContentAlignment.TopLeft:
                case EDynamicScrollContentAlignment.MiddleLeft:
                case EDynamicScrollContentAlignment.BottomLeft:
                    return 0.0f;
                case EDynamicScrollContentAlignment.TopCenter:
                case EDynamicScrollContentAlignment.MiddleCenter:
                case EDynamicScrollContentAlignment.BottomCenter:
                    return 0.5f;
                case EDynamicScrollContentAlignment.TopRight:
                case EDynamicScrollContentAlignment.MiddleRight:
                case EDynamicScrollContentAlignment.BottomRight:
                    return 1.0f;
            }

            return 0.0f;
        }

        private static float ResolveCrossAlignY(EDynamicScrollContentAlignment alignment)
        {
            switch(alignment)
            {
                case EDynamicScrollContentAlignment.TopLeft:
                case EDynamicScrollContentAlignment.TopCenter:
                case EDynamicScrollContentAlignment.TopRight:
                    return 1.0f;
                case EDynamicScrollContentAlignment.MiddleLeft:
                case EDynamicScrollContentAlignment.MiddleCenter:
                case EDynamicScrollContentAlignment.MiddleRight:
                    return 0.5f;
                case EDynamicScrollContentAlignment.BottomLeft:
                case EDynamicScrollContentAlignment.BottomCenter:
                case EDynamicScrollContentAlignment.BottomRight:
                    return 0.0f;
            }

            return 1.0f;
        }

        //============================================================
        //Callbacks
        //============================================================
        protected virtual void HandleScrollValueChanged(Vector2 value) { }
    }
}
