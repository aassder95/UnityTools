using UnityEngine;
using UnityTools.Util.UIFramework;

namespace UnityTools.Samples.Inven
{
    public class InvenItemModel : BaseModel
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly int _id;
        private readonly string _itemName;

        //============================================================
        // Fields
        //============================================================
        private EInvenGrade _grade;
        private int _cnt;

        //============================================================
        // Properties
        //============================================================
        public int Id => _id;
        public string ItemName => _itemName;
        public EInvenGrade Grade => _grade;
        public int Cnt => _cnt;

        //============================================================
        // Constructors
        //============================================================
        public InvenItemModel(int id)
        {
            _id = id;
            _itemName = $"ITEM-{id:D3}";
            _grade = (EInvenGrade)Random.Range(0, 4);
            _cnt = Random.Range(1, 100);
        }

        //============================================================
        // Logic
        //============================================================
        public void RandomizeGradeAndCnt()
        {
            RunBatchUpdate(() =>
            {
                SetField(ref _grade, (EInvenGrade)Random.Range(0, 4));
                SetField(ref _cnt, Random.Range(1, 100));
            });
        }
    }
}
