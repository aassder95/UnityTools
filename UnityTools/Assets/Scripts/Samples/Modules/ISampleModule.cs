namespace UnityTools.Samples.Modules
{
    public interface ISampleModule
    {
        string ModuleKey { get; }
        bool IsInit { get; }
        void Init();
        void Show();
        void Release();
    }
}
