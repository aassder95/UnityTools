using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Util
{
    public static class CollectionExtensions
    {
        public static bool IsValidIndex<T>(this List<T> list, int index)
        {
            if (list == null)
            {
                Debug.LogWarning("[CollectionExtensions:IsValidIndex] List is null");
                return false;
            }

            if (0 > index || index >= list.Count)
            {
                Debug.LogWarning("[CollectionExtensions:IsValidIndex] Index out of range");
                return false;
            }

            return true;
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys, Func<TKey, TValue> valueSelector)
        {
            foreach (var key in keys)
            {
                dictionary[key] = valueSelector(key);
            }
        }
    }
}