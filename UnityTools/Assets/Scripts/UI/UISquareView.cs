using TMPro;
using UnityEngine;
using UnityTools.Util;

namespace UnityTools.UI
{
    public class UISquareView : UIView
    {
        [SerializeField] TextMeshProUGUI _txtIdx;
        [SerializeField] RectTransform _rtSquare;

        public override void OnGet()
        {
        }

        public override void OnReturn()
        {
        }

        public void SetIndex(int idx)
        {
            _txtIdx.SetText("{0}", idx);
        }

        public void SetPositionY(float y)
        {
            _rtSquare.SetAnchoredPositionY(y);
        }
    }
}