using UnityEngine;
using UnityTools.Model;
using UnityTools.Util;

namespace UnityTools.UI
{
    //============================================================
    //Logic
    //============================================================
    public class InvenView : BaseView<InvenModel>
    {
        [SerializeField] private InvenScrollView _scrollView;

        public InvenScrollView ScrollView => _scrollView;
        
        public override void Refresh(InvenModel model)
        {
            _scrollView.UpdateItemView();
        }
    }
}