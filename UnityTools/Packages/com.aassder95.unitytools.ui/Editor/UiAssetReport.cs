using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace UnityTools.Ui.Editor
{
    public static class UiAssetReport
    {
        //============================================================
        // Logic
        //============================================================
        public static bool TryScan(string folder, out IReadOnlyList<UiImageUsage> usages)
        {
            usages = null;
            if (string.IsNullOrEmpty(folder) || !AssetDatabase.IsValidFolder(folder))
                return false;

            var atlases = new List<SpriteAtlas>();
            var packables = new List<UnityEngine.Object[]>();
            foreach (string guid in AssetDatabase.FindAssets("t:SpriteAtlas"))
            {
                SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AssetDatabase.GUIDToAssetPath(guid));
                if (atlas == null)
                    continue;

                atlases.Add(atlas);
                packables.Add(atlas.GetPackables());
            }

            var results = new List<UiImageUsage>();
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
            System.Array.Sort(prefabGuids, System.StringComparer.Ordinal);
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                foreach (Image image in prefab.GetComponentsInChildren<Image>(true))
                {
                    Sprite sprite = image.overrideSprite;
                    var matches = new List<SpriteAtlas>();
                    if (sprite != null)
                    {
                        string spritePath = AssetDatabase.GetAssetPath(sprite);
                        for (int idx = 0; idx < atlases.Count; idx++)
                        {
                            if (Contains(sprite, spritePath, packables[idx]))
                                matches.Add(atlases[idx]);
                        }
                    }

                    string imagePath = AnimationUtility.CalculateTransformPath(image.transform, prefab.transform);
                    results.Add(new UiImageUsage(path, imagePath, sprite, image.material, matches.AsReadOnly()));
                }
            }

            usages = results.AsReadOnly();
            return true;
        }

        //============================================================
        // Utilities
        //============================================================
        private static bool Contains(Sprite sprite, string spritePath, UnityEngine.Object[] packables)
        {
            foreach (UnityEngine.Object packable in packables)
            {
                if (packable == sprite)
                    return true;

                string path = AssetDatabase.GetAssetPath(packable);
                if (packable is Texture2D && path == spritePath)
                    return true;

                if (AssetDatabase.IsValidFolder(path) && spritePath.StartsWith(path + "/", System.StringComparison.Ordinal))
                    return true;
            }

            return false;
        }
    }
}
