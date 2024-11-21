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
                Debug.LogWarning("[Utils:IsValidIndex] List is null");
                return false;
            }

            return 0 <= index && index < list.Count;
        }
    }
}