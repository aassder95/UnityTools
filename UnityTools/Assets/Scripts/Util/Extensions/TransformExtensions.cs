using UnityEngine;

namespace UnityTools.Util
{
    public static class TransformExtensions
    {
        public static void SetSizeHeight(this RectTransform rt, float height)
        {
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
        }

        public static void SetAnchoredPositionY(this RectTransform rt, float y)
        {
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
        }
    }
}