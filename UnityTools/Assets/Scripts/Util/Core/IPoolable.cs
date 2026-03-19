namespace UnityTools.Util
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
