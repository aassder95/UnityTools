using UnityEngine;

namespace UnityTools.Vfx.Editor
{
    public static class VfxColorAnalyzer
    {
        //============================================================
        // Logic
        //============================================================
        public static EVfxColor Analyze(Color[] pixels, Color background)
        {
            if (pixels == null || pixels.Length == 0)
                return EVfxColor.Invisible;

            float[] weights = new float[(int)EVfxColor.White + 1];
            for (int idx = 0; idx < pixels.Length; idx++)
            {
                Color pixel = pixels[idx];
                float dist = Mathf.Abs(pixel.r - background.r) + Mathf.Abs(pixel.g - background.g) + Mathf.Abs(pixel.b - background.b);
                if (pixel.a < 0.05f || dist < 0.15f)
                    continue;

                Color.RGBToHSV(pixel, out float hue, out float saturation, out float value);
                if (value < 0.15f)
                    continue;

                EVfxColor color;
                if (saturation < 0.18f)
                {
                    color = EVfxColor.White;
                }
                else if (hue < 0.04f || hue >= 0.96f)
                {
                    color = EVfxColor.Red;
                }
                else if (hue < 0.11f)
                {
                    color = EVfxColor.Orange;
                }
                else if (hue < 0.19f)
                {
                    color = EVfxColor.Yellow;
                }
                else if (hue < 0.43f)
                {
                    color = EVfxColor.Green;
                }
                else if (hue < 0.54f)
                {
                    color = EVfxColor.Cyan;
                }
                else if (hue < 0.71f)
                {
                    color = EVfxColor.Blue;
                }
                else if (hue < 0.84f)
                {
                    color = EVfxColor.Purple;
                }
                else
                {
                    color = EVfxColor.Pink;
                }

                weights[(int)color] += pixel.a * value;
            }

            EVfxColor dominant = EVfxColor.Invisible;
            float maxWeight = 0.0f;
            for (int idx = (int)EVfxColor.Red; idx < weights.Length; idx++)
            {
                if (weights[idx] > maxWeight)
                {
                    maxWeight = weights[idx];
                    dominant = (EVfxColor)idx;
                }
            }

            return dominant;
        }
    }
}
