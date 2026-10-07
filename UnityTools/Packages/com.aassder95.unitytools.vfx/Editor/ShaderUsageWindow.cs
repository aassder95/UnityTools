using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Vfx.Editor
{
    public class ShaderUsageWindow : EditorWindow
    {
        //============================================================
        // Fields
        //============================================================
        private string _folder = "Assets";
        private string _excludedFolders = string.Empty;
        private Shader _shader;
        private IReadOnlyList<ShaderUsage> _usages;
        private Vector2 _scroll;
        private string _error;

        //============================================================
        // Init/Register
        //============================================================
        [MenuItem("Tools/UnityTools/VFX/Shader Usage")]
        public static void Open()
        {
            GetWindow<ShaderUsageWindow>("Shader Usage");
        }

        //============================================================
        // Unity Methods
        //============================================================
        private void OnGUI()
        {
            _folder = EditorGUILayout.TextField("Material folder", _folder);
            _excludedFolders = EditorGUILayout.TextField("Exclude folders (;)", _excludedFolders);
            _shader = (Shader)EditorGUILayout.ObjectField("Shader filter", _shader, typeof(Shader), false);
            if (GUILayout.Button("Scan"))
            {
                string[] folders = _excludedFolders.Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
                for (int idx = 0; idx < folders.Length; idx++)
                {
                    folders[idx] = folders[idx].Trim();
                }

                bool isScanned = ShaderUsageReport.TryScan(_folder, folders, out _usages);
                _error = isScanned ? null : "검색 및 제외 폴더 경로를 확인하세요.";
            }

            EditorGUILayout.HelpBox("선택 폴더의 Material 에셋을 Shader별로 집계합니다. 미사용 Material도 포함하며 실제 scene 사용·shader variant·빌드 포함 여부는 판정하지 않습니다. 결과는 Scan 시점의 스냅샷입니다.", MessageType.Info);
            if (!string.IsNullOrEmpty(_error))
                EditorGUILayout.HelpBox(_error, MessageType.Warning);

            if (_usages == null)
                return;

            using (var scroll = new EditorGUILayout.ScrollViewScope(_scroll))
            {
                _scroll = scroll.scrollPosition;
                foreach (ShaderUsage usage in _usages)
                {
                    if (_shader != null && usage.Shader != _shader)
                        continue;

                    EditorGUILayout.ObjectField("Shader", usage.Shader, typeof(Shader), false);
                    EditorGUILayout.LabelField("Materials: " + usage.Materials.Count);
                    foreach (Material material in usage.Materials)
                    {
                        EditorGUILayout.ObjectField(AssetDatabase.GetAssetPath(material), material, typeof(Material), false);
                    }
                }
            }
        }
    }
}
