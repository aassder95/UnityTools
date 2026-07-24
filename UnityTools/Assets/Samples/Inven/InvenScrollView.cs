using UnityTools.Ui;

namespace UnityTools.Samples.Inven
{
    public class InvenScrollView : DynamicScrollView<InvenItemView>
    {
        //============================================================
        // Logic
        //============================================================
        public void RefreshItems()
        {
            ItemController.UpdateItems();
        }
    }
}
