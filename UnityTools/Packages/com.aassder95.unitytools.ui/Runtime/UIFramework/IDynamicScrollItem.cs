using UnityEngine;

namespace UnityTools.Util.UIFramework
{
    public interface IDynamicScrollItem
    {
        //============================================================
        // Properties
        //============================================================
        int Idx { get; }

        //============================================================
        // Logic
        //============================================================
        bool Init();
        void SetIdx(int idx);
        void SetPos(Vector2 pos);
    }
}
