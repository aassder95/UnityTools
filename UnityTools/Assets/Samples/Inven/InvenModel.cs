using System.Collections.Generic;
using UnityEngine;
using UnityTools.Util.Extensions;
using UnityTools.Util.UIFramework;

namespace UnityTools.Samples.Inven
{
    public class InvenModel : BaseModel
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly List<InvenItemModel> _itemModels = new();

        //============================================================
        //Properties
        //============================================================
        public int ItemCount => _itemModels.Count;

        //============================================================
        //Constructors
        //============================================================
        public InvenModel(int cnt)
        {
            for(int i = 0; i < cnt; i++)
                _itemModels.Add(new InvenItemModel(i));
        }

        //============================================================
        //Logic
        //============================================================
        public InvenItemModel Get(int idx)
        {
            return _itemModels.IsValidIndex(idx) ? _itemModels[idx] : null;
        }

        public void ShuffleItems()
        {
            for(int i = _itemModels.Count - 1; i > 0; i--)
            {
                int swapIndex = Random.Range(0, i + 1);
                InvenItemModel tempItem = _itemModels[i];
                _itemModels[i] = _itemModels[swapIndex];
                _itemModels[swapIndex] = tempItem;
            }

            NotifyUpdated();
        }

        public void SortByCountDesc()
        {
            _itemModels.Sort((left, right) => right.Count.CompareTo(left.Count));
            NotifyUpdated();
        }

        public void SortByGradeDesc()
        {
            _itemModels.Sort((left, right) =>
            {
                int gradeCompare = right.Grade.CompareTo(left.Grade);
                if(gradeCompare != 0)
                    return gradeCompare;

                return right.Count.CompareTo(left.Count);
            });

            NotifyUpdated();
        }

        public void RandomizeCounts()
        {
            for(int i = 0; i < _itemModels.Count; i++)
                _itemModels[i].RandomizeGradeAndCount();

            NotifyUpdated();
        }
    }
}
