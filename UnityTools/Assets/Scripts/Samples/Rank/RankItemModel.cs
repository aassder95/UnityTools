using UnityEngine;
using UnityTools.Util;
using UnityTools.Util.Constants;
using UnityTools.Util.Core.Collections;
using UnityTools.Util.Core.Events;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Core.Persistence;
using UnityTools.Util.Core.Pooling;
using UnityTools.Util.Core.Singleton;
using UnityTools.Util.Core.State;
using UnityTools.Util.Core.Timer.Period;
using UnityTools.Util.Core.Timer.Shared;
using UnityTools.Util.Core.Timer.Task;
using UnityTools.Util.Coroutines;
using UnityTools.Util.Extensions;
using UnityTools.Util.UIFramework;
using UnityTools.Util.Utilities;

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
        }

        //============================================================
        //Logic
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
