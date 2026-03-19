using UnityEngine;

namespace UnityTools.Util.UIFramework
{
    public interface IDynamicScrollItem
    {
        //============================================================
        //Logic
        //============================================================
        int GetIndex();
        void SetIndex(int index);
        void SetPosition(Vector2 pos);
    }
}
