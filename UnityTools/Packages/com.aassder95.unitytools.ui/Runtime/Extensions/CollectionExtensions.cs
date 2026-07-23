using System.Collections.Generic;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Util.Extensions
{
    //============================================================
    // Logic
    //============================================================
    public static class CollectionExtensions
    {
        //============================================================
        // Logic
        //============================================================
        public static bool IsValidIdx<T>(this List<T> list, int idx)
        {
            if(list == null)
            {
                DebugLogger.LogError("인덱스를 확인할 리스트가 비어 있습니다.");
                return false;
            }

            if(0 > idx || idx >= list.Count)
            {
                DebugLogger.LogError("인덱스가 리스트 범위를 벗어났습니다. 인덱스=" + idx + ", 개수=" + list.Count);
                return false;
            }

            return true;
        }
    }
}
