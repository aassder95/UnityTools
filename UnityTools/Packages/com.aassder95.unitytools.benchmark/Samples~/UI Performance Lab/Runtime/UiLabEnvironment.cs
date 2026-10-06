using UnityEngine;

namespace UnityTools.Benchmark.Samples
{
    public class UiLabEnvironment
    {
        //============================================================
        // Properties
        //============================================================
        public int ScreenWidth { get; }
        public int ScreenHeight { get; }
        public int QualityIdx { get; }
        public string QualityName { get; }
        public int VSyncCnt { get; }
        public int TargetFrameRate { get; }

        //============================================================
        // Constructors
        //============================================================
        public UiLabEnvironment(int screenWidth, int screenHeight, int qualityIdx, string qualityName, int vSyncCnt, int targetFrameRate)
        {
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
            QualityIdx = qualityIdx;
            QualityName = qualityName;
            VSyncCnt = vSyncCnt;
            TargetFrameRate = targetFrameRate;
        }

        //============================================================
        // Logic
        //============================================================
        public static UiLabEnvironment Capture()
        {
            int qualityIdx = QualitySettings.GetQualityLevel();
            return new UiLabEnvironment(Screen.width, Screen.height, qualityIdx, QualitySettings.names[qualityIdx], QualitySettings.vSyncCount, Application.targetFrameRate);
        }

        public bool MatchesCurrent()
        {
            return ScreenWidth == Screen.width && ScreenHeight == Screen.height && QualityIdx == QualitySettings.GetQualityLevel() && VSyncCnt == QualitySettings.vSyncCount && TargetFrameRate == Application.targetFrameRate;
        }
    }
}
