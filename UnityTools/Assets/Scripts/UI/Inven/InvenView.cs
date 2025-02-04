using UnityEngine;

namespace UnityTools.UI
{
    public class InvenView : MonoBehaviour
    {
        [SerializeField] InvenScrollView _scrollView;

        public InvenScrollView ScrollView => _scrollView;

        public void InitView(int totalCnt)
        {
            _scrollView.InitView(totalCnt);
        }

        public void UpdateView()
        {
            _scrollView.UpdateView();
        }
    }
}