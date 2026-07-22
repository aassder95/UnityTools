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
        private const int MIN_VISIBLE_LINE_COUNT = 1;
        private const int MIN_FIXED_CELLS_PER_GROUP = 1;
        private const int DEFAULT_EXTRA_VISIBLE_LINE_COUNT = 1;
        private const float SCROLL_REFRESH_EPSILON = 0.01f;
        private const float MIN_SCROLL_DURATION_SEC = 0.0001f;

        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private TView _item;
        [SerializeField] private EDynamicScrollAxisType _axisType = EDynamicScrollAxisType.Vertical;
        [SerializeField] private EDynamicScrollLayoutMode _layoutMode = EDynamicScrollLayoutMode.FixedCount;
        [SerializeField] private int _fixedCellsPerGroup = 1;
        [SerializeField] private int _minVisibleLineCount = MIN_VISIBLE_LINE_COUNT;
        [SerializeField] private EDynamicScrollMovementType _movementType = EDynamicScrollMovementType.Clamped;
        [SerializeField] private bool _isInertia = true;
        [SerializeField] [Range(0.001f, 0.3f)] private float _decelerationRate = 0.135f;
        [SerializeField] [Range(0.1f, 1.0f)] private float _elasticity = 0.1f;
        [SerializeField] private EDynamicScrollContentAlignment _contentAlignment = EDynamicScrollContentAlignment.TopLeft;
        [SerializeField] private Vector2 _spacing;
        [SerializeField] private RectOffset _padding;

        //============================================================
        // Fields
        //============================================================
        private DynamicScrollContext _context;
        private DynamicScrollItemController<TView> _itemCtrl;
        private ObjectPool<TView> _pool;
        private int _totalItemCount;
        private int _totalLineCount;
        private int _visibleLineCount = MIN_VISIBLE_LINE_COUNT;
        private int _itemCountPerLine = MIN_FIXED_CELLS_PER_GROUP;
        private RectTransform _rt;
        private RectTransform _rtContent;
        private RectTransform _rtItem;
        private ScrollRect _scrollRect;
        private UnityAction<TView> _onControllerItemUpdated;
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
        protected int TotalLineCount => _totalLineCount;
        protected int VisibleLineCount => _visibleLineCount;
        protected int TotalItemCount => _totalItemCount;
        protected int ItemCountPerLine => _itemCountPerLine;
        protected RectTransform RectTransform => _rt;
        protected RectTransform RtContent => _rtContent;
        protected ScrollRect ScrollRect => _scrollRect;
        protected bool IsVertical => _axisType == EDynamicScrollAxisType.Vertical;
        public override bool IsInitialized => _isInitialized;

        //============================================================
        // Unity Methods
        //============================================================
        private void Awake()
        {
            EnsureComponentReady();
        }

        private void OnRectTransformDimensionsChange()
        {
            if(!_isInitialized || !_isComponentReady || _context == null)
                return;

            int prevItemCountPerLine = _itemCountPerLine;
            int prevVisibleLineCount = _visibleLineCount;

            UpdateLayoutConfig();
            if(prevItemCountPerLine == _itemCountPerLine && prevVisibleLineCount == _visibleLineCount)
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
        // Init/Register
        //============================================================
        private bool EnsureComponentReady()
        {
            if(_isComponentReady)
                return true;

            _rt = GetComponent<RectTransform>();
            _scrollRect = GetComponent<ScrollRect>();
            _rtContent = _scrollRect != null ? _scrollRect.content : null;
            if(!ValidateInspectorFields())
            {
                enabled = false;
                return false;
            }

            ApplyScrollRectSettings();
            ApplyContentAlignment();

            _itemCountPerLine = ResolveItemCountPerLine();
            _context = new DynamicScrollContext(_itemCountPerLine, _spacing, _padding, _rtItem, _scrollRect);
            UpdateAutoVisibleLineCount();

            int initialItemCount = Mathf.Max(_visibleLineCount * _itemCountPerLine, _itemCountPerLine);
            _pool = new ObjectPool<TView>(initialItemCount, _item, _rtContent);
            _itemCtrl = new DynamicScrollItemController<TView>(_context, _pool);

            if(_onControllerItemUpdated == null)
                _onControllerItemUpdated = itemView => _onItemUpdated?.Invoke(itemView);

            _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            _itemCtrl.OnItemUpdated += _onControllerItemUpdated;
            _isComponentReady = true;
            return true;
        }

        //============================================================
        // Logic
        //============================================================
        public void InitView(int totalItemCount)
        {
            if(!EnsureComponentReady())
                return;

            _isInitialized = true;
            UpdateLayoutConfig();

            int nextTotalItemCount = Mathf.Max(0, totalItemCount);
            bool isSameTotalItemCount = nextTotalItemCount == _totalItemCount;
            SetTotalItemCount(nextTotalItemCount);
            if(isSameTotalItemCount)
                RebuildVisibleItems();
        }

        public void RefreshView()
        {
            if(!_isInitialized || !_isComponentReady)
                return;

            RebuildVisibleItems();
        }

        public void ScrollTo(int itemIdx, bool isImmediate = false, float durationSec = 0.3f)
        {
            if(!EnsureComponentReady())
                return;

            if(_context == null || _rtContent == null || _totalItemCount <= 0)
                return;

            int targetIdx = Mathf.Clamp(itemIdx, 0, _totalItemCount - 1);
            Vector2 targetPos = _context.CalculateContentPosition(targetIdx);
            targetPos = _context.ClampContentPosition(targetPos, _totalLineCount, _visibleLineCount);

            if(isImmediate || durationSec <= MIN_SCROLL_DURATION_SEC)
            {
                StopSmoothScroll();
                _rtContent.anchoredPosition = targetPos;
                OnScrollValueChanged(Vector2.zero);
                return;
            }

            StartSmoothScroll(targetPos, durationSec);
        }

        protected void SetTotalItemCount(int totalItemCount)
        {
            if(!EnsureComponentReady())
                return;

            int nextTotalItemCount = Mathf.Max(0, totalItemCount);
            if(nextTotalItemCount == _totalItemCount)
                return;

            _totalItemCount = nextTotalItemCount;
            UpdateContentLayout();

            if(_isInitialized)
            {
                RebuildVisibleItems();
                return;
            }

            _itemCtrl?.UpdatePosition();
        }

        protected void SetVisibleLineCount(int visibleLineCount)
        {
            if(!EnsureComponentReady())
                return;

            int nextVisibleLineCount = Mathf.Max(MIN_VISIBLE_LINE_COUNT, visibleLineCount);
            if(nextVisibleLineCount == _visibleLineCount)
                return;

            _visibleLineCount = nextVisibleLineCount;
            if(_isInitialized)
                RebuildVisibleItems();
        }

        protected void SetContentPosition(int itemIdx, float offset = 0.0f)
        {
            if(!EnsureComponentReady())
                return;

            if(_totalItemCount <= 0 || _context == null || _rtContent == null)
                return;

            int targetIdx = Mathf.Clamp(itemIdx, 0, _totalItemCount - 1);
            Vector2 targetPos = _context.CalculateContentPosition(targetIdx, offset);
            _rtContent.anchoredPosition = _context.ClampContentPosition(targetPos, _totalLineCount, _visibleLineCount);
            OnScrollValueChanged(Vector2.zero);
        }

        private void OnScrollValueChanged(Vector2 value)
        {
            if(!_isComponentReady || _rtContent == null || _context == null)
            {
                HandleScrollValueChanged(value);
                return;
            }

            if(_totalItemCount <= 0 || _itemCtrl.Count <= 0)
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
            int currentLine = _itemCtrl.FirstIndex / _itemCountPerLine;
            int visibleLine = _context.CalculateFirstVisibleLine(Mathf.Max(0, _totalLineCount - _visibleLineCount));
            if(currentLine != visibleLine)
            {
                bool isDown = visibleLine > currentLine;
                int moveLineCount = Mathf.Min(Mathf.Abs(currentLine - visibleLine), _visibleLineCount);
                for(int i = 0; i < moveLineCount; i++)
                {
                    int addLine = isDown ? visibleLine + _visibleLineCount - moveLineCount + i : visibleLine + moveLineCount - i - 1;
                    int removeLine = isDown ? currentLine + i : currentLine + _visibleLineCount - i - 1;

                    _itemCtrl.AddRange(_context.GetItemCountForLine(addLine, _totalItemCount), addLine * _itemCountPerLine, isDown);
                    _itemCtrl.RemoveRange(_context.GetItemCountForLine(removeLine, _totalItemCount), !isDown);
                }
            }

            HandleScrollValueChanged(value);
        }

        private bool ValidateInspectorFields()
        {
            if(_padding == null)
            {
                DebugLogger.LogWarning("Padding 값이 비어 있어 기본값으로 보정합니다.");
                _padding = new RectOffset();
            }

            if(_fixedCellsPerGroup < MIN_FIXED_CELLS_PER_GROUP)
            {
                DebugLogger.LogWarning($"고정 라인 아이템 수({_fixedCellsPerGroup})가 잘못되어 1로 보정합니다.");
                _fixedCellsPerGroup = MIN_FIXED_CELLS_PER_GROUP;
            }

            if(_minVisibleLineCount < MIN_VISIBLE_LINE_COUNT)
            {
                DebugLogger.LogWarning($"최소 표시 라인 수({_minVisibleLineCount})가 잘못되어 1로 보정합니다.");
                _minVisibleLineCount = MIN_VISIBLE_LINE_COUNT;
            }

            if(_scrollRect == null)
            {
                DebugLogger.LogError("ScrollRect 참조를 찾을 수 없습니다.");
                return false;
            }

            if(_rtContent == null)
            {
                DebugLogger.LogError("ScrollRect Content 참조가 비어 있습니다.");
                return false;
            }

            if(_item == null)
            {
                DebugLogger.LogError("동적 스크롤 아이템 프리팹 참조가 비어 있습니다.");
                return false;
            }

            _rtItem = _item.GetComponent<RectTransform>();
            if(_rtItem == null)
            {
                DebugLogger.LogError("동적 스크롤 아이템에 RectTransform이 없습니다.");
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

            int nextItemCountPerLine = ResolveItemCountPerLine();
            if(nextItemCountPerLine != _itemCountPerLine)
            {
                _itemCountPerLine = nextItemCountPerLine;
                _context?.SetItemCountPerLine(_itemCountPerLine);
            }

            UpdateAutoVisibleLineCount();
        }

        private void UpdateAutoVisibleLineCount()
        {
            if(_context == null)
                return;

            int autoVisibleLineCount = _context.CalculateAutoVisibleLineCount(DEFAULT_EXTRA_VISIBLE_LINE_COUNT);
            _visibleLineCount = Mathf.Max(_minVisibleLineCount, autoVisibleLineCount);
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

            int fitCount = Mathf.FloorToInt((availableCrossSize + crossSpacingSize) / cellSpan);
            return Mathf.Max(MIN_FIXED_CELLS_PER_GROUP, fitCount);
        }

        private void UpdateContentLayout()
        {
            if(_context == null || _rtContent == null)
                return;

            _totalLineCount = _itemCountPerLine > 0 ? Mathf.CeilToInt((float)_totalItemCount / _itemCountPerLine) : 0;
            _rtContent.sizeDelta = _context.CalculateContentSize(_totalLineCount);
        }

        private void RebuildVisibleItems()
        {
            if(!_isComponentReady || _itemCtrl == null || _context == null || _rtContent == null)
                return;

            _lastScrollPos = float.MinValue;
            _itemCtrl.Clear();
            if(_totalItemCount <= 0)
                return;

            _rtContent.anchoredPosition = _context.ClampContentPosition(_rtContent.anchoredPosition, _totalLineCount, _visibleLineCount);
            int firstLine = _context.CalculateFirstVisibleLine(Mathf.Max(0, _totalLineCount - _visibleLineCount));
            int firstIdx = firstLine * _itemCountPerLine;
            if(firstIdx >= _totalItemCount)
                return;

            int visibleItemCount = Mathf.Min(_visibleLineCount * _itemCountPerLine, _totalItemCount - firstIdx);
            if(visibleItemCount <= 0)
                return;

            _itemCtrl.AddRange(visibleItemCount, firstIdx, true);
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
        // Utilities
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
        // Callbacks
        //============================================================
        protected virtual void HandleScrollValueChanged(Vector2 value) { }
    }
}
