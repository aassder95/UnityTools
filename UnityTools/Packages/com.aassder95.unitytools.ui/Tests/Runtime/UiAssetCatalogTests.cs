using NUnit.Framework;
using UnityEngine;
using UnityTools.Ui.Assets;

namespace UnityTools.Ui.Tests
{
    public class UiAssetCatalogTests
    {
        //============================================================
        // Logic
        //============================================================
        [Test]
        public void DuplicateKeysFailWithoutReturningAnArbitraryAsset()
        {
            var catalog = ScriptableObject.CreateInstance<UiAssetCatalog>();
            var go = new GameObject("Entry");
            try
            {
                var field = typeof(UiAssetCatalog).GetField("_prefabs", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                field.SetValue(catalog, new[] { new AssetEntry<GameObject>("key", go) });
                Assert.That(catalog.TryValidate(out _), Is.True);
                Assert.That(catalog.TryGetPrefab("key", out var result), Is.True);
                Assert.That(result, Is.SameAs(go));
                Assert.That(catalog.TryGetPrefab("KEY", out _), Is.False);
                field.SetValue(catalog, new[] { new AssetEntry<GameObject>("key", go), new AssetEntry<GameObject>("key", go) });
                Assert.That(catalog.TryValidate(out _), Is.False);
                Assert.That(catalog.TryGetPrefab("key", out result), Is.False);
                Assert.That(result, Is.Null);
                field.SetValue(catalog, new[] { new AssetEntry<GameObject>("key", null) });
                Assert.That(catalog.TryValidate(out _), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(catalog);
                Object.DestroyImmediate(go);
            }
        }
    }
}
