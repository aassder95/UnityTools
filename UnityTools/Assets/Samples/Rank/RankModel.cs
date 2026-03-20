using System.Collections.Generic;
using UnityEngine;
using UnityTools.Util.Extensions;
using UnityTools.Util.UIFramework;

namespace UnityTools.Samples.Rank
{
    public class RankModel : BaseModel
    {
        //============================================================
        //Constants
        //============================================================
        private const int SCORE_MIN = 1;
        private const int SCORE_MAX = 5001;

        //============================================================
        //Readonly
        //============================================================
        private readonly List<RankItemModel> _itemModels = new();

        //============================================================
        //Fields
        //============================================================
        private int _nextItemId;

        //============================================================
        //Properties
        //============================================================
        public int ItemCount => _itemModels.Count;

        //============================================================
        //Constructors
        //============================================================
        public RankModel(int cnt)
        {
            for(int i = 0; i < cnt; i++)
                _itemModels.Add(new RankItemModel(i));

            _nextItemId = cnt;
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
            for(int i = 0; i < _itemModels.Count; i++)
                _itemModels[i].SetScore(Random.Range(SCORE_MIN, SCORE_MAX));

            SortByScoreAndUpdateRank();
            NotifyUpdated();
        }

        public void BoostTopScores()
        {
            int topCount = Mathf.Min(3, _itemModels.Count);
            if(topCount <= 0)
                return;

            for(int i = 0; i < topCount; i++)
                _itemModels[i].SetScore(_itemModels[i].Score + Random.Range(10, 31));

            SortByScoreAndUpdateRank();
            NotifyUpdated();
        }

        public void SetWaveScore()
        {
            for(int i = 0; i < _itemModels.Count; i++)
            {
                float normalized = _itemModels.Count <= 1 ? 0f : (float)i / (_itemModels.Count - 1);
                int waveScore = Mathf.RoundToInt(Mathf.Lerp(4900f, 700f, normalized)) + Random.Range(-240, 241);
                _itemModels[i].SetScore(Mathf.Clamp(waveScore, SCORE_MIN, SCORE_MAX - 1));
            }

            SortByScoreAndUpdateRank();
            NotifyUpdated();
        }

        public void AddItemAndRandomize()
        {
            _itemModels.Add(new RankItemModel(_nextItemId));
            _nextItemId++;
            SetRandomScore();
        }

        public void RemoveLastItemAndRandomize()
        {
            if(_itemModels.Count <= 1)
                return;

            _itemModels.RemoveAt(_itemModels.Count - 1);
            SetRandomScore();
        }

        private void SortByScoreAndUpdateRank()
        {
            _itemModels.Sort((left, right) => right.Score.CompareTo(left.Score));
            for(int i = 0; i < _itemModels.Count; i++)
                _itemModels[i].SetRank(i + 1);
        }
    }
}
