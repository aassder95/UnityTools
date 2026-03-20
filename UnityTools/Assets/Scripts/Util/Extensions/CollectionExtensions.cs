using System.Collections.Generic;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Util.Extensions
{
    // Exception: stateless utility is kept as a static helper.
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
                DebugLogger.LogWarning("由ъ뒪?멸? 鍮꾩뼱 ?덉뒿?덈떎.");
                return false;
            }

            if(0 > index || index >= list.Count)
            {
                DebugLogger.LogWarning("?몃뜳?ㅺ? 踰붿쐞瑜?踰쀬뼱?ъ뒿?덈떎.");
                return false;
            }

            return true;
        }
    }
}
