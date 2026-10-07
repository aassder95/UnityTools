namespace UnityTools.Qa
{
    public class RewardAdOptions
    {
        //============================================================
        // Properties
        //============================================================
        public float LoadDelaySec { get; }
        public float WatchDurationSec { get; }
        public ERewardAdResult LoadResult { get; }
        public ERewardAdResult ShowResult { get; }

        //============================================================
        // Constructors
        //============================================================
        public RewardAdOptions(float loadDelaySec = 1.0f, float watchDurationSec = 5.0f, ERewardAdResult loadResult = ERewardAdResult.Loaded, ERewardAdResult showResult = ERewardAdResult.Completed)
        {
            LoadDelaySec = loadDelaySec;
            WatchDurationSec = watchDurationSec;
            LoadResult = loadResult;
            ShowResult = showResult;
        }
    }
}
