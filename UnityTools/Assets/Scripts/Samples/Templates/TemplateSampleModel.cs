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
            if (_count == count)
                return;

            _count = count;
            NotifyUpdated();
        }

        public void Increase()
        {
            SetCount(_count + 1);
        }
    }
}
