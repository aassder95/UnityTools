using UnityEngine;

namespace UnityTools.Util
{
    //============================================================
    //Logic
    //============================================================
    public static class TransformExtensions
    {
        public static void SetSizeWidth(this RectTransform rt, float width)
        {
            if (rt == null)
            {
                Debug.LogWarning("[TransformExtensions:SetSizeWidth] RectTransform가 null입니다.");
                return;
            }

            rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);
        }

        public static void SetSizeHeight(this RectTransform rt, float height)
        {
            if (rt == null)
            {
                Debug.LogWarning("[TransformExtensions:SetSizeHeight] RectTransform가 null입니다.");
                return;
            }

            rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
        }

        public static void SetAnchoredPositionY(this RectTransform rt, float y)
        {
            if (rt == null)
            {
                Debug.LogWarning("[TransformExtensions:SetAnchoredPositionY] RectTransform가 null입니다.");
                return;
            }

            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
        }
    }
}
