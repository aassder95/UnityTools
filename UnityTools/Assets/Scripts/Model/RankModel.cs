using System.Collections.Generic;
using UnityTools.Util;

namespace UnityTools.Model
{
    public class RankModel
    {
        List<RankItemModel> _models = new();

        public int Count => _models.Count;

        public RankModel(int cnt)
        {
            for (int i = 0; i < cnt; i++)
            {
                _models.Add(new RankItemModel(i));
            }
        }

        public void Update(int cnt)
        {
            int diff = cnt - _models.Count;
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
                _models.Add(new RankItemModel(_models.Count + i));
            }
        }

        void Remove(int cnt = 1)
        {
            int lastIdx = _models.Count - 1;
            for (int i = lastIdx; i >= lastIdx - cnt; i--)
            {
                _models.RemoveAt(i);
            }
        }

        public RankItemModel Get(int idx)
        {
            if (!_models.IsValidIndex(idx))
                return null;

            return _models[idx];
        }

        public void SetRandomScore()
        {
            foreach (RankItemModel model in _models)
            {
                model.SetRandomScore();
            }

            _models.Sort((a, b) => b.Score.CompareTo(a.Score));

            for (int i = 0; i < _models.Count; i++)
            {
                _models[i].SetRank(i + 1);
            }
        }
    }
}
