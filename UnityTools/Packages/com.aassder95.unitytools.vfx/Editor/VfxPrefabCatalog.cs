using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Vfx.Editor
{
    public class VfxPrefabCatalog
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly List<VfxPrefabInfo> _items = new List<VfxPrefabInfo>();

        //============================================================
        // Properties
        //============================================================
        public IReadOnlyList<VfxPrefabInfo> Items => _items;

        //============================================================
        // Logic
        //============================================================
        public bool TryRefresh(string rootPath)
        {
            if (string.IsNullOrEmpty(rootPath) || !AssetDatabase.IsValidFolder(rootPath))
                return false;

            List<VfxPrefabInfo> items = new List<VfxPrefabInfo>();
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { rootPath });
            for (int idx = 0; idx < guids.Length; idx++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[idx]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                    continue;

                ParticleSystem[] systems = prefab.GetComponentsInChildren<ParticleSystem>(true);
                if (systems.Length == 0)
                    continue;

                bool isLooping = false;
                for (int systemIdx = 0; systemIdx < systems.Length; systemIdx++)
                {
                    isLooping |= systems[systemIdx].main.loop;
                }

                items.Add(new VfxPrefabInfo(guids[idx], path, prefab.name, systems.Length, isLooping));
            }

            items.Sort((left, right) => StringComparer.OrdinalIgnoreCase.Compare(left.AssetPath, right.AssetPath));
            _items.Clear();
            _items.AddRange(items);
            return true;
        }

        public void Filter(string query, EVfxLoopFilter loopFilter, ISet<string> favoriteGuids, bool isFavoritesOnly, List<VfxPrefabInfo> results)
        {
            results.Clear();
            string[] tokens = (query ?? string.Empty).Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            for (int idx = 0; idx < _items.Count; idx++)
            {
                VfxPrefabInfo item = _items[idx];
                if ((loopFilter == EVfxLoopFilter.Looping && !item.IsLooping) || (loopFilter == EVfxLoopFilter.OneShot && item.IsLooping) || (isFavoritesOnly && !favoriteGuids.Contains(item.Guid)))
                    continue;

                bool isMatch = true;
                for (int tokenIdx = 0; tokenIdx < tokens.Length; tokenIdx++)
                {
                    if (item.AssetPath.IndexOf(tokens[tokenIdx], StringComparison.OrdinalIgnoreCase) < 0 && item.Name.IndexOf(tokens[tokenIdx], StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        isMatch = false;
                        break;
                    }
                }

                if (isMatch)
                    results.Add(item);
            }
        }
    }
}
