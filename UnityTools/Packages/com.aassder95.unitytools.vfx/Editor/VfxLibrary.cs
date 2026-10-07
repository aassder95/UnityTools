using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Vfx.Editor
{
    public class VfxLibrary : ScriptableObject
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private List<VfxLabels> _entries = new List<VfxLabels>();

        //============================================================
        // Logic
        //============================================================
        public VfxLabels FindLabels(string guid)
        {
            for (int idx = 0; idx < _entries.Count; idx++)
            {
                if (_entries[idx].Guid == guid)
                    return _entries[idx];
            }

            return null;
        }

        public void SetLabels(string guid, string tags, string collections)
        {
            if (string.IsNullOrEmpty(guid) || guid.Length != 32)
                return;

            VfxLabels entry = new VfxLabels(guid, tags, collections);
            for (int idx = 0; idx < _entries.Count; idx++)
            {
                if (_entries[idx].Guid == guid)
                {
                    _entries[idx] = entry;
                    return;
                }
            }

            _entries.Add(entry);
        }

        public bool Matches(string guid, string tag, string collection)
        {
            VfxLabels entry = FindLabels(guid);
            return entry != null ? entry.Matches(tag, collection) : string.IsNullOrWhiteSpace(tag) && string.IsNullOrWhiteSpace(collection);
        }
    }
}
