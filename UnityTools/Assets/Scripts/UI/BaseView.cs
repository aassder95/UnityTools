namespace UnityTools.UI
{
    public interface IView<TModel>
    {
        void InitView(TModel model = default);
        void ShowView(TModel model = default);
        void HideView();
        void UpdateView(TModel model = default);
    }
}