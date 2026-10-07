using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityTools.Vat.Tests
{
    public class VatPlayerTests
    {
        //============================================================
        // Logic
        //============================================================
        [UnityTest]
        public IEnumerator PlaybackWritesPropertyBlockAndKeepsSharedMaterialUnchanged()
        {
            var go = new GameObject("Vat", typeof(MeshFilter), typeof(MeshRenderer));
            var mesh = new Mesh { vertices = new[] { Vector3.zero } };
            var positions = new Texture2D(1, 2, TextureFormat.RGBAFloat, false, true);
            var normals = new Texture2D(1, 2, TextureFormat.RGBAFloat, false, true);
            var clip = ScriptableObject.CreateInstance<VatClip>();
            var material = new Material(Shader.Find("UnityTools/VAT/Unlit"));
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            try
            {
                Assert.That(clip.TryConfigure(mesh, positions, normals, 1.0f, 2), Is.True);
                var player = go.AddComponent<VatPlayer>();
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                typeof(VatPlayer).GetField("_meshFilter", flags).SetValue(player, go.GetComponent<MeshFilter>());
                typeof(VatPlayer).GetField("_renderer", flags).SetValue(player, renderer);
                typeof(VatPlayer).GetField("_clip", flags).SetValue(player, clip);
                typeof(VatPlayer).GetField("_shouldLoop", flags).SetValue(player, false);
                player.Init();
                Assert.That(player.TrySeek(0.5f), Is.True);
                var properties = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(properties);
                Assert.That(properties.GetFloat("_VatFrame"), Is.EqualTo(0.5f));
                Assert.That(material.GetFloat("_VatFrame"), Is.Zero);
                Assert.That(player.TrySeek(float.NaN), Is.False);
                Assert.That(player.TryPlay(1000.0f), Is.True);
                yield return null;
                Assert.That(player.TimeSec, Is.EqualTo(1.0f));
                Assert.That(player.IsPlaying, Is.False);
                Assert.That(renderer.sharedMaterial, Is.SameAs(material));
                player.Release();
                Assert.That(player.TryPlay(), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(clip);
                Object.DestroyImmediate(mesh);
                Object.DestroyImmediate(positions);
                Object.DestroyImmediate(normals);
                Object.DestroyImmediate(material);
            }
        }
    }
}
