using System.Collections.Generic;
using UnityEngine;
using UnityTools.Util;

namespace UnityTools.Model
{
    public class RankModel
    {
        List<RankItemModel> _itemModels = new();

        public List<RankItemModel> ItemModels => _itemModels;

        public RankModel(int cnt)
        {
            for (int i = 0; i < cnt; i++)
            {
                _itemModels.Add(new(i));
            }

            SetRandomScore();
        }

        public RankItemModel Get(int idx)
        {
            if (!_itemModels.IsValidIndex(idx))
                return null;

            return _itemModels[idx];
        }

        public void SetRandomScore()
        {
            foreach (RankItemModel model in _itemModels)
            {
                model.SetScore(Random.Range(1, 101));
            }

            _itemModels.Sort((a, b) => b.Score.CompareTo(a.Score));

            for (int i = 0; i < _itemModels.Count; i++)
            {
                _itemModels[i].SetRank(i + 1);
            }
        }
    }
}
