namespace UnityTools.UI
{
    public interface IView<TModel>
    {
        void InitView(TModel model);
        void UpdateView(TModel model);
    }
}