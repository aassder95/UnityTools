using UnityEngine;

namespace UnityTools.Samples.Util
{
    public readonly struct SampleTestLayout
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly RectTransform _rtRoot;
        private readonly RectTransform _rtControls;
        private readonly RectTransform _rtContentViewport;

        //============================================================
        // Properties
        //============================================================
        public RectTransform RtRoot => _rtRoot;
        public RectTransform RtControls => _rtControls;
        public RectTransform RtContentViewport => _rtContentViewport;

        //============================================================
        // Constructors
        //============================================================
        public SampleTestLayout(RectTransform rtRoot, RectTransform rtControls, RectTransform rtContentViewport)
        {
            _rtRoot = rtRoot;
            _rtControls = rtControls;
            _rtContentViewport = rtContentViewport;
        }
    }
}
