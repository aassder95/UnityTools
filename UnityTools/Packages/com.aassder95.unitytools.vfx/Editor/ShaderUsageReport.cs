using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Vfx.Editor
{
    public static class ShaderUsageReport
    {
        //============================================================
        // Logic
        //============================================================
        public static bool TryScan(string folder, IReadOnlyList<string> excludedFolders, out IReadOnlyList<ShaderUsage> usages)
        {
            usages = null;
            if (string.IsNullOrEmpty(folder) || !AssetDatabase.IsValidFolder(folder))
                return false;

            if (excludedFolders != null)
            {
                for (int idx = 0; idx < excludedFolders.Count; idx++)
                {
                    if (string.IsNullOrEmpty(excludedFolders[idx]) || !AssetDatabase.IsValidFolder(excludedFolders[idx]))
                        return false;
                }
            }

            var materials = new Dictionary<Shader, List<Material>>();
            string[] guids = AssetDatabase.FindAssets("t:Material", new[] { folder });
            Array.Sort(guids, StringComparer.Ordinal);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (IsExcluded(path, excludedFolders))
                    continue;

                // 하나의 FBX 등에 포함된 여러 Material도 각각 집계합니다.
                foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (!(asset is Material material) || material.shader == null)
                        continue;

                    if (!materials.TryGetValue(material.shader, out List<Material> group))
                    {
                        group = new List<Material>();
                        materials.Add(material.shader, group);
                    }

                    if (!group.Contains(material))
                        group.Add(material);
                }
            }

            var results = new List<ShaderUsage>();
            foreach (KeyValuePair<Shader, List<Material>> pair in materials)
            {
                pair.Value.Sort((left, right) => string.CompareOrdinal(AssetDatabase.GetAssetPath(left), AssetDatabase.GetAssetPath(right)));
                results.Add(new ShaderUsage(pair.Key, pair.Value.AsReadOnly()));
            }

            results.Sort((left, right) => string.CompareOrdinal(left.Shader.name, right.Shader.name));
            usages = results.AsReadOnly();
            return true;
        }

        //============================================================
        // Utilities
        //============================================================
        private static bool IsExcluded(string path, IReadOnlyList<string> folders)
        {
            if (folders == null)
                return false;

            for (int idx = 0; idx < folders.Count; idx++)
            {
                string folder = folders[idx].TrimEnd('/');
                if (path.StartsWith(folder + "/", StringComparison.Ordinal))
                    return true;
            }

            return false;
        }
    }
}
