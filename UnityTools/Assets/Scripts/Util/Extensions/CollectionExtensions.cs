using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Util
{
    //============================================================
    //Logic
    //============================================================
    public static class CollectionExtensions
    {
        public static bool IsValidIndex<T>(this List<T> list, int index)
        {
            if(list == null)
            {
                Debug.LogWarning("[CollectionExtensions:IsValidIndex] List가 null입니다.");
                return false;
            }

            if(0 > index || index >= list.Count)
            {
                Debug.LogWarning("[CollectionExtensions:IsValidIndex] Index가 범위를 벗어났습니다.");
                return false;
            }

            return true;
        }
    }
}
