using System;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Vfx.Editor.Tests
{
    public class VfxLibraryTests
    {
        //============================================================
        // Logic
        //============================================================
        [Test]
        public void LibrarySurvivesAssetReloadAndPrefabRenameWithoutChangingPrefab()
        {
            string folder = "Assets/LibraryTests-" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", Path.GetFileName(folder));
            GameObject go = new GameObject("Effect");
            try
            {
                string path = folder + "/Effect.prefab";
                PrefabUtility.SaveAsPrefabAsset(go, path);
                byte[] bytes = File.ReadAllBytes(path);
                string guid = AssetDatabase.AssetPathToGUID(path);
                VfxLibrary library = ScriptableObject.CreateInstance<VfxLibrary>();
                library.SetLabels(guid, " 보상, Reward, reward, ", "Stage1, 이벤트");
                AssetDatabase.CreateAsset(library, folder + "/Library.asset");
                AssetDatabase.SaveAssets();
                Assert.That(File.ReadAllBytes(path), Is.EqualTo(bytes));
                Assert.That(AssetDatabase.MoveAsset(path, folder + "/Moved.prefab"), Is.Empty);
                AssetDatabase.ImportAsset(folder + "/Library.asset", ImportAssetOptions.ForceUpdate);
                library = AssetDatabase.LoadAssetAtPath<VfxLibrary>(folder + "/Library.asset");
                Assert.That(library.Matches(AssetDatabase.AssetPathToGUID(folder + "/Moved.prefab"), "reward", "이벤트"), Is.True);
                Assert.That(library.FindLabels(guid).TagsText, Is.EqualTo("보상, Reward"));
                Assert.That(library.Matches(guid, "피격", ""), Is.False);
                library.SetLabels(guid, "피격", "Combat");
                Assert.That(library.Matches(guid, "보상", ""), Is.False);
                Assert.That(library.Matches(guid, "피격", "combat"), Is.True);
                Assert.That(library.Matches("missing", "", ""), Is.True);
                Assert.That(library.Matches("missing", "피격", ""), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                AssetDatabase.DeleteAsset(folder);
            }
        }
    }
}
