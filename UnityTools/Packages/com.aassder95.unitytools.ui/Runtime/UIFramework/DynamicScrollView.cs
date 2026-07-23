using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityTools.Util.Core.Pooling;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Util.UIFramework
{
    [RequireComponent(typeof(ScrollRect))]
    public class DynamicScrollView<TView> : DynamicScrollViewBase where TView : Component, IDynamicScrollItem, IPoolable
    {
        //============================================================
        // Constants
        //============================================================
        private const int MIN_VISIBLE_LINE_CNT = 1;
        private const int MIN_FIXED_CELLS_PER_GROUP = 1;
        private const int DEFAULT_EXTRA_VISIBLE_LINE_CNT = 1;
        private const float SCROLL_REFRESH_EPSILON = 0.01f;
        private const float MIN_SCROLL_DURATION_SEC = 0.0001f;

        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Item")] [SerializeField] private TView _item;
        [Header("Layout")] [SerializeField] private EDynamicScrollAxisType _axisType = EDynamicScrollAxisType.Vertical;
        [SerializeField] private EDynamicScrollLayoutMode _layoutMode = EDynamicScrollLayoutMode.FixedCnt;
        [SerializeField] private int _fixedCellsPerGroup = 1;
        [SerializeField] private int _minVisibleLineCnt = MIN_VISIBLE_LINE_CNT;
        [SerializeField] private EDynamicScrollContentAlignment _contentAlignment = EDynamicScrollContentAlignment.TopLeft;
        [SerializeField] private Vector2 _spacing;
        [SerializeField] private RectOffset _padding;
        [Header("Movement")] [SerializeField] private EDynamicScrollMovementType _movementType = EDynamicScrollMovementType.Clamped;
        [SerializeField] private bool _isInertia = true;
        [SerializeField] [Range(0.001f, 0.3f)] private float _decelerationRate = 0.135f;
        [SerializeField] [Range(0.1f, 1.0f)] private float _elasticity = 0.1f;

        //============================================================
        // Fields
        //============================================================
        private DynamicScrollContext _context;
        private DynamicScrollItemController<TView> _itemCtrl;
        private ObjectPool<TView> _pool;
        private int _totalItemCnt;
        private int _totalLineCnt;
        private int _visibleLineCnt = MIN_VISIBLE_LINE_CNT;
        private int _itemCntPerLine = MIN_FIXED_CELLS_PER_GROUP;
        private RectTransform _rtView;
        private RectTransform _rtContent;
        private RectTransform _rtItem;
        private ScrollRect _scrollRect;
        private bool _isComponentReady;
        private bool _isInitialized;
        private float _lastScrollPos = float.MinValue;
        private Coroutine _coSmoothScroll;

        //============================================================
        // Events
        //============================================================
        public event UnityAction<TView> OnItemUpdated { add => _onItemUpdated.AddListener(value); remove => _onItemUpdated.RemoveListener(value); }
        private readonly UnityEvent<TView> _onItemUpdated = new();

        //============================================================
        // Properties
        //============================================================
        protected DynamicScrollContext Context => _context;
        protected DynamicScrollItemController<TView> ItemController => _itemCtrl;
        protected int TotalLineCnt => _totalLineCnt;
        protected int VisibleLineCnt => _visibleLineCnt;
        protected int TotalItemCnt => _totalItemCnt;
        protected int ItemCntPerLine => _itemCntPerLine;
        protected RectTransform RtView => _rtView;
        protected RectTransform RtContent => _rtContent;
        protected ScrollRect ScrollRect => _scrollRect;
        protected bool IsVertical => _axisType == EDynamicScrollAxisType.Vertical;
        public override bool IsInitialized => _isInitialized;

        //============================================================
        // Unity Methods
        //============================================================
        private void Awake()
        {
            if(!PrepareComponents())
                enabled = false;
        }

        private void OnRectTransformDimensionsChange()
        {
            if(!_isInitialized)
                return;

            int prevItemCntPerLine = _itemCntPerLine;
            int prevVisibleLineCnt = _visibleLineCnt;
            UpdateLayoutConfig();
            if(prevItemCntPerLine == _itemCntPerLine && prevVisibleLineCnt == _visibleLineCnt)
                return;

            UpdateContentLayout();
            if(!TryRebuildVisibleItems())
                enabled = false;
        }

        private void OnDestroy()
        {
            if(!TryReleaseView())
                DebugLogger.LogError("DynamicScrollView 파괴 중 Item 정리를 완료하지 못했습니다.", this);
        }

        //============================================================
        // Init/Register
        //============================================================
        private bool PrepareComponents()
        {
            if(_isComponentReady)
                return true;

            _rtView = GetComponent<RectTransform>();
            _scrollRect = GetComponent<ScrollRect>();
            _rtContent = _scrollRect.content;
            _rtItem = _item.transform as RectTransform;
            if(!ValidateConfig())
            {
                enabled = false;
                return false;
            }

            ApplyScrollRectSettings();
            ApplyContentAlignment();
            _itemCntPerLine = GetItemCntPerLine();
            _context = new DynamicScrollContext(_itemCntPerLine, _spacing, _padding, _rtItem, _scrollRect);
            UpdateAutoVisibleLineCnt();

            int initialItemCnt = Mathf.Max(_visibleLineCnt * _itemCntPerLine, _itemCntPerLine);
            _pool = ObjectPool<TView>.Create(initialItemCnt, _item, _rtContent);
            _itemCtrl = new DynamicScrollItemController<TView>(_context, _pool);

            _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            _itemCtrl.OnItemUpdated += OnControllerItemUpdated;
            _isComponentReady = true;
            return true;
        }

        public bool TryInitView(int totalItemCnt)
        {
            if(totalItemCnt < 0)
            {
                DebugLogger.LogError("동적 스크롤 전체 Item 수는 0 이상이어야 합니다. 값=" + totalItemCnt, this);
                return false;
            }

            if(!PrepareComponents())
                return false;

            _isInitialized = true;
            UpdateLayoutConfig();
            bool isSameTotalItemCnt = totalItemCnt == _totalItemCnt;
            if(!TrySetTotalItemCnt(totalItemCnt))
            {
                _isInitialized = false;
                return false;
            }

            if(isSameTotalItemCnt && !TryRebuildVisibleItems())
            {
                _isInitialized = false;
                return false;
            }

            return true;
        }

        public bool TryReleaseView()
        {
            StopSmoothScroll();
            if(_scrollRect != null)
                _scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
            if(_itemCtrl != null)
                _itemCtrl.OnItemUpdated -= OnControllerItemUpdated;

            bool isSuccess = _itemCtrl == null || _itemCtrl.TryClear();
            _pool?.Clear();
            _context = null;
            _itemCtrl = null;
            _pool = null;
            _isComponentReady = false;
            _isInitialized = false;
            return isSuccess;
        }

        //============================================================
        // Logic
        //============================================================
        public bool TryRefreshView()
        {
            if(!_isInitialized)
                return false;

            return TryRebuildVisibleItems();
        }

        public bool TryScrollTo(int itemIdx, bool isImmediate = false, float durationSec = 0.3f)
        {
            if(!PrepareComponents())
                return false;
            if(_totalItemCnt <= 0)
                return false;
            if(!isImmediate && durationSec <= 0.0f)
            {
                DebugLogger.LogError("동적 스크롤 이동 시간은 0초보다 커야 합니다. 값=" + durationSec, this);
                return false;
            }

            int targetIdx = Mathf.Clamp(itemIdx, 0, _totalItemCnt - 1);
            Vector2 targetPos = _context.GetContentPos(targetIdx);
            targetPos = _context.ClampContentPos(targetPos, _totalLineCnt, _visibleLineCnt);
            if(isImmediate || durationSec <= MIN_SCROLL_DURATION_SEC)
            {
                StopSmoothScroll();
                _rtContent.anchoredPosition = targetPos;
                return TryProcessScroll(Vector2.zero);
            }

            StartSmoothScroll(targetPos, durationSec);
            return true;
        }

        protected bool TrySetTotalItemCnt(int totalItemCnt)
        {
            if(totalItemCnt < 0)
            {
                DebugLogger.LogError("동적 스크롤 전체 Item 수는 0 이상이어야 합니다. 값=" + totalItemCnt, this);
                return false;
            }

            if(!PrepareComponents())
                return false;
            if(totalItemCnt == _totalItemCnt)
                return true;

            _totalItemCnt = totalItemCnt;
            UpdateContentLayout();
            if(_isInitialized)
                return TryRebuildVisibleItems();

            _itemCtrl.UpdatePos();
            return true;
        }

        protected bool TrySetVisibleLineCnt(int visibleLineCnt)
        {
            if(visibleLineCnt < MIN_VISIBLE_LINE_CNT)
            {
                DebugLogger.LogError("동적 스크롤 표시 Line 수는 1 이상이어야 합니다. 값=" + visibleLineCnt, this);
                return false;
            }

            if(!PrepareComponents())
                return false;
            if(visibleLineCnt == _visibleLineCnt)
                return true;

            _visibleLineCnt = visibleLineCnt;
            return !_isInitialized || TryRebuildVisibleItems();
        }


        private bool TryProcessScroll(Vector2 value)
        {
            if(!_isComponentReady || _totalItemCnt <= 0 || _itemCtrl.Cnt <= 0)
                return TryNotifyScrollChanged(value);

            float curScrollPos = IsVertical ? _rtContent.anchoredPosition.y : -_rtContent.anchoredPosition.x;
            if(Mathf.Abs(curScrollPos - _lastScrollPos) <= SCROLL_REFRESH_EPSILON)
                return TryNotifyScrollChanged(value);

            _lastScrollPos = curScrollPos;
            int curLine = _itemCtrl.FirstIdx / _itemCntPerLine;
            int visibleLine = _context.GetFirstVisibleLine(Mathf.Max(0, _totalLineCnt - _visibleLineCnt));
            if(curLine != visibleLine)
            {
                bool isDown = visibleLine > curLine;
                int moveLineCnt = Mathf.Min(Mathf.Abs(curLine - visibleLine), _visibleLineCnt);
                for(int i = 0; i < moveLineCnt; i++)
                {
                    int addLine = isDown ? visibleLine + _visibleLineCnt - moveLineCnt + i : visibleLine + moveLineCnt - i - 1;
                    int removeLine = isDown ? curLine + i : curLine + _visibleLineCnt - i - 1;
                    if(!_itemCtrl.TryAddRange(_context.GetItemCntForLine(addLine, _totalItemCnt), addLine * _itemCntPerLine, isDown))
                        return false;
                    if(!_itemCtrl.TryRemoveRange(_context.GetItemCntForLine(removeLine, _totalItemCnt), !isDown))
                        return false;
                }
            }

            return TryNotifyScrollChanged(value);
        }
        private bool ValidateConfig()
        {
            if(_fixedCellsPerGroup < MIN_FIXED_CELLS_PER_GROUP)
            {
                DebugLogger.LogError("DynamicScrollView의 고정 Line Item 수는 1 이상이어야 합니다. 값=" + _fixedCellsPerGroup, this);
                return false;
            }

            if(_minVisibleLineCnt < MIN_VISIBLE_LINE_CNT)
            {
                DebugLogger.LogError("DynamicScrollView의 최소 표시 Line 수는 1 이상이어야 합니다. 값=" + _minVisibleLineCnt, this);
                return false;
            }

            return true;
        }
        private void ApplyScrollRectSettings()
        {
            _scrollRect.vertical = IsVertical;
            _scrollRect.horizontal = !IsVertical;
            _scrollRect.movementType = ConvertMovementType(_movementType);
            _scrollRect.inertia = _isInertia;
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
            if(!_isComponentReady)
                return;

            int nextItemCntPerLine = GetItemCntPerLine();
            if(nextItemCntPerLine != _itemCntPerLine)
            {
                _itemCntPerLine = nextItemCntPerLine;
                _context.SetItemCntPerLine(_itemCntPerLine);
            }

            UpdateAutoVisibleLineCnt();
        }

        private void UpdateAutoVisibleLineCnt()
        {
            int autoVisibleLineCnt = _context.GetAutoVisibleLineCnt(DEFAULT_EXTRA_VISIBLE_LINE_CNT);
            _visibleLineCnt = Mathf.Max(_minVisibleLineCnt, autoVisibleLineCnt);
        }

        private int GetItemCntPerLine()
        {
            switch(_layoutMode)
            {
                case EDynamicScrollLayoutMode.Single:
                    return MIN_FIXED_CELLS_PER_GROUP;
                case EDynamicScrollLayoutMode.FixedCnt:
                    return Mathf.Max(MIN_FIXED_CELLS_PER_GROUP, _fixedCellsPerGroup);
                case EDynamicScrollLayoutMode.AutoFit:
                    return GetAutoItemCntPerLine();
            }

            return MIN_FIXED_CELLS_PER_GROUP;
        }

        private int GetAutoItemCntPerLine()
        {
            RectTransform rtViewport = _scrollRect.viewport != null ? _scrollRect.viewport : _rtView;

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
            _rtContent.sizeDelta = _context.GetContentSize(_totalLineCnt);
        }

        private bool TryRebuildVisibleItems()
        {
            if(!_isComponentReady)
                return false;

            _lastScrollPos = float.MinValue;
            if(!_itemCtrl.TryClear())
                return false;
            if(_totalItemCnt <= 0)
                return true;

            _rtContent.anchoredPosition = _context.ClampContentPos(_rtContent.anchoredPosition, _totalLineCnt, _visibleLineCnt);
            int firstLine = _context.GetFirstVisibleLine(Mathf.Max(0, _totalLineCnt - _visibleLineCnt));
            int firstIdx = firstLine * _itemCntPerLine;
            if(firstIdx >= _totalItemCnt)
                return true;

            int visibleItemCnt = Mathf.Min(_visibleLineCnt * _itemCntPerLine, _totalItemCnt - firstIdx);
            return visibleItemCnt <= 0 || _itemCtrl.TryAddRange(visibleItemCnt, firstIdx, true);
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
        // Coroutines
        //============================================================
        private IEnumerator CoSmoothScroll(Vector2 targetPos, float durationSec)
        {
            Vector2 startPos = _rtContent.anchoredPosition;
            float elapsedSec = 0.0f;

            while(elapsedSec < durationSec)
            {
                elapsedSec += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedSec / durationSec);
                float smoothT = t * t * (3.0f - (2.0f * t));
                _rtContent.anchoredPosition = Vector2.Lerp(startPos, targetPos, smoothT);
                OnScrollValueChanged(Vector2.zero);
                yield return null;
            }

            _rtContent.anchoredPosition = targetPos;
            OnScrollValueChanged(Vector2.zero);
            _coSmoothScroll = null;
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnScrollValueChanged(Vector2 value)
        {
            if(!TryProcessScroll(value))
                enabled = false;
        }

        private void OnControllerItemUpdated(TView itemView)
        {
            _onItemUpdated.Invoke(itemView);
        }

        protected virtual void HandleScrollValueChanged(Vector2 value) { }
        //============================================================
        // Utilities
        //============================================================
        private bool TryNotifyScrollChanged(Vector2 value)
        {
            try
            {
                HandleScrollValueChanged(value);
                return true;
            }
            catch(Exception exception)
            {
                DebugLogger.LogError("DynamicScrollView 값 변경 Callback 실행에 실패했습니다. 원인=" + exception.Message, this);
                return false;
            }
        }
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

    }
}
