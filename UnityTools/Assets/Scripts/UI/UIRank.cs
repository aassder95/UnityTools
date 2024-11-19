using UnityEngine;
using UnityTools.Manager;

namespace UnityTools.UI
{
    public class UIRank : MonoBehaviour
    {
        [SerializeField] UIRankScrollView _svRank;

        void Start()
        {
            _svRank.Init(RankManager.Instance.TotalCount);
        }

        public void UpdateView()
        {
            _svRank.UpdateView();
        }

        public void OnRadomScore()
        {
            RankManager.Instance.OnRadomScore();
        }
    }
}