using UnityEngine;
using UnityTools.Util;

namespace UnityTools.Samples.Rank
{
    public class RankItemModel : BaseModel
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly int _id;
        private readonly Color _bgColor;

        //============================================================
        //Fields
        //============================================================
        private int _rank;
        private int _score;

        //============================================================
        //Properties
        //============================================================
        public int Id => _id;
        public int Rank => _rank;
        public int Score => _score;
        public Color BgColor => _bgColor;

        //============================================================
        //Constructors
        //============================================================
        public RankItemModel(int id)
        {
            _id = id;
            _bgColor = RandomUtils.GetRandomColor();
            NotifyUpdated();
        }

        //============================================================
        //Logic
        //============================================================
        public void SetRank(int rank)
        {
            _rank = rank;
            NotifyUpdated();
        }

        public void SetScore(int score)
        {
            _score = score;
            NotifyUpdated();
        }
    }
}
