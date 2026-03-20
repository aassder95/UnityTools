
using UnityTools.Util.UIFramework;

namespace UnityTools.Samples.Inven
{
    //============================================================
    //Logic
    //============================================================
    public class InvenScrollView : DynamicScrollView<InvenItemView>
    {
        //============================================================
        //Logic
        //============================================================
        public void RefreshItems()
        {
            if(ItemController == null)
                return;

            ItemController.Update();
        }

        public void UpdateItemView()
        {
            RefreshItems();
        }
    }
}
