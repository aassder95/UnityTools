using System.Collections.Generic;
using UnityTools.Util;

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
            for (int i = 0; i < cnt; i++)
            {
                _itemModels.Add(new(i));
            }
        }

        //============================================================
        //Logic
        //============================================================
        public InvenItemModel Get(int idx)
        {
            return _itemModels.IsValidIndex(idx) ? _itemModels[idx] : null;
        }
    }
}
