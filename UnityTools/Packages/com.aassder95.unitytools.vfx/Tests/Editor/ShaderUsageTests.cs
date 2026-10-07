using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Vfx.Editor.Tests
{
    public class ShaderUsageTests
    {
        //============================================================
        // Logic
        //============================================================
        [Test]
        public void ExclusionsRespectFolderBoundariesAndSubassetsAreCounted()
        {
            string folder = "Assets/ShaderReport_" + System.Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", folder.Substring(7));
            try
            {
                AssetDatabase.CreateFolder(folder, "Skip");
                AssetDatabase.CreateFolder(folder, "SkipMore");
                Shader shader = Shader.Find("UI/Default");
                Assert.That(shader, Is.Not.Null);
                var first = new Material(shader);
                AssetDatabase.CreateAsset(first, folder + "/main.mat");
                AssetDatabase.AddObjectToAsset(new Material(shader), first);
                AssetDatabase.CreateAsset(new Material(shader), folder + "/Skip/skip.mat");
                AssetDatabase.CreateAsset(new Material(shader), folder + "/SkipMore/keep.mat");
                Assert.That(ShaderUsageReport.TryScan(folder, new[] { folder + "/Skip" }, out var usages), Is.True);
                Assert.That(usages.Count, Is.EqualTo(1));
                Assert.That(usages[0].Materials.Count, Is.EqualTo(3));
                Assert.That(ShaderUsageReport.TryScan(folder, new[] { folder + "/missing" }, out _), Is.False);
            }
            finally
            {
                AssetDatabase.DeleteAsset(folder);
            }
        }
    }
}
