using UnityEngine;

namespace UnityTools.Ui
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
        void OnGet();
        void OnReturn();
        void SetIdx(int idx);
        void SetPos(Vector2 pos);
    }
}
