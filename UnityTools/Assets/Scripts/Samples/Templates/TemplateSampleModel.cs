namespace UnityTools.Samples.Templates
{
    public class TemplateSampleModel : UnityTools.Util.BaseModel
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
