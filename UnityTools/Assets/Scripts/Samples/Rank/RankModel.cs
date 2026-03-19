using System.Collections.Generic;
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
    public class RankModel : BaseModel
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly List<RankItemModel> _itemModels = new();

        //============================================================
        //Properties
        //============================================================
        public int ItemCount => _itemModels.Count;

        //============================================================
        //Constructors
        //============================================================
        public RankModel(int cnt)
        {
            for (int i = 0; i < cnt; i++)
            {
                _itemModels.Add(new(i));
            }

            SetRandomScore();
        }

        //============================================================
        //Logic
        //============================================================
        public RankItemModel Get(int idx)
        {
            return _itemModels.IsValidIndex(idx) ? _itemModels[idx] : null;
        }

        public void SetRandomScore()
        {
            foreach (RankItemModel model in _itemModels)
            {
                model.SetScore(Random.Range(1, 101));
            }

            _itemModels.Sort((a, b) => b.Score.CompareTo(a.Score));

            for (int i = 0; i < _itemModels.Count; i++)
            {
                _itemModels[i].SetRank(i + 1);
            }
            
            NotifyUpdated();
        }
    }
}
