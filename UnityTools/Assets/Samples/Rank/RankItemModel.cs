using UnityEngine;
using UnityTools.Ui;

namespace UnityTools.Samples.Rank
{
    public class RankItemModel : BaseModel
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly int _id;
        private readonly Color _bgColor;

        //============================================================
        // Fields
        //============================================================
        private int _rank;
        private int _score;

        //============================================================
        // Properties
        //============================================================
        public int Id => _id;
        public int Rank => _rank;
        public int Score => _score;
        public Color BgColor => _bgColor;

        //============================================================
        // Constructors
        //============================================================
        public RankItemModel(int id)
        {
            _id = id;
            _bgColor = Random.ColorHSV(0.0f, 1.0f, 0.5f, 1.0f, 0.7f, 1.0f);
        }

        //============================================================
        // Logic
        //============================================================
        public void SetRank(int rank)
        {
            SetField(ref _rank, rank);
        }

        public void SetScore(int score)
        {
            SetField(ref _score, score);
        }
    }
}
