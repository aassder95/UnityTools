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
        void Init();
        void Release();

        //============================================================
        // Logic
        //============================================================
        void Show();
        void Hide();
        void Refresh(TModel model);
    }
}
