using UnityEngine;

namespace UnityTools.Util
{
    public static class Utils
    {
        public static Vector2 GetRandomPos(Vector2 center, Vector2 range)
        {
            float x = Random.Range(center.x - range.x * 0.5f, center.x + range.x * 0.5f);
            float y = Random.Range(center.y - range.y * 0.5f, center.y + range.y * 0.5f);
            return new Vector2(x, y);
        }

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