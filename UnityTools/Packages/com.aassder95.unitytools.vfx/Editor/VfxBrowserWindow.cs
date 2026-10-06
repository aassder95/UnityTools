using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Vfx.Editor
{
    public class VfxBrowserWindow : EditorWindow
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly VfxPrefabCatalog _catalog = new VfxPrefabCatalog();
        private readonly List<VfxPrefabInfo> _results = new List<VfxPrefabInfo>();
        private readonly HashSet<string> _favoriteGuids = new HashSet<string>();

        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private string _rootPath = "Assets";
        [SerializeField] private string _query = string.Empty;
        [SerializeField] private string _selectedGuid = string.Empty;
        [SerializeField] private EVfxLoopFilter _loopFilter;
        [SerializeField] private bool _isFavoritesOnly;
        [SerializeField] private float _durationSec = 5.0f;
        [SerializeField] private float _speed = 1.0f;

        //============================================================
        // Fields
        //============================================================
        private VfxPreviewSession _preview;
        private DefaultAsset _folder;
        private Vector2 _scrollPos;
        private string _favoritesKey;
        private bool _isPlaying = true;
        private bool _isIndexDirty;
        private double _lastTickSec;
        private float _pendingSec;

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
        }

        private void OnDisable()
        {
            EditorApplication.update -= TickPreview;
            EditorApplication.projectChanged -= HandleProjectChanged;
            EditorApplication.playModeStateChanged -= HandlePlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= ReleasePreview;
            ReleasePreview();
        }

        private void OnGUI()
        {
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
                    FilterCatalog();
            }

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
        }

        private void SelectItem(VfxPrefabInfo item)
        {
            ReleasePreview();
            _selectedGuid = item.Guid;
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

        private void DrawResults()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(280.0f)))
            {
                EditorGUILayout.LabelField(_results.Count + " / " + _catalog.Items.Count + " prefabs", EditorStyles.boldLabel);
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                string toggledGuid = null;
                for (int idx = 0; idx < _results.Count; idx++)
                {
                    VfxPrefabInfo item = _results[idx];
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        bool isFavorite = _favoriteGuids.Contains(item.Guid);
                        bool shouldFavorite = GUILayout.Toggle(isFavorite, "★", EditorStyles.miniButton, GUILayout.Width(28.0f));
                        if (shouldFavorite != isFavorite)
                            toggledGuid = item.Guid;

                        GUIStyle style = item.Guid == _selectedGuid ? EditorStyles.boldLabel : EditorStyles.label;
                        if (GUILayout.Button(new GUIContent(item.Name, item.AssetPath), style))
                            SelectItem(item);
                    }

                    EditorGUILayout.LabelField(item.ParticleSystemCnt + " systems · " + (item.IsLooping ? "Loop" : "One shot"), EditorStyles.miniLabel);
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
            Repaint();
        }

        private void HandlePlayModeChanged(PlayModeStateChange state)
        {
            ReleasePreview();
            if (state == PlayModeStateChange.EnteredEditMode)
                RefreshCatalog();
        }
    }
}
