using UnityEngine;
using UnityTools.Util;

namespace UnityTools.Samples.Inven
{
    public class InvenView : BaseView<InvenModel>
    {
        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private InvenScrollView _scrollView;

        //============================================================
        //Properties
        //============================================================
        public InvenScrollView ScrollView => _scrollView;

        //============================================================
        //Logic
        //============================================================
        public override void Refresh(InvenModel model)
        {
            if(model == null || _scrollView == null)
                return;

            _scrollView.UpdateItemView();
        }
    }
}
