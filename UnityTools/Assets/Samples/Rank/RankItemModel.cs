using UnityEngine;
using UnityTools.Util.UiFramework;
using UnityTools.Util.Utilities;

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
            _bgColor = RandomUtils.GetRandomColor();
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
