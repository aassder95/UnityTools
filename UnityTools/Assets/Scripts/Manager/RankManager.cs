using System.Collections.Generic;
using UnityTools.Model;
using UnityTools.Util;
using UnityEngine;
using UnityTools.UI;

namespace UnityTools.Manager
{
    public class RankManager : Singleton<RankManager>
    {
        [SerializeField] UIRank _view;

        List<RankItemModel> _models = new();

        public int TotalCount => _models.Count;

        void Awake()
        {
            for (int i = 0; i < 10; i++)
            {
                RankItemModel model = new RankItemModel();
                model.Id = i;
                _models.Add(model);
            }
        }

        public RankItemModel GetModel(int idx)
        {
            return _models[idx];
        }

        public void OnRadomScore()
        {
            for (int i = 0; i < _models.Count; i++)
            {
                _models[i].Score = Random.Range(1, 100);
            }

            _models.Sort((a, b) => b.Score.CompareTo(a.Score));

            for (int i = 0; i < _models.Count; i++)
            {
                _models[i].Rank = i + 1;
            }

            _view.UpdateView();
        }
    }
}