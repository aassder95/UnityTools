using UnityTools.Util;

namespace UnityTools.UI
{
    //============================================================
    //Logic
    //============================================================
    public class InvenScrollView : DynamicScrollView<InvenItemView>
    {
        //============================================================
        //Logic
        //============================================================
        public void UpdateItemView()
        {
            if(ItemController == null)
                return;

            ItemController.Update();
        }
    }
}
