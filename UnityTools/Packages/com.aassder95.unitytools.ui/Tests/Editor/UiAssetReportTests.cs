using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace UnityTools.Ui.Editor.Tests
{
    public class UiAssetReportTests
    {
        //============================================================
        // Fields
        //============================================================
        private string _folder;

        //============================================================
        // Init/Register
        //============================================================
        [SetUp]
        public void SetUp()
        {
            _folder = "Assets/UiReport_" + System.Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", _folder.Substring(7));
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(_folder);
        }

        //============================================================
        // Logic
        //============================================================
        [Test]
        public void FindsInactiveImagesAndExactSpriteAtlasMembership()
        {
            var texture = new Texture2D(8, 8);
            AssetDatabase.CreateAsset(texture, _folder + "/texture.asset");
            Sprite first = Sprite.Create(texture, new Rect(0, 0, 4, 4), Vector2.zero);
            Sprite second = Sprite.Create(texture, new Rect(4, 0, 4, 4), Vector2.zero);
            AssetDatabase.AddObjectToAsset(first, texture);
            AssetDatabase.AddObjectToAsset(second, texture);
            var atlas = new SpriteAtlas();
            atlas.Add(new UnityEngine.Object[] { first });
            AssetDatabase.CreateAsset(atlas, _folder + "/atlas.spriteatlas");
            var go = new GameObject("UI", typeof(RectTransform), typeof(Image));
            try
            {
                go.GetComponent<Image>().sprite = first;
                go.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(go, _folder + "/first.prefab");
                go.GetComponent<Image>().sprite = second;
                PrefabUtility.SaveAsPrefabAsset(go, _folder + "/second.prefab");
                go.GetComponent<Image>().sprite = null;
                PrefabUtility.SaveAsPrefabAsset(go, _folder + "/missing.prefab");
                Assert.That(UiAssetReport.TryScan(_folder, out var usages), Is.True);
                Assert.That(usages.Count, Is.EqualTo(3));
                Assert.That(usages.Single(x => x.PrefabPath.EndsWith("/first.prefab")).Atlases, Does.Contain(atlas));
                Assert.That(usages.Single(x => x.PrefabPath.EndsWith("/second.prefab")).HasAtlas, Is.False);
                Assert.That(usages.Single(x => x.PrefabPath.EndsWith("/missing.prefab")).HasSprite, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void InvalidFolderDoesNotReturnPartialReport()
        {
            Assert.That(UiAssetReport.TryScan(_folder + "/absent", out var usages), Is.False);
            Assert.That(usages, Is.Null);
        }
    }
}
