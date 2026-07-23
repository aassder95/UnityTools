using System.Collections.Generic;
using UnityEngine;
using UnityTools.Util.UIFramework;

namespace UnityTools.Samples.Inven
{
    public class InvenModel : BaseModel
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly List<InvenItemModel> _itemModels = new();

        //============================================================
        // Properties
        //============================================================
        public InvenItemModel this[int idx] => _itemModels[idx];
        public int ItemCnt => _itemModels.Count;

        //============================================================
        // Constructors
        //============================================================
        public InvenModel(int cnt)
        {
            for(int i = 0; i < cnt; i++)
            {
                _itemModels.Add(new InvenItemModel(i));
            }
        }

        //============================================================
        // Logic
        //============================================================
        public void ShuffleItems()
        {
            for(int i = _itemModels.Count - 1; i > 0; i--)
            {
                int swapIdx = Random.Range(0, i + 1);
                InvenItemModel tempItem = _itemModels[i];
                _itemModels[i] = _itemModels[swapIdx];
                _itemModels[swapIdx] = tempItem;
            }

            NotifyUpdated();
        }

        public void SortByCntDesc()
        {
            _itemModels.Sort((left, right) => right.Cnt.CompareTo(left.Cnt));
            NotifyUpdated();
        }

        public void SortByGradeDesc()
        {
            _itemModels.Sort((left, right) =>
            {
                int gradeCompare = right.Grade.CompareTo(left.Grade);
                if(gradeCompare != 0)
                    return gradeCompare;

                return right.Cnt.CompareTo(left.Cnt);
            });

            NotifyUpdated();
        }

        public void RandomizeItemCnts()
        {
            for(int i = 0; i < _itemModels.Count; i++)
            {
                _itemModels[i].RandomizeGradeAndCnt();
            }

            NotifyUpdated();
        }
    }
}
