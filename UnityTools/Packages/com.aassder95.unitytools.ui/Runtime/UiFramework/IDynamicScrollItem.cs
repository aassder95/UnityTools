using UnityEngine;

namespace UnityTools.Util.UiFramework
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
        void Init();
        void SetIdx(int idx);
        void SetPos(Vector2 pos);
    }
}
