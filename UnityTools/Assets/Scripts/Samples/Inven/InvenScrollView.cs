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
        public void UpdateItemView()
        {
            if(ItemController == null)
                return;

            ItemController.Update();
        }
    }
}
