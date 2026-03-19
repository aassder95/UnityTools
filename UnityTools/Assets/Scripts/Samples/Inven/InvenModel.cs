using System.Collections.Generic;
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
