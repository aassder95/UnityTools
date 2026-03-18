using System.Collections.Generic;

namespace UnityTools.Util
{
    //============================================================
    //Logic
    //============================================================
    public static class CollectionExtensions
    {
        //============================================================
        //Logic
        //============================================================
        public static bool IsValidIndex<T>(this List<T> list, int index)
        {
            if(list == null)
            {
                DebugLogger.LogWarning("List가 null입니다.");
                return false;
            }

            if(0 > index || index >= list.Count)
            {
                DebugLogger.LogWarning("Index가 범위를 벗어났습니다.");
                return false;
            }

            return true;
        }
    }
}
