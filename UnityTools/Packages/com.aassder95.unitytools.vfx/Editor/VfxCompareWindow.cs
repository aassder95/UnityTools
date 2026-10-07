using UnityEditor;
using UnityEngine;

namespace UnityTools.Vfx.Editor
{
    public class VfxCompareWindow : EditorWindow
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly VfxComparison _comparison = new VfxComparison();

        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Prefabs")]
        [SerializeField] private GameObject _leftPrefab;
        [SerializeField] private GameObject _rightPrefab;
        [Header("Playback")]
        [SerializeField] private float _durationSec = 5.0f;
        [SerializeField] private float _speed = 1.0f;

        //============================================================
        // Fields
        //============================================================
        private bool _isPlaying;
        private double _lastTickSec;
        private float _pendingSec;
        private string _error = string.Empty;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnEnable()
        {
            EditorApplication.update += Tick;
            EditorApplication.projectChanged += HandleProjectChanged;
            EditorApplication.playModeStateChanged += HandlePlayMode;
            AssemblyReloadEvents.beforeAssemblyReload += ReleasePreviews;
            _lastTickSec = EditorApplication.timeSinceStartup;
            if (_leftPrefab != null && _rightPrefab != null && !EditorApplication.isPlayingOrWillChangePlaymode)
                RefreshPreviews();
        }

        private void OnDisable()
        {
            EditorApplication.update -= Tick;
            EditorApplication.projectChanged -= HandleProjectChanged;
            EditorApplication.playModeStateChanged -= HandlePlayMode;
            AssemblyReloadEvents.beforeAssemblyReload -= ReleasePreviews;
            ReleasePreviews();
        }

        private void OnGUI()
        {
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                EditorGUI.BeginChangeCheck();
                _leftPrefab = (GameObject)EditorGUILayout.ObjectField("Left", _leftPrefab, typeof(GameObject), false);
                _rightPrefab = (GameObject)EditorGUILayout.ObjectField("Right", _rightPrefab, typeof(GameObject), false);
                if (EditorGUI.EndChangeCheck())
                    RefreshPreviews();

                if (GUILayout.Button("Refresh previews"))
                    RefreshPreviews();
            }

            if (!string.IsNullOrEmpty(_error))
                EditorGUILayout.HelpBox(_error, MessageType.Info);

            if (!_comparison.HasPreviews)
                return;

            Rect leftRect;
            Rect rightRect;
            using (new EditorGUILayout.HorizontalScope())
            {
                leftRect = GUILayoutUtility.GetRect(180.0f, 180.0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                rightRect = GUILayoutUtility.GetRect(180.0f, 180.0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            }

            if (Event.current.type == EventType.Repaint)
            {
                GUI.DrawTexture(leftRect, _comparison.Left.Render(leftRect), ScaleMode.StretchToFill, false);
                GUI.DrawTexture(rightRect, _comparison.Right.Render(rightRect), ScaleMode.StretchToFill, false);
            }

            if (leftRect.Contains(Event.current.mousePosition) || rightRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
                {
                    _comparison.Orbit(Event.current.delta);
                    Event.current.Use();
                    Repaint();
                }
                else if (Event.current.type == EventType.ScrollWheel)
                {
                    _comparison.Zoom(Event.current.delta.y);
                    Event.current.Use();
                    Repaint();
                }
            }

            EditorGUI.BeginChangeCheck();
            float timeSec = EditorGUILayout.Slider("Time (sec)", _comparison.TimeSec, 0.0f, _durationSec);
            if (EditorGUI.EndChangeCheck())
            {
                _isPlaying = false;
                bool isSought = _comparison.TrySeek(timeSec);
                if (isSought)
                    Repaint();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                _isPlaying = GUILayout.Toggle(_isPlaying, "Play", EditorStyles.miniButton);
                if (GUILayout.Button("Restart"))
                    _isPlaying = _comparison.TrySeek(0.0f);

                if (GUILayout.Button("Fit together"))
                    _comparison.Fit();

                if (GUILayout.Button("Export PNG"))
                {
                    _isPlaying = false;
                    string path = EditorUtility.SaveFilePanel("Save VFX comparison", "", "VfxComparison-" + _comparison.TimeSec.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + "sec", "png");
                    if (!string.IsNullOrEmpty(path))
                    {
                        bool isSaved = VfxComparisonExport.TrySave(_comparison, path, 512, out _error);
                        if (isSaved)
                            _error = "비교 이미지를 저장했습니다: " + path;
                    }
                }
            }

            _durationSec = EditorGUILayout.Slider("Replay (sec)", _durationSec, 0.1f, VfxPreviewSession.MAX_PREVIEW_SEC);
            _speed = EditorGUILayout.Slider("Speed", _speed, 0.1f, 3.0f);
            EditorGUILayout.LabelField("Live particles: " + _comparison.Left.ParticleCnt + " / " + _comparison.Right.ParticleCnt);
        }

        //============================================================
        // Logic
        //============================================================
        [MenuItem("Tools/UnityTools/VFX Compare")]
        public static void Open()
        {
            VfxCompareWindow window = GetWindow<VfxCompareWindow>("VFX Compare");
            window.minSize = new Vector2(600.0f, 450.0f);
            window.Show();
        }

        private void RefreshPreviews()
        {
            ReleasePreviews();
            bool isReady = _comparison.TrySetPrefabs(_leftPrefab, _rightPrefab);
            _error = isReady ? string.Empty : "좌우에 파티클 prefab을 연결하세요.";
            _isPlaying = isReady;
            _lastTickSec = EditorApplication.timeSinceStartup;
            Repaint();
        }

        private void ReleasePreviews()
        {
            _comparison.Dispose();
            _isPlaying = false;
            _pendingSec = 0.0f;
        }

        //============================================================
        // Callbacks
        //============================================================
        private void HandleProjectChanged()
        {
            ReleasePreviews();
            _error = "에셋이 변경되었습니다. Refresh previews로 갱신하세요.";
            Repaint();
        }

        private void HandlePlayMode(PlayModeStateChange state)
        {
            ReleasePreviews();
            if (state == PlayModeStateChange.EnteredEditMode)
                RefreshPreviews();
        }

        private void Tick()
        {
            double nowSec = EditorApplication.timeSinceStartup;
            float deltaSec = (float)(nowSec - _lastTickSec);
            _lastTickSec = nowSec;
            if (!_comparison.HasPreviews || !_isPlaying || !hasFocus || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                _pendingSec = 0.0f;
                return;
            }

            _pendingSec += Mathf.Clamp(deltaSec, 0.0f, 0.1f);
            if (_pendingSec < 1.0f / 30.0f)
                return;

            float stepSec = _pendingSec * _speed;
            _pendingSec = 0.0f;
            if (_comparison.TimeSec + stepSec >= _durationSec)
                _isPlaying = _comparison.TrySeek(0.0f);
            else
                _comparison.Advance(stepSec);

            Repaint();
        }
    }
}
