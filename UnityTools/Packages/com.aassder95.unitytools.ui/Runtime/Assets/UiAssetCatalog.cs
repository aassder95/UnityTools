using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace UnityTools.Ui.Assets
{
    [CreateAssetMenu(menuName = "UnityTools/UI/Asset Catalog")]
    public class UiAssetCatalog : ScriptableObject
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private AssetEntry<GameObject>[] _prefabs = Array.Empty<AssetEntry<GameObject>>();
        [SerializeField] private AssetEntry<Sprite>[] _sprites = Array.Empty<AssetEntry<Sprite>>();
        [SerializeField] private AssetEntry<SpriteAtlas>[] _atlases = Array.Empty<AssetEntry<SpriteAtlas>>();

        //============================================================
        // Logic
        //============================================================
        public bool TryValidate(out string error)
        {
            return Validate(_prefabs, "Prefab", out error) && Validate(_sprites, "Sprite", out error) && Validate(_atlases, "Atlas", out error);
        }

        public bool TryGetPrefab(string key, out GameObject prefab)
        {
            return TryResolve(_prefabs, key, out prefab);
        }

        public bool TryGetSprite(string key, out Sprite sprite)
        {
            return TryResolve(_sprites, key, out sprite);
        }

        public bool TryGetAtlas(string key, out SpriteAtlas atlas)
        {
            return TryResolve(_atlases, key, out atlas);
        }

        //============================================================
        // Utilities
        //============================================================
        private static bool Validate<T>(AssetEntry<T>[] entries, string category, out string error) where T : UnityEngine.Object
        {
            error = null;
            if (entries == null)
            {
                error = category + " 항목 목록이 없습니다.";
                return false;
            }

            var keys = new HashSet<string>(StringComparer.Ordinal);
            for (int idx = 0; idx < entries.Length; idx++)
            {
                AssetEntry<T> entry = entries[idx];
                if (entry == null || string.IsNullOrWhiteSpace(entry.Key) || entry.Asset == null || !keys.Add(entry.Key))
                {
                    error = category + " 키 중복·빈 키·참조 누락: " + idx;
                    return false;
                }
            }

            return true;
        }

        private static bool TryResolve<T>(AssetEntry<T>[] entries, string key, out T asset) where T : UnityEngine.Object
        {
            asset = null;
            if (entries == null || string.IsNullOrWhiteSpace(key))
                return false;

            bool hasMatch = false;
            for (int idx = 0; idx < entries.Length; idx++)
            {
                AssetEntry<T> entry = entries[idx];
                if (entry == null || !string.Equals(entry.Key, key, StringComparison.Ordinal))
                    continue;

                if (hasMatch || entry.Asset == null)
                {
                    asset = null;
                    return false;
                }

                hasMatch = true;
                asset = entry.Asset;
            }

            return hasMatch;
        }
    }
}
