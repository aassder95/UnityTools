using UnityTools.Util;

namespace UnityTools.UI
{
    //============================================================
    //Logic
    //============================================================
    public class InvenScrollView : DynamicScrollView<InvenItemView>
    {
        public void UpdateItemView() => ItemController.Update();
    }
}