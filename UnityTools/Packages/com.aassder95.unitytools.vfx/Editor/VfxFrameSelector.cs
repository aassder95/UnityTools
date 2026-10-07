using UnityEngine;

namespace UnityTools.Vfx.Editor
{
    public static class VfxFrameSelector
    {
        //============================================================
        // Logic
        //============================================================
        public static bool TrySelect(GameObject prefab, float durationSec, int sampleCnt, out float timeSec)
        {
            timeSec = 0.0f;
            if (prefab == null || float.IsNaN(durationSec) || float.IsInfinity(durationSec) || durationSec <= 0.0f || durationSec > VfxPreviewSession.MAX_PREVIEW_SEC || sampleCnt < 2 || sampleCnt > 120)
                return false;

            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
                return false;

            using (var session = new VfxPreviewSession(prefab))
            {
                if (!session.TrySeek(0.0f))
                    return false;

                // 동일한 카메라로 점수를 비교하도록 전체 구간의 bounds를 먼저 확보합니다.
                Bounds bounds = default;
                bool hasBounds = false;
                for (int idx = 0; idx < sampleCnt; idx++)
                {
                    session.Advance(idx == 0 ? 0.0f : durationSec / (sampleCnt - 1));
                    session.Fit();
                    if (hasBounds)
                        bounds.Encapsulate(session.ViewBounds);
                    else
                        bounds = session.ViewBounds;

                    hasBounds = true;
                }

                float bestScore = 0.0f;
                for (int idx = 0; idx < sampleCnt; idx++)
                {
                    float candidateSec = durationSec * idx / (sampleCnt - 1);
                    if (!session.TrySeek(candidateSec))
                        return false;

                    session.FitBounds(bounds);
                    Texture2D image = session.Capture(64, 64);
                    if (image == null)
                        return false;

                    try
                    {
                        float score = Score(image.GetPixels(), image.width, image.height);
                        if (score > bestScore)
                        {
                            bestScore = score;
                            timeSec = candidateSec;
                        }
                    }
                    finally
                    {
                        Object.DestroyImmediate(image);
                    }
                }

                return bestScore > 0.0f;
            }
        }

        public static float Score(Color[] pixels, int width, int height)
        {
            if (pixels == null || width < 2 || height < 2 || (long)width * height != pixels.Length)
                return 0.0f;

            // 배경색/색 공간을 가정하지 않고 가장자리 네 모서리의 중앙값을 사용합니다.
            Color[] corners = { pixels[0], pixels[width - 1], pixels[(height - 1) * width], pixels[pixels.Length - 1] };
            float[] values = new float[4];
            Color background = default;
            for (int channel = 0; channel < 3; channel++)
            {
                for (int idx = 0; idx < 4; idx++)
                {
                    values[idx] = corners[idx][channel];
                }

                System.Array.Sort(values);
                background[channel] = (values[1] + values[2]) * 0.5f;
            }

            float score = 0.0f;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = pixels[y * width + x];
                    float contrast = Mathf.Max(Mathf.Abs(pixel.r - background.r), Mathf.Abs(pixel.g - background.g), Mathf.Abs(pixel.b - background.b));
                    if (contrast < 0.04f)
                        continue;

                    float dx = (x + 0.5f) / width - 0.5f;
                    float dy = (y + 0.5f) / height - 0.5f;
                    float centerWeight = Mathf.Clamp01(1.0f - dx * dx - dy * dy);
                    score += Mathf.Min(contrast, 1.0f) * centerWeight;
                }
            }

            return score / pixels.Length;
        }
    }
}
