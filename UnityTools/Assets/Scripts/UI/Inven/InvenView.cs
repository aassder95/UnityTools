using UnityEngine;

namespace UnityTools.UI
{
    public class InvenView : MonoBehaviour, IView<int>
    {
        [SerializeField] InvenScrollView _scrollView;

        public InvenScrollView ScrollView => _scrollView;

        public void InitView(int totalCnt)
        {
            _scrollView.InitView(totalCnt);
        }

        public void ShowView(int totalCnt) { }

        public void HideView() { }

        public void UpdateView(int totalCnt)
        {
            _scrollView.UpdateItemView();
        }
    }
}