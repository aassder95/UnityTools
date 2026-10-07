using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityTools.Ui.Assets;

namespace UnityTools.Ui.Editor.Tests
{
    public class UiAssetCatalogSerializationTests
    {
        //============================================================
        // Logic
        //============================================================
        [Test]
        public void InspectorEntriesPersistAsTypedPrefabReferences()
        {
            string folder = "Assets/Catalog_" + System.Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", folder.Substring(7));
            var go = new GameObject("Prefab");
            UiAssetCatalog catalog = null;
            try
            {
                var prefab = PrefabUtility.SaveAsPrefabAsset(go, folder + "/prefab.prefab");
                catalog = ScriptableObject.CreateInstance<UiAssetCatalog>();
                AssetDatabase.CreateAsset(catalog, folder + "/catalog.asset");
                var data = new SerializedObject(catalog);
                var entries = data.FindProperty("_prefabs");
                entries.arraySize = 1;
                var entry = entries.GetArrayElementAtIndex(0);
                entry.FindPropertyRelative("_key").stringValue = "screen";
                entry.FindPropertyRelative("_asset").objectReferenceValue = prefab;
                data.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssets();
                Assert.That(catalog.TryValidate(out _), Is.True);
                Assert.That(catalog.TryGetPrefab("screen", out var result), Is.True);
                Assert.That(result, Is.SameAs(prefab));
                string yaml = System.IO.File.ReadAllText(folder + "/catalog.asset");
                Assert.That(yaml, Does.Contain("_key: screen"));
                Assert.That(yaml, Does.Contain(AssetDatabase.AssetPathToGUID(folder + "/prefab.prefab")));
            }
            finally
            {
                Object.DestroyImmediate(go);
                AssetDatabase.DeleteAsset(folder);
            }
        }
    }
}
