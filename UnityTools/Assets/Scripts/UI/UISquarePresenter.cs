using UnityTools.Util;

namespace UnityTools.UI
{
    public class UISquarePresenter : UIPresenter<UISquareModel, UISquareView>, IDynamicScrollViewChild
    {
        public void SetIndex(int idx)
        {
            View.SetIndex(idx);
            Model.SetIndex(idx);
        }

        public void SetPositionY(float y)
        {
            View.SetPositionY(y);
        }
    }
}