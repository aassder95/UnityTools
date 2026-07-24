using UnityEngine;

namespace UnityTools.Samples.Util
{
    public static class RankItemPalette
    {
        //============================================================
        // Properties
        //============================================================
        public static Color IdColor => new Color(0.93f, 0.96f, 1.0f, 1.0f);

        //============================================================
        // Logic
        //============================================================
        public static Color ResolveRankColor(int rank)
        {
            if (rank <= 1)
                return new Color(1.0f, 0.86f, 0.4f, 1.0f);

            if (rank <= 3)
                return new Color(0.62f, 0.88f, 1.0f, 1.0f);

            return new Color(0.91f, 0.94f, 1.0f, 1.0f);
        }

        public static Color ResolveScoreColor(int score)
        {
            float normalized = Mathf.InverseLerp(1.0f, 5000.0f, score);
            return Color.Lerp(new Color(0.54f, 0.84f, 1.0f, 1.0f), new Color(1.0f, 0.64f, 0.7f, 1.0f), normalized);
        }

        public static Color ResolveBgColor(Color sourceColor)
        {
            Color opaqueColor = new Color(sourceColor.r, sourceColor.g, sourceColor.b, 1.0f);
            return Color.Lerp(new Color(0.05f, 0.09f, 0.16f, 1.0f), opaqueColor, 0.18f);
        }
    }
}
