using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Vfx.Editor
{
    public class VfxBrowserWindow : EditorWindow
    {
        //============================================================
        // Constants
        //============================================================
        private const int PAGE_SIZE = 12;

        //============================================================
        // Readonly
        //============================================================
        private readonly VfxPrefabCatalog _catalog = new VfxPrefabCatalog();
        private readonly List<VfxPrefabInfo> _results = new List<VfxPrefabInfo>();
        private readonly HashSet<string> _favoriteGuids = new HashSet<string>();
        private readonly VfxThumbnailIndex _thumbnails = new VfxThumbnailIndex();
        private readonly HashSet<string> _failedGuids = new HashSet<string>();

        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Library")]
        [SerializeField] private VfxLibrary _library;
        [SerializeField] private string _tag = string.Empty;
        [SerializeField] private string _collection = string.Empty;
        [Header("Catalog")]
        [SerializeField] private string _rootPath = "Assets";
        [SerializeField] private string _query = string.Empty;
        [SerializeField] private string _selectedGuid = string.Empty;
        [SerializeField] private EVfxLoopFilter _loopFilter;
        [SerializeField] private bool _isFavoritesOnly;
        [SerializeField] private float _durationSec = 5.0f;
        [SerializeField] private float _speed = 1.0f;
        [Header("Thumbnails")]
        [SerializeField] private EVfxColor _colorFilter;
        [SerializeField] private float _thumbnailTimeSec = 1.0f;

        //============================================================
        // Fields
        //============================================================
        private VfxPreviewSession _preview;
        private DefaultAsset _folder;
        private Vector2 _scrollPos;
        private string _favoritesKey;
        private string _tagsText = string.Empty;
        private string _collectionsText = string.Empty;
        private bool _isPlaying = true;
        private bool _isIndexDirty;
        private double _lastTickSec;
        private float _pendingSec;
        private int _pageIdx;
        private int _scanIdx;
        private bool _isIndexingColors;
        private double _nextThumbnailSec;
        private string _thumbnailError = string.Empty;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnEnable()
        {
            _favoritesKey = "UnityTools.Vfx.Favorites." + Hash128.Compute(Application.dataPath);
            _favoriteGuids.Clear();
            string[] guids = EditorPrefs.GetString(_favoritesKey, string.Empty).Split(';');
            for (int idx = 0; idx < guids.Length; idx++)
            {
                if (guids[idx].Length == 32)
                    _favoriteGuids.Add(guids[idx]);
            }

            _folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(_rootPath);
            RefreshCatalog();
            _lastTickSec = EditorApplication.timeSinceStartup;
            EditorApplication.update += TickPreview;
            EditorApplication.projectChanged += HandleProjectChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += ReleasePreview;
            AssemblyReloadEvents.beforeAssemblyReload += ClearThumbnails;
            EditorApplication.update += TickThumbnails;
        }

        private void OnDisable()
        {
            EditorApplication.update -= TickPreview;
            EditorApplication.projectChanged -= HandleProjectChanged;
            EditorApplication.playModeStateChanged -= HandlePlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= ReleasePreview;
            AssemblyReloadEvents.beforeAssemblyReload -= ClearThumbnails;
            EditorApplication.update -= TickThumbnails;
            ReleasePreview();
            ClearThumbnails();
        }

        private void OnGUI()
        {
            DrawLibrary();
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginChangeCheck();
                DefaultAsset folder = (DefaultAsset)EditorGUILayout.ObjectField(_folder, typeof(DefaultAsset), false, GUILayout.Width(250.0f));
                if (EditorGUI.EndChangeCheck())
                {
                    string path = folder == null ? "Assets" : AssetDatabase.GetAssetPath(folder);
                    if (AssetDatabase.IsValidFolder(path))
                    {
                        _rootPath = path;
                        _folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(_rootPath);
                        RefreshCatalog();
                    }
                }

                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70.0f)))
                    RefreshCatalog();

                EditorGUI.BeginChangeCheck();
                _query = GUILayout.TextField(_query, EditorStyles.toolbarTextField);
                _loopFilter = (EVfxLoopFilter)EditorGUILayout.EnumPopup(_loopFilter, EditorStyles.toolbarPopup, GUILayout.Width(90.0f));
                _isFavoritesOnly = GUILayout.Toggle(_isFavoritesOnly, "Favorites", EditorStyles.toolbarButton, GUILayout.Width(80.0f));
                if (EditorGUI.EndChangeCheck())
                {
                    _pageIdx = 0;
                    FilterCatalog();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                _colorFilter = (EVfxColor)EditorGUILayout.EnumPopup("Color", _colorFilter);
                if (EditorGUI.EndChangeCheck())
                {
                    _pageIdx = 0;
                    FilterCatalog();
                }

                EditorGUI.BeginChangeCheck();
                _thumbnailTimeSec = EditorGUILayout.Slider("Frame (sec)", _thumbnailTimeSec, 0.0f, 10.0f);
                if (EditorGUI.EndChangeCheck())
                {
                    ClearThumbnails();
                    RestoreColors();
                    FilterCatalog();
                }

                using (new EditorGUI.DisabledScope(_isIndexDirty || EditorApplication.isPlayingOrWillChangePlaymode))
                {
                    if (GUILayout.Button(_isIndexingColors ? "Stop analysis" : "Analyze colors"))
                    {
                        _isIndexingColors = !_isIndexingColors;
                        _scanIdx = 0;
                    }
                }
            }

            EditorGUILayout.LabelField("Analyzed: " + _thumbnails.AnalyzedCnt + " / " + _catalog.Items.Count + " · textures: " + _thumbnails.TextureCnt + " / " + VfxThumbnailIndex.MAX_TEXTURE_CNT, EditorStyles.miniLabel);
            if (!string.IsNullOrEmpty(_thumbnails.StorageError))
                EditorGUILayout.HelpBox(_thumbnails.StorageError, MessageType.Warning);

            if (!string.IsNullOrEmpty(_thumbnailError))
                EditorGUILayout.HelpBox(_thumbnailError, MessageType.Warning);

            if (_isIndexDirty)
                EditorGUILayout.HelpBox("에셋이 변경되었습니다. Refresh로 목록을 갱신하세요.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawResults();
                DrawPreview();
            }
        }

        //============================================================
        // Persistence
        //============================================================
        private void SaveFavorites()
        {
            EditorPrefs.SetString(_favoritesKey, string.Join(";", _favoriteGuids));
        }

        //============================================================
        // Logic
        //============================================================
        [MenuItem("Tools/UnityTools/VFX Browser")]
        public static void Open()
        {
            VfxBrowserWindow window = GetWindow<VfxBrowserWindow>("VFX Browser");
            window.minSize = new Vector2(740.0f, 430.0f);
            window.Show();
        }

        private void RefreshCatalog()
        {
            bool isRefreshed = _catalog.TryRefresh(_rootPath);
            if (!isRefreshed)
            {
                _isIndexDirty = true;
                return;
            }

            _isIndexDirty = false;
            ClearThumbnails();
            RestoreColors();
            _pageIdx = 0;
            FilterCatalog();
            ReleasePreview();
            for (int idx = 0; idx < _catalog.Items.Count; idx++)
            {
                if (_catalog.Items[idx].Guid == _selectedGuid)
                {
                    SelectItem(_catalog.Items[idx]);
                    break;
                }
            }
        }

        private void FilterCatalog()
        {
            _catalog.Filter(_query, _loopFilter, _favoriteGuids, _isFavoritesOnly, _results);
            if (_colorFilter != EVfxColor.All)
            {
                for (int idx = _results.Count - 1; idx >= 0; idx--)
                {
                    if (_thumbnails.ColorOf(_results[idx].Guid) != _colorFilter)
                        _results.RemoveAt(idx);
                }
            }

            for (int idx = _results.Count - 1; idx >= 0; idx--)
            {
                bool matches = _library != null ? _library.Matches(_results[idx].Guid, _tag, _collection) : string.IsNullOrWhiteSpace(_tag) && string.IsNullOrWhiteSpace(_collection);
                if (!matches)
                    _results.RemoveAt(idx);
            }

            _pageIdx = Mathf.Clamp(_pageIdx, 0, Mathf.Max(0, (_results.Count - 1) / PAGE_SIZE));
        }

        private void SelectItem(VfxPrefabInfo item)
        {
            ReleasePreview();
            _selectedGuid = item.Guid;
            LoadLabels();
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(item.Guid));
            if (prefab == null)
            {
                _isIndexDirty = true;
                return;
            }

            _preview = new VfxPreviewSession(prefab);
            _lastTickSec = EditorApplication.timeSinceStartup;
            _pendingSec = 0.0f;
        }

        private void ReleasePreview()
        {
            _preview?.Dispose();
            _preview = null;
        }

        private void DrawLibrary()
        {
            EditorGUI.BeginChangeCheck();
            _library = (VfxLibrary)EditorGUILayout.ObjectField("Library", _library, typeof(VfxLibrary), false);
            _tag = EditorGUILayout.TextField("Tag filter", _tag);
            _collection = EditorGUILayout.TextField("Collection filter", _collection);
            if (EditorGUI.EndChangeCheck())
            {
                LoadLabels();
                _pageIdx = 0;
                FilterCatalog();
            }

            if (_library == null && GUILayout.Button("Create library"))
            {
                string path = EditorUtility.SaveFilePanelInProject("Save VFX library", "VfxLibrary", "asset", "태그와 컬렉션을 저장할 Editor 폴더를 선택하세요.");
                if (string.IsNullOrEmpty(path))
                    return;

                if (AssetDatabase.LoadMainAssetAtPath(path) != null)
                {
                    _thumbnailError = "기존 에셋은 Library 필드에서 선택하세요.";
                    return;
                }

                _library = CreateInstance<VfxLibrary>();
                AssetDatabase.CreateAsset(_library, path);
                AssetDatabase.SaveAssets();
                LoadLabels();
            }
        }

        private void LoadLabels()
        {
            VfxLabels labels = _library == null ? null : _library.FindLabels(_selectedGuid);
            _tagsText = labels == null ? string.Empty : labels.TagsText;
            _collectionsText = labels == null ? string.Empty : labels.CollectionsText;
        }

        private void DrawResults()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(280.0f)))
            {
                EditorGUILayout.LabelField(_results.Count + " / " + _catalog.Items.Count + " prefabs", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("<") && _pageIdx > 0)
                        _pageIdx--;

                    EditorGUILayout.LabelField((_pageIdx + 1) + " / " + Mathf.Max(1, (_results.Count + PAGE_SIZE - 1) / PAGE_SIZE), GUILayout.Width(80.0f));
                    if (GUILayout.Button(">") && (_pageIdx + 1) * PAGE_SIZE < _results.Count)
                        _pageIdx++;
                }

                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                string toggledGuid = null;
                int endIdx = Mathf.Min(_results.Count, (_pageIdx + 1) * PAGE_SIZE);
                for (int idx = _pageIdx * PAGE_SIZE; idx < endIdx; idx++)
                {
                    VfxPrefabInfo item = _results[idx];
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        Rect thumbnailRect = GUILayoutUtility.GetRect(48.0f, 48.0f, GUILayout.Width(48.0f));
                        if (_thumbnails.TryGetTexture(item.Guid, out Texture2D thumbnail))
                            GUI.DrawTexture(thumbnailRect, thumbnail, ScaleMode.ScaleToFit);

                        bool isFavorite = _favoriteGuids.Contains(item.Guid);
                        bool shouldFavorite = GUILayout.Toggle(isFavorite, "★", EditorStyles.miniButton, GUILayout.Width(28.0f));
                        if (shouldFavorite != isFavorite)
                            toggledGuid = item.Guid;

                        GUIStyle style = item.Guid == _selectedGuid ? EditorStyles.boldLabel : EditorStyles.label;
                        if (GUILayout.Button(new GUIContent(item.Name, item.AssetPath), style))
                            SelectItem(item);
                    }

                    EditorGUILayout.LabelField(item.ParticleSystemCnt + " systems · " + (item.IsLooping ? "Loop" : "One shot") + " · " + _thumbnails.ColorOf(item.Guid), EditorStyles.miniLabel);
                }

                EditorGUILayout.EndScrollView();
                if (toggledGuid != null)
                {
                    if (!_favoriteGuids.Remove(toggledGuid))
                        _favoriteGuids.Add(toggledGuid);

                    SaveFavorites();
                    FilterCatalog();
                }
            }
        }

        private void DrawPreview()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                if (_preview == null)
                {
                    EditorGUILayout.HelpBox("파티클 prefab을 선택하세요. 이 미리보기는 프로젝트 스크립트·Animator·오디오를 실행하지 않습니다.", MessageType.Info);
                    return;
                }

                using (new EditorGUI.DisabledScope(_library == null))
                {
                    _tagsText = EditorGUILayout.TextField("Tags (comma)", _tagsText);
                    _collectionsText = EditorGUILayout.TextField("Collections (comma)", _collectionsText);
                    if (GUILayout.Button("Apply labels"))
                    {
                        Undo.RecordObject(_library, "Apply VFX labels");
                        _library.SetLabels(_selectedGuid, _tagsText, _collectionsText);
                        EditorUtility.SetDirty(_library);
                        AssetDatabase.SaveAssets();
                        LoadLabels();
                        FilterCatalog();
                    }
                }

                Rect rect = GUILayoutUtility.GetRect(200.0f, 200.0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                if (Event.current.type == EventType.Repaint)
                    GUI.DrawTexture(rect, _preview.Render(rect), ScaleMode.StretchToFill, false);

                if (rect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
                    {
                        _preview.Orbit(Event.current.delta);
                        Event.current.Use();
                        Repaint();
                    }
                    else if (Event.current.type == EventType.ScrollWheel)
                    {
                        _preview.Zoom(Event.current.delta.y);
                        Event.current.Use();
                        Repaint();
                    }
                }

                EditorGUI.BeginChangeCheck();
                float timeSec = EditorGUILayout.Slider("Time (sec)", _preview.TimeSec, 0.0f, _durationSec);
                if (EditorGUI.EndChangeCheck())
                {
                    _isPlaying = false;
                    bool isSought = _preview.TrySeek(timeSec);
                    if (isSought)
                        Repaint();
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    _isPlaying = GUILayout.Toggle(_isPlaying, "Play", EditorStyles.miniButton);
                    if (GUILayout.Button("Restart"))
                    {
                        bool isRestarted = _preview.TrySeek(0.0f);
                        _isPlaying = isRestarted;
                    }

                    if (GUILayout.Button("Fit"))
                        _preview.Fit();

                    if (GUILayout.Button("Compare"))
                        VfxCompareWindow.Open();

                    if (GUILayout.Button("Ping prefab"))
                        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(_selectedGuid)));
                }

                _durationSec = EditorGUILayout.Slider("Replay (sec)", _durationSec, 0.1f, VfxPreviewSession.MAX_PREVIEW_SEC);
                _speed = EditorGUILayout.Slider("Speed", _speed, 0.1f, 3.0f);
                EditorGUILayout.LabelField("Live particles: " + _preview.ParticleCnt);
            }
        }

        //============================================================
        // Callbacks
        //============================================================
        private void TickPreview()
        {
            double nowSec = EditorApplication.timeSinceStartup;
            float deltaSec = (float)(nowSec - _lastTickSec);
            _lastTickSec = nowSec;
            if (_preview == null || !_isPlaying || !hasFocus || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                _pendingSec = 0.0f;
                return;
            }

            _pendingSec += Mathf.Clamp(deltaSec, 0.0f, 0.1f);
            if (_pendingSec < 1.0f / 30.0f)
                return;

            float stepSec = _pendingSec * _speed;
            _pendingSec = 0.0f;
            if (_preview.TimeSec + stepSec >= _durationSec)
            {
                bool isRestarted = _preview.TrySeek(0.0f);
                _isPlaying = isRestarted;
            }
            else
            {
                _preview.Advance(stepSec);
            }

            Repaint();
        }

        private void HandleProjectChanged()
        {
            _isIndexDirty = true;
            ReleasePreview();
            ClearThumbnails();
            Repaint();
        }

        private void HandlePlayModeChanged(PlayModeStateChange state)
        {
            ReleasePreview();
            ClearThumbnails();
            if (state == PlayModeStateChange.EnteredEditMode)
                RefreshCatalog();
        }

        private void ClearThumbnails()
        {
            _thumbnails.Dispose();
            _failedGuids.Clear();
            _isIndexingColors = false;
            _scanIdx = 0;
            _thumbnailError = string.Empty;
        }

        private void RestoreColors()
        {
            for (int idx = 0; idx < _catalog.Items.Count; idx++)
            {
                _thumbnails.RestoreColor(_catalog.Items[idx].Guid, _thumbnailTimeSec);
            }
        }

        private void TickThumbnails()
        {
            if (!hasFocus || _isIndexDirty || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.timeSinceStartup < _nextThumbnailSec)
                return;

            _nextThumbnailSec = EditorApplication.timeSinceStartup + 0.1;
            VfxPrefabInfo item = null;
            if (_isIndexingColors)
            {
                while (_scanIdx < _catalog.Items.Count)
                {
                    VfxPrefabInfo candidate = _catalog.Items[_scanIdx++];
                    if (_thumbnails.ColorOf(candidate.Guid) == EVfxColor.Unanalyzed && !_failedGuids.Contains(candidate.Guid))
                    {
                        item = candidate;
                        break;
                    }
                }

                if (item == null)
                    _isIndexingColors = false;
            }
            else
            {
                int endIdx = Mathf.Min(_results.Count, (_pageIdx + 1) * PAGE_SIZE);
                for (int idx = _pageIdx * PAGE_SIZE; idx < endIdx; idx++)
                {
                    VfxPrefabInfo candidate = _results[idx];
                    if (!_failedGuids.Contains(candidate.Guid) && !_thumbnails.TryGetTexture(candidate.Guid, out Texture2D texture))
                    {
                        item = candidate;
                        break;
                    }
                }
            }

            if (item == null)
                return;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(item.Guid));
            try
            {
                if (!_thumbnails.TryCapture(item.Guid, prefab, _thumbnailTimeSec))
                {
                    _failedGuids.Add(item.Guid);
                    _thumbnailError = "썸네일을 생성하지 못했습니다. 그래픽 장치와 prefab을 확인한 뒤 Refresh하세요.";
                }
            }
            catch (UnityException ex)
            {
                _failedGuids.Add(item.Guid);
                _thumbnailError = "썸네일 렌더링에 실패했습니다: " + ex.Message;
            }

            FilterCatalog();
            Repaint();
        }
    }
}
