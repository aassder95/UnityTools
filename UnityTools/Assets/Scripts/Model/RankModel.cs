using System.Collections.Generic;
using UnityTools.Util;

namespace UnityTools.Model
{
    public class RankModel
    {
        List<RankItemModel> _itemModels = new();

        public int Count => _itemModels.Count;
        public List<RankItemModel> ItemModels => _itemModels;

        public RankModel(int cnt)
        {
            for (int i = 0; i < cnt; i++)
            {
                _itemModels.Add(new(i));
            }

            SetRandomScore();
        }

        public void Update(int cnt)
        {
            int diff = cnt - _itemModels.Count;
            if (diff == 0)
                return;

            if (diff > 0)
                Add(diff);
            else
                Remove(-diff);
        }

        void Add(int cnt = 1)
        {
            for (int i = 0; i < cnt; i++)
            {
                _itemModels.Add(new(_itemModels.Count + i));
            }
        }

        void Remove(int cnt = 1)
        {
            int lastIdx = _itemModels.Count - 1;
            for (int i = lastIdx; i >= lastIdx - cnt; i--)
            {
                _itemModels.RemoveAt(i);
            }
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
                model.SetRandomScore();
            }

            _itemModels.Sort((a, b) => b.Score.CompareTo(a.Score));

            for (int i = 0; i < _itemModels.Count; i++)
            {
                _itemModels[i].SetRank(i + 1);
            }
        }
    }
}
