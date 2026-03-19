using UnityEngine;

namespace UnityTools.Util
{
    public static class RandomUtils
    {
        //============================================================
        //Constants
        //============================================================
        private const float HALF = 0.5f;

        //============================================================
        //Logic
        //============================================================
        public static Vector2 GetRandomPositionInRange(Vector2 center, Vector2 range)
        {
            float x = UnityEngine.Random.Range(center.x - range.x * HALF, center.x + range.x * HALF);
            float y = UnityEngine.Random.Range(center.y - range.y * HALF, center.y + range.y * HALF);
            return new Vector2(x, y);
        }

        public static Color GetRandomColor()
        {
            return new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
        }
    }
}
