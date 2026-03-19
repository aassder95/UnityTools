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

namespace UnityTools.Samples.Templates
{
    public class TemplateSampleModel : BaseModel
    {
        //============================================================
        //Fields
        //============================================================
        private int _count;

        //============================================================
        //Properties
        //============================================================
        public int Count => _count;

        //============================================================
        //Logic
        //============================================================
        public void SetCount(int count)
        {
            SetField(ref _count, count);
        }

        public void Increase()
        {
            SetCount(_count + 1);
        }
    }
}
