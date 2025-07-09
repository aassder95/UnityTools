using UnityEngine;

namespace UnityTools.Util
{
    public interface IView<TModel> where TModel : BaseModel
    {
        void Init();
        void Release();
        void Show();
        void Hide();
        void Refresh(TModel model);
    }

    public abstract class BaseView<TModel> : MonoBehaviour, IView<TModel> where TModel : BaseModel
    {
        public bool IsVisible => gameObject.activeSelf;
        
        public virtual void Init()
        {
            
        }

        public virtual void Release()
        {
            
        }
        
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        public virtual void Refresh(TModel model)
        {
            if (model == null)
                return;
        }
    }
}