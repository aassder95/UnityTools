using UnityEngine;

namespace UnityTools.Util
{
    public static class TransformExtensions
    {
        public static void SetSizeHeight(this RectTransform rt, float height)
        {
            if (rt == null)
            {
                Debug.LogWarning("[TransformExtensions:SetSizeHeight] RectTransform is null");
                return;
            }

            rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
        }

        public static void SetAnchoredPositionY(this RectTransform rt, float y)
        {
            if (rt == null)
            {
                Debug.LogWarning("[TransformExtensions:SetAnchoredPositionY] RectTransform is null");
                return;
            }

            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
        }
    }
}