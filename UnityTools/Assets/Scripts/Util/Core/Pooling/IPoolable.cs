namespace UnityTools.Util.Core.Pooling
{
    public interface IPoolable
    {
        //============================================================
        //Callbacks
        //============================================================
        void OnGet();
        void OnReturn();
    }
}
