namespace UnityTools.Samples.Modules
{
    public interface ISampleModule
    {
        string ModuleKey { get; }
        bool IsInitialized { get; }
        void Init();
        void Show();
        void Release();
    }
}
