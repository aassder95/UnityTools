namespace UnityTools.Qa
{
    public struct RewardAdSnapshot
    {
        //============================================================
        // Properties
        //============================================================
        public string Placement { get; }
        public ERewardAdState State { get; }
        public ERewardAdResult Result { get; }
        public float ElapsedSec { get; }

        //============================================================
        // Constructors
        //============================================================
        public RewardAdSnapshot(string placement, ERewardAdState state, ERewardAdResult result, float elapsedSec)
        {
            Placement = placement;
            State = state;
            Result = result;
            ElapsedSec = elapsedSec;
        }
    }
}
