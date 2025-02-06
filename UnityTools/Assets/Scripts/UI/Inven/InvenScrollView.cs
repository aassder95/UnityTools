using UnityTools.Util;

namespace UnityTools.UI
{
    public class InvenScrollView : DynamicScrollView<InvenItemView>
    {
        public void UpdateItemView() => ItemController.Update();
    }
}