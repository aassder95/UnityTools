namespace UnityTools.Util.UIFramework
{
    public interface IView<TModel> where TModel : IModel
    {
        //============================================================
        // Properties
        //============================================================
        bool IsInit { get; }
        bool IsVisible { get; }

        //============================================================
        // Init/Register
        //============================================================
        bool TryInit();
        bool TryRelease();

        //============================================================
        // Logic
        //============================================================
        bool TryShow();
        bool TryHide();
        bool TryRefresh(TModel model);
    }
}
