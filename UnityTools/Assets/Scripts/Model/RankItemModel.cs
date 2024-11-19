using UnityEngine;

namespace UnityTools.Model
{
    public class RankItemModel
    {
        int _id;
        int _rank;
        int _score;

        public int Id => _id;
        public int Rank => _rank;
        public int Score => _score;

        public RankItemModel(int id)
        {
            _id = id;
        }

        public void SetRank(int rank)
        {
            _rank = rank;
        }

        public void SetScore(int score)
        {
            _score = score;
        }

        public void SetRandomScore()
        {
            SetScore(Random.Range(1, 101));
        }
    }
}
