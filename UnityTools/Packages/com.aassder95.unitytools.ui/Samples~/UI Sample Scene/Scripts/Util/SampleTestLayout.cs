using UnityEngine;

namespace UnityTools.Samples.Util
{
    public readonly struct SampleTestLayout
    {
        //============================================================
        // Readonly
        //============================================================
        public readonly RectTransform RtRoot;
        public readonly RectTransform RtControls;
        public readonly RectTransform RtContentViewport;

        //============================================================
        // Constructors
        //============================================================
        public SampleTestLayout(RectTransform rtRoot, RectTransform rtControls, RectTransform rtContentViewport)
        {
            RtRoot = rtRoot;
            RtControls = rtControls;
            RtContentViewport = rtContentViewport;
        }
    }
}
