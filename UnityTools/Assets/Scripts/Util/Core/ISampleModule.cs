
namespace UnityTools.Util.Core
{
    // Exception: interface-centric file uses Types section.
    //============================================================
    //Types
    //============================================================
    public interface ISampleModule
    {
        //============================================================
        //Properties
        //============================================================
        string ModuleKey { get; }
        bool IsInit { get; }

        //============================================================
        //Logic
        //============================================================
        void Init();
        void Show();
        void Hide();
        void Release();
    }
}
