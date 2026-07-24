using System.Collections.Generic;
using UnityEngine;
using UnityTools.Util.UIFramework;

namespace UnityTools.Samples.Rank
{
    public class RankModel : BaseModel
    {
        //============================================================
        // Constants
        //============================================================
        private const int SCORE_MIN = 1;
        private const int SCORE_MAX = 5001;

        //============================================================
        // Readonly
        //============================================================
        private readonly List<RankItemModel> _itemModels = new();

        //============================================================
        // Fields
        //============================================================
        private int _nextItemId;

        //============================================================
        // Properties
        //============================================================
        public RankItemModel this[int idx] => _itemModels[idx];
        public int ItemCnt => _itemModels.Count;

        //============================================================
        // Constructors
        //============================================================
        public RankModel(int cnt)
        {
            for (int i = 0; i < cnt; i++)
            {
                _itemModels.Add(new RankItemModel(i));
            }

            _nextItemId = cnt;
            SetRandomScore();
        }

        //============================================================
        // Logic
        //============================================================
        public void SetRandomScore()
        {
            for (int i = 0; i < _itemModels.Count; i++)
            {
                _itemModels[i].SetScore(Random.Range(SCORE_MIN, SCORE_MAX));
            }

            SortByScoreAndUpdateRank();
            NotifyUpdated();
        }

        public void BoostTopScores()
        {
            int topCnt = Mathf.Min(3, _itemModels.Count);
            if (topCnt <= 0)
                return;

            for (int i = 0; i < topCnt; i++)
            {
                _itemModels[i].SetScore(_itemModels[i].Score + Random.Range(10, 31));
            }

            SortByScoreAndUpdateRank();
            NotifyUpdated();
        }

        public void SetWaveScore()
        {
            for (int i = 0; i < _itemModels.Count; i++)
            {
                float normalized = _itemModels.Count <= 1 ? 0.0f : (float)i / (_itemModels.Count - 1);
                int waveScore = Mathf.RoundToInt(Mathf.Lerp(4900.0f, 700.0f, normalized)) + Random.Range(-240, 241);
                _itemModels[i].SetScore(Mathf.Clamp(waveScore, SCORE_MIN, SCORE_MAX - 1));
            }

            SortByScoreAndUpdateRank();
            NotifyUpdated();
        }

        public void AddItem()
        {
            RankItemModel itemModel = new(_nextItemId);
            itemModel.SetScore(Random.Range(SCORE_MIN, SCORE_MAX));
            _itemModels.Add(itemModel);
            _nextItemId++;
            SortByScoreAndUpdateRank();
            NotifyUpdated();
        }

        public void RemoveLastItem()
        {
            if (_itemModels.Count <= 1)
                return;

            _itemModels.RemoveAt(_itemModels.Count - 1);
            SortByScoreAndUpdateRank();
            NotifyUpdated();
        }

        private void SortByScoreAndUpdateRank()
        {
            _itemModels.Sort((left, right) => right.Score.CompareTo(left.Score));
            for (int i = 0; i < _itemModels.Count; i++)
            {
                _itemModels[i].SetRank(i + 1);
            }
        }
    }
}
