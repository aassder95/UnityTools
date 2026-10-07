using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UnityTools.Vat.Editor.Tests
{
    public class VatBakerTests
    {
        //============================================================
        // Logic
        //============================================================
        [Test]
        public void BakesBoneMotionPreservesSourceAndRendersSelectedFrames()
        {
            string folder = "Assets/VatBake_" + System.Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", folder.Substring(7));
            var root = new GameObject("Root");
            var bone = new GameObject("Bone");
            bone.transform.SetParent(root.transform, false);
            var skinGo = new GameObject("Skin", typeof(SkinnedMeshRenderer));
            skinGo.transform.SetParent(root.transform, false);
            var mesh = new Mesh();
            mesh.vertices = new[] { new Vector3(-0.5f, -0.5f, 0.0f), new Vector3(0.0f, 0.5f, 0.0f), new Vector3(0.5f, -0.5f, 0.0f) };
            mesh.normals = new[] { Vector3.back, Vector3.back, Vector3.back };
            mesh.uv = new[] { Vector2.zero, Vector2.up, Vector2.right };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.bindposes = new[] { Matrix4x4.identity };
            mesh.boneWeights = new[] { new BoneWeight { boneIndex0 = 0, weight0 = 1.0f }, new BoneWeight { boneIndex0 = 0, weight0 = 1.0f }, new BoneWeight { boneIndex0 = 0, weight0 = 1.0f } };
            var skin = skinGo.GetComponent<SkinnedMeshRenderer>();
            skin.sharedMesh = mesh;
            skin.bones = new[] { bone.transform };
            skin.rootBone = bone.transform;
            var animation = new AnimationClip { legacy = true };
            animation.SetCurve("Bone", typeof(Transform), "localPosition.x", AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f));
            int sceneCnt = EditorSceneManager.previewSceneCount;
            try
            {
                Assert.That(VatBaker.TryBake(root, skin, animation, 2.0f, folder + "/clip.asset", out var clip, out string error), Is.True, error);
                Assert.That(clip.FrameCnt, Is.EqualTo(3));
                Assert.That(clip.Positions.GetPixel(0, 0).r, Is.EqualTo(-0.5f).Within(0.001f));
                Assert.That(clip.Positions.GetPixel(0, 2).r, Is.EqualTo(0.5f).Within(0.001f));
                Assert.That(bone.transform.localPosition, Is.EqualTo(Vector3.zero));
                Assert.That(mesh.vertices[0].x, Is.EqualTo(-0.5f));
                Assert.That(EditorSceneManager.previewSceneCount, Is.EqualTo(sceneCnt));
                Assert.That(VatBaker.TryBake(root, skin, animation, 2.0f, folder + "/clip.asset", out _, out _), Is.False);
                Shader shader = Shader.Find("UnityTools/VAT/Unlit");
                Assert.That(shader, Is.Not.Null);
                Assert.That(ShaderUtil.ShaderHasError(shader), Is.False);
                float first = CaptureCentroid(clip, shader, 0.0f);
                float last = CaptureCentroid(clip, shader, 2.0f);
                Assert.That(last, Is.GreaterThan(first + 5.0f));
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(mesh);
                Object.DestroyImmediate(animation);
                AssetDatabase.DeleteAsset(folder);
            }
        }

        //============================================================
        // Utilities
        //============================================================
        private static float CaptureCentroid(VatClip clip, Shader shader, float frame)
        {
            var preview = new PreviewRenderUtility();
            var go = new GameObject("VAT Render", typeof(MeshFilter), typeof(MeshRenderer));
            var material = new Material(shader);
            Texture2D image = null;
            try
            {
                preview.AddSingleGO(go);
                go.GetComponent<MeshFilter>().sharedMesh = clip.Mesh;
                go.GetComponent<MeshRenderer>().sharedMaterial = material;
                material.SetTexture("_VatPositions", clip.Positions);
                material.SetFloat("_VatFrame", frame);
                preview.camera.transform.position = new Vector3(0.5f, 0.0f, -4.0f);
                preview.camera.transform.rotation = Quaternion.identity;
                preview.camera.clearFlags = CameraClearFlags.SolidColor;
                preview.camera.backgroundColor = Color.black;
                preview.camera.nearClipPlane = 0.1f;
                preview.camera.farClipPlane = 10.0f;
                preview.BeginStaticPreview(new Rect(0, 0, 128, 128));
                preview.Render(true);
                image = preview.EndStaticPreview();
                Color[] pixels = image.GetPixels();
                float total = 0.0f;
                int cnt = 0;
                for (int idx = 0; idx < pixels.Length; idx++)
                {
                    if (pixels[idx].maxColorComponent < 0.5f)
                        continue;

                    total += idx % 128;
                    cnt++;
                }

                Assert.That(cnt, Is.GreaterThan(20));
                return total / cnt;
            }
            finally
            {
                if (image != null)
                    Object.DestroyImmediate(image);

                preview.Cleanup();
                Object.DestroyImmediate(material);
            }
        }
    }
}
