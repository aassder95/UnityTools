using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Vfx.Editor
{
    [Serializable]
    public class VfxLabels
    {
        //============================================================
        // Fields
        //============================================================
        [SerializeField] private string _guid;
        [SerializeField] private string[] _tags;
        [SerializeField] private string[] _collections;

        //============================================================
        // Properties
        //============================================================
        public string Guid => _guid;
        public string TagsText => string.Join(", ", _tags);
        public string CollectionsText => string.Join(", ", _collections);

        //============================================================
        // Constructors
        //============================================================
        public VfxLabels(string guid, string tags, string collections)
        {
            _guid = guid;
            _tags = ParseLabels(tags);
            _collections = ParseLabels(collections);
        }

        //============================================================
        // Logic
        //============================================================
        public bool Matches(string tag, string collection)
        {
            return Contains(_tags, tag) && Contains(_collections, collection);
        }

        //============================================================
        // Utilities
        //============================================================
        private static string[] ParseLabels(string text)
        {
            List<string> labels = new List<string>();
            HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string[] parts = (text ?? string.Empty).Split(',');
            for (int idx = 0; idx < parts.Length; idx++)
            {
                string name = parts[idx].Trim();
                if (name.Length > 0 && names.Add(name))
                    labels.Add(name);
            }

            return labels.ToArray();
        }

        private static bool Contains(string[] labels, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return true;

            for (int idx = 0; idx < labels.Length; idx++)
            {
                if (string.Equals(labels[idx], query.Trim(), StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
