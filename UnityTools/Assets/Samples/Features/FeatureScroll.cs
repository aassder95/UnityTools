using UnityTools.Ui;

namespace UnityTools.Samples.Features
{
    public class FeatureScroll : DynamicScrollView<FeatureItem>
    {
        //============================================================
        // Fields
        //============================================================
        private float[] _heights;

        //============================================================
        // Properties
        //============================================================
        public int CreatedItemCnt => RtContent.childCount;

        //============================================================
        // Init/Register
        //============================================================
        public bool TryInit(float[] heights)
        {
            OnItemUpdated -= BindItem;
            _heights = heights;
            OnItemUpdated += BindItem;
            if (TryInitView(heights))
                return true;

            OnItemUpdated -= BindItem;
            return false;
        }

        public void Release()
        {
            OnItemUpdated -= BindItem;
            ReleaseView();
            _heights = null;
        }

        //============================================================
        // Callbacks
        //============================================================
        private void BindItem(FeatureItem item) => item.Bind(_heights[item.Idx]);
    }
}
