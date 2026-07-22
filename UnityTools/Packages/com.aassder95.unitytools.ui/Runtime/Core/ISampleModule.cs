
namespace UnityTools.Util.Core
{
    public interface ISampleModule
    {
        //============================================================
        // Properties
        //============================================================
        string ModuleKey { get; }
        bool IsInit { get; }

        //============================================================
        // Logic
        //============================================================
        void Init();
        void Show();
        void Hide();
        void Release();
    }
}
