using UnityEngine;

namespace UnityTools.Model
{
    public interface IRankElementModel { }

    public class RankItemModel : IRankElementModel
    {
        int _id;
        int _rank;
        int _score;
        Color _bgColor;

        public int Id => _id;
        public int Rank => _rank;
        public int Score => _score;
        public Color BgColor => _bgColor;

        public RankItemModel(int id)
        {
            _id = id;
            _bgColor = new(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
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

    public class RankDividerModel : IRankElementModel { }
}
