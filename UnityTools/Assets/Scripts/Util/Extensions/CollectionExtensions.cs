using System.Collections.Generic;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Util.Extensions
{
    // Exception: stateless utility is kept as a static helper.
    //============================================================
    // Logic
    //============================================================
    public static class CollectionExtensions
    {
        //============================================================
        // Logic
        //============================================================
        public static bool IsValidIndex<T>(this List<T> list, int index)
        {
            if(list == null)
            {
                DebugLogger.LogWarning("리스트가 비어 있습니다.");
                return false;
            }

            if(0 > index || index >= list.Count)
            {
                DebugLogger.LogWarning("인덱스가 범위를 벗어났습니다.");
                return false;
            }

            return true;
        }
    }
}
