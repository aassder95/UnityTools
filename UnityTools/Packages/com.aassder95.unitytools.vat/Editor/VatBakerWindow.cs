using UnityEditor;
using UnityEngine;

namespace UnityTools.Vat.Editor
{
    public class VatBakerWindow : EditorWindow
    {
        //============================================================
        // Fields
        //============================================================
        private GameObject _root;
        private SkinnedMeshRenderer _skin;
        private AnimationClip _animation;
        private float _fps = 30.0f;
        private string _status;

        //============================================================
        // Init/Register
        //============================================================
        [MenuItem("Tools/UnityTools/VAT/Baker")]
        public static void Open()
        {
            GetWindow<VatBakerWindow>("VAT Baker");
        }

        //============================================================
        // Unity Methods
        //============================================================
        private void OnGUI()
        {
            _root = (GameObject)EditorGUILayout.ObjectField("Animation root", _root, typeof(GameObject), true);
            _skin = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Skinned renderer", _skin, typeof(SkinnedMeshRenderer), true);
            _animation = (AnimationClip)EditorGUILayout.ObjectField("Clip", _animation, typeof(AnimationClip), false);
            _fps = EditorGUILayout.FloatField("Sample FPS", _fps);
            EditorGUILayout.HelpBox("단일 SkinnedMeshRenderer를 복제한 bone 계층에서 베이킹합니다. 프로젝트 스크립트는 실행하지 않습니다. 새 VAT asset에 Mesh와 position/normal 텍스처를 저장합니다.", MessageType.Info);
            if (GUILayout.Button("Bake new asset"))
            {
                string path = EditorUtility.SaveFilePanelInProject("VAT asset", "VatClip", "asset", "새 에셋 위치를 선택하세요.");
                if (string.IsNullOrEmpty(path))
                    return;

                bool isBaked = VatBaker.TryBake(_root, _skin, _animation, _fps, path, out VatClip clip, out string error);
                _status = isBaked ? "베이킹 완료: " + clip.FrameCnt + " frames" : error;
                if (isBaked)
                    EditorGUIUtility.PingObject(clip);
            }

            if (!string.IsNullOrEmpty(_status))
                EditorGUILayout.HelpBox(_status, MessageType.Info);
        }
    }
}
