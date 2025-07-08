using UnityEngine;
using UnityTools.Util;

namespace UnityTools.Model
{
    public class RankItemModel : BaseModel
    {
        public int Id { get; private set; }
        public int Rank { get; private set; }
        public int Score { get; private set; }
        public Color BgColor { get; private set; }

        public RankItemModel(int id)
        {
            Id = id;
            BgColor = RandomUtils.GetRandomColor();
            NotifyUpdated();
        }

        public void SetRank(int rank)
        {
            Rank = rank;
            NotifyUpdated();
        }

        public void SetScore(int score)
        {
            Score = score;
            NotifyUpdated();
        }
    }
}
