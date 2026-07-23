
using UnityTools.Util.UIFramework;

namespace UnityTools.Samples.Templates
{
    public class TemplateSampleModel : BaseModel
    {
        //============================================================
        // Fields
        //============================================================
        private int _cnt;

        //============================================================
        // Properties
        //============================================================
        public int Cnt => _cnt;

        //============================================================
        // Logic
        //============================================================
        public void SetCnt(int cnt)
        {
            SetField(ref _cnt, cnt);
        }

        public void Increase()
        {
            SetCnt(_cnt + 1);
        }
    }
}
