using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Object = UnityEngine.Object;

namespace UnityTools.Vat.Editor
{
    public static class VatBaker
    {
        //============================================================
        // Logic
        //============================================================
        public static bool TryBake(GameObject root, SkinnedMeshRenderer skin, AnimationClip animation, float fps, string assetPath, out VatClip clip, out string error)
        {
            clip = null;
            error = null;
            if (root == null || skin == null || animation == null || skin.sharedMesh == null || !skin.transform.IsChildOf(root.transform) || animation.length <= 0.0f || float.IsNaN(fps) || float.IsInfinity(fps) || fps < 1.0f || fps > 120.0f)
            {
                error = "루트·하위 SkinnedMeshRenderer·클립·FPS(1~120)를 확인하세요.";
                return false;
            }

            int vertexCnt = skin.sharedMesh.vertexCount;
            double frames = Math.Ceiling(animation.length * (double)fps) + 1;
            if (vertexCnt <= 0 || vertexCnt > SystemInfo.maxTextureSize || frames > SystemInfo.maxTextureSize || frames * vertexCnt > 4194304 || !SystemInfo.SupportsTextureFormat(TextureFormat.RGBAFloat))
            {
                error = "VAT 텍스처 크기·RGBAFloat 지원·총 픽셀 제한(4194304)을 확인하세요.";
                return false;
            }

            if (string.IsNullOrEmpty(assetPath) || !assetPath.StartsWith("Assets/", StringComparison.Ordinal) || assetPath.Contains("..") || !assetPath.EndsWith(".asset", StringComparison.Ordinal) || !AssetDatabase.IsValidFolder(System.IO.Path.GetDirectoryName(assetPath)?.Replace('\\', '/')) || AssetDatabase.LoadMainAssetAtPath(assetPath) != null)
            {
                error = "존재하는 Assets 폴더의 새 .asset 경로를 지정하세요. 기존 에셋은 덮어쓰지 않습니다.";
                return false;
            }

            int frameCnt = (int)frames;
            Scene scene = EditorSceneManager.NewPreviewScene();
            GameObject copyRoot = null;
            Mesh baked = new Mesh();
            Texture2D positions = null;
            Texture2D normals = null;
            Mesh outputMesh = null;
            bool hasSaved = false;
            try
            {
                var transforms = new Dictionary<Transform, Transform>();
                copyRoot = CloneTransforms(root.transform, null, scene, transforms);
                GameObject copySkin = transforms[skin.transform].gameObject;
                var renderer = copySkin.AddComponent<SkinnedMeshRenderer>();
                EditorUtility.CopySerialized(skin, renderer);
                Transform[] bones = skin.bones;
                for (int idx = 0; idx < bones.Length; idx++)
                {
                    if (bones[idx] != null && !transforms.TryGetValue(bones[idx], out bones[idx]))
                    {
                        error = "루트 외부 bone 참조는 베이킹할 수 없습니다.";
                        return false;
                    }
                }

                renderer.bones = bones;
                if (skin.rootBone != null)
                {
                    if (!transforms.TryGetValue(skin.rootBone, out Transform trRootBone))
                    {
                        error = "루트 외부 rootBone 참조는 베이킹할 수 없습니다.";
                        return false;
                    }

                    renderer.rootBone = trRootBone;
                }

                var positionPixels = new Color[vertexCnt * frameCnt];
                var normalPixels = new Color[positionPixels.Length];
                var vertices = new List<Vector3>(vertexCnt);
                var meshNormals = new List<Vector3>(vertexCnt);
                Bounds bounds = default;
                bool hasBounds = false;
                for (int frame = 0; frame < frameCnt; frame++)
                {
                    animation.SampleAnimation(copyRoot, animation.length * frame / (frameCnt - 1));
                    renderer.BakeMesh(baked);
                    baked.GetVertices(vertices);
                    baked.GetNormals(meshNormals);
                    if (vertices.Count != vertexCnt)
                    {
                        error = "베이킹 중 vertex 수가 변경됐습니다.";
                        return false;
                    }

                    for (int idx = 0; idx < vertexCnt; idx++)
                    {
                        Vector3 pos = vertices[idx];
                        Vector3 normal = meshNormals.Count == vertexCnt ? meshNormals[idx] : Vector3.up;
                        positionPixels[frame * vertexCnt + idx] = new Color(pos.x, pos.y, pos.z, 1.0f);
                        normalPixels[frame * vertexCnt + idx] = new Color(normal.x, normal.y, normal.z, 1.0f);
                        if (hasBounds)
                            bounds.Encapsulate(pos);
                        else
                            bounds = new Bounds(pos, Vector3.zero);

                        hasBounds = true;
                    }
                }

                positions = new Texture2D(vertexCnt, frameCnt, TextureFormat.RGBAFloat, false, true) { name = "Positions", filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
                normals = new Texture2D(vertexCnt, frameCnt, TextureFormat.RGBAFloat, false, true) { name = "Normals", filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
                positions.SetPixels(positionPixels);
                positions.Apply(false, false);
                normals.SetPixels(normalPixels);
                normals.Apply(false, false);
                outputMesh = Object.Instantiate(skin.sharedMesh);
                outputMesh.name = "VAT Mesh";
                outputMesh.bounds = bounds;
                clip = ScriptableObject.CreateInstance<VatClip>();
                clip.name = animation.name;
                if (!clip.TryConfigure(outputMesh, positions, normals, animation.length, frameCnt))
                {
                    error = "VAT 데이터 구성이 실패했습니다.";
                    return false;
                }

                AssetDatabase.CreateAsset(clip, assetPath);
                AssetDatabase.AddObjectToAsset(outputMesh, clip);
                AssetDatabase.AddObjectToAsset(positions, clip);
                AssetDatabase.AddObjectToAsset(normals, clip);
                EditorUtility.SetDirty(clip);
                AssetDatabase.SaveAssets();
                hasSaved = true;
                return true;
            }
            catch (Exception ex)
            {
                error = "VAT 베이킹 실패: " + ex.Message;
                return false;
            }
            finally
            {
                Object.DestroyImmediate(baked);
                if (copyRoot != null)
                    Object.DestroyImmediate(copyRoot);

                EditorSceneManager.ClosePreviewScene(scene);
                if (!hasSaved)
                {
                    if (AssetDatabase.LoadMainAssetAtPath(assetPath) == clip && clip != null)
                        AssetDatabase.DeleteAsset(assetPath);

                    if (positions != null && !EditorUtility.IsPersistent(positions))
                        Object.DestroyImmediate(positions);

                    if (normals != null && !EditorUtility.IsPersistent(normals))
                        Object.DestroyImmediate(normals);

                    if (outputMesh != null && !EditorUtility.IsPersistent(outputMesh))
                        Object.DestroyImmediate(outputMesh);

                    if (clip != null && !EditorUtility.IsPersistent(clip))
                        Object.DestroyImmediate(clip);

                    clip = null;
                }
            }
        }

        //============================================================
        // Utilities
        //============================================================
        private static GameObject CloneTransforms(Transform source, Transform parent, Scene scene, Dictionary<Transform, Transform> transforms)
        {
            var go = new GameObject(source.name) { hideFlags = HideFlags.HideAndDontSave };
            SceneManager.MoveGameObjectToScene(go, scene);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = source.localPosition;
            go.transform.localRotation = source.localRotation;
            go.transform.localScale = source.localScale;
            transforms.Add(source, go.transform);
            for (int idx = 0; idx < source.childCount; idx++)
            {
                CloneTransforms(source.GetChild(idx), go.transform, scene, transforms);
            }

            return go;
        }
    }
}
