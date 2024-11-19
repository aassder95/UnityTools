using System.Collections.Generic;

namespace UnityTools.Model
{
    public class RankModel
    {
        List<RankItemModel> _itemModels = new();

        public List<RankItemModel> ItemModels => _itemModels;

        public RankModel(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _itemModels.Add(new RankItemModel(i));
            }
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
