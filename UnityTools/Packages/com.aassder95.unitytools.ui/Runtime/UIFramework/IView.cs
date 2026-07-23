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
        bool Init();
        bool Release();

        //============================================================
        // Logic
        //============================================================
        bool Show();
        bool Hide();
        bool Refresh(TModel model);
    }
}
