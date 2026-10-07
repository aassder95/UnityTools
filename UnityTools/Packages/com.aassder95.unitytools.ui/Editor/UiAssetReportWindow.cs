using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace UnityTools.Ui.Editor
{
    public class UiAssetReportWindow : EditorWindow
    {
        //============================================================
        // Fields
        //============================================================
        private string _folder = "Assets";
        private UnityEngine.Object _filter;
        private IReadOnlyList<UiImageUsage> _usages;
        private Vector2 _scroll;
        private bool _hasMissingOnly;
        private string _error;

        //============================================================
        // Init/Register
        //============================================================
        [MenuItem("Tools/UnityTools/UI/Asset Report")]
        public static void Open()
        {
            GetWindow<UiAssetReportWindow>("UI Asset Report");
        }

        //============================================================
        // Unity Methods
        //============================================================
        private void OnGUI()
        {
            _folder = EditorGUILayout.TextField("Prefab folder", _folder);
            UnityEngine.Object candidate = EditorGUILayout.ObjectField("Sprite / Texture / Atlas", _filter, typeof(UnityEngine.Object), false);
            if (candidate == null || candidate is Sprite || candidate is Texture || candidate is SpriteAtlas)
                _filter = candidate;

            _hasMissingOnly = EditorGUILayout.Toggle("Missing sprite / atlas only", _hasMissingOnly);
            if (GUILayout.Button("Scan"))
            {
                bool isScanned = UiAssetReport.TryScan(_folder, out _usages);
                _error = null;
                if (!isScanned)
                    _error = "프로젝트의 유효한 폴더 경로를 입력하세요.";
            }

            EditorGUILayout.HelpBox("비활성 Image와 저장된 Sprite를 포함합니다. 런타임 overrideSprite 변경은 추적하지 않습니다. Atlas는 packable 설정 기준이며 실제 빌드 포함·GPU 배칭 판정이 아닙니다. Sprite가 없는 Image도 의도된 설정일 수 있습니다. 분석 결과는 Scan 시점의 스냅샷입니다.", MessageType.Info);
            if (!string.IsNullOrEmpty(_error))
                EditorGUILayout.HelpBox(_error, MessageType.Warning);

            if (_usages == null)
                return;

            EditorGUILayout.LabelField("Image entries: " + _usages.Count);
            using (var scroll = new EditorGUILayout.ScrollViewScope(_scroll))
            {
                _scroll = scroll.scrollPosition;
                foreach (UiImageUsage usage in _usages)
                {
                    if (_hasMissingOnly && usage.HasSprite && usage.HasAtlas)
                        continue;

                    bool hasMatch = _filter == null || _filter == usage.Sprite || _filter == usage.Texture;
                    for (int idx = 0; idx < usage.Atlases.Count; idx++)
                    {
                        hasMatch |= _filter == usage.Atlases[idx];
                    }

                    if (!hasMatch)
                        continue;

                    EditorGUILayout.LabelField(usage.PrefabPath + " / " + usage.ImagePath, EditorStyles.boldLabel);
                    EditorGUILayout.ObjectField("Sprite", usage.Sprite, typeof(Sprite), false);
                    EditorGUILayout.ObjectField("Texture", usage.Texture, typeof(Texture), false);
                    EditorGUILayout.ObjectField("Material", usage.Material, typeof(Material), false);
                    foreach (SpriteAtlas atlas in usage.Atlases)
                    {
                        EditorGUILayout.ObjectField("Atlas", atlas, typeof(SpriteAtlas), false);
                    }

                    if (!usage.HasSprite || !usage.HasAtlas)
                        EditorGUILayout.LabelField(usage.HasSprite ? "Atlas packable 없음" : "Sprite 없음");

                    if (GUILayout.Button("Ping prefab"))
                        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(usage.PrefabPath));
                }
            }
        }
    }
}
