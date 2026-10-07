using NUnit.Framework;

namespace UnityTools.Qa.Tests
{
    public class RewardAdTests
    {
        //============================================================
        // Logic
        //============================================================
        [Test]
        public void CompletionConsumesLoadedAdAndDoesNotRepeatReward()
        {
            using (var simulator = new RewardAdSimulator())
            {
                Assert.That(simulator.TryConfigure(new RewardAdOptions(1.0f, 2.0f)), Is.True);
                Assert.That(simulator.TryLoad("reward"), Is.True);
                Assert.That(simulator.TryLoad("reward"), Is.False);
                Assert.That(simulator.TryShow("reward"), Is.False);
                simulator.Advance(0.5f);
                Assert.That(simulator.TryShow("reward"), Is.False);
                simulator.Advance(0.5f);
                Assert.That(simulator.TryShow("reward"), Is.True);
                simulator.Advance(2.0f);
                Assert.That(simulator.TryGetSnapshot("reward", out var snapshot), Is.True);
                Assert.That(snapshot.Result, Is.EqualTo(ERewardAdResult.Completed));
                Assert.That(simulator.TryShow("reward"), Is.False);
                simulator.Advance(20.0f);
                Assert.That(simulator.TryGetSnapshot("reward", out snapshot), Is.True);
                Assert.That(snapshot.State, Is.EqualTo(ERewardAdState.Idle));
            }
        }

        [TestCase(ERewardAdResult.NoFill)]
        [TestCase(ERewardAdResult.NetworkError)]
        public void LoadFailuresNeverBecomeReady(ERewardAdResult outcome)
        {
            using (var simulator = new RewardAdSimulator())
            {
                Assert.That(simulator.TryConfigure(new RewardAdOptions(0.0f, 1.0f, outcome)), Is.True);
                Assert.That(simulator.TryLoad("reward"), Is.True);
                simulator.Advance(0.0f);
                Assert.That(simulator.TryShow("reward"), Is.False);
                Assert.That(simulator.TryGetSnapshot("reward", out var snapshot), Is.True);
                Assert.That(snapshot.Result, Is.EqualTo(outcome));
            }
        }

        [TestCase(ERewardAdResult.Skipped)]
        [TestCase(ERewardAdResult.ShowFailed)]
        public void ShowFailuresNeverGrantCompletion(ERewardAdResult outcome)
        {
            using (var simulator = new RewardAdSimulator())
            {
                Assert.That(simulator.TryConfigure(new RewardAdOptions(0.0f, 1.0f, ERewardAdResult.Loaded, outcome)), Is.True);
                Assert.That(simulator.TryLoad("reward"), Is.True);
                simulator.Advance(0.0f);
                Assert.That(simulator.TryShow("reward"), Is.True);
                simulator.Advance(1.0f);
                Assert.That(simulator.TryGetSnapshot("reward", out var snapshot), Is.True);
                Assert.That(snapshot.Result, Is.EqualTo(outcome));
            }
        }

        [Test]
        public void CompletionNotificationIsEmittedExactlyOnce()
        {
            using (var simulator = new RewardAdSimulator())
            {
                var notifications = new System.Collections.Generic.List<RewardAdSnapshot>();
                simulator.OnChanged += notifications.Add;
                try
                {
                    Assert.That(simulator.TryConfigure(new RewardAdOptions(0.0f, 1.0f)), Is.True);
                    Assert.That(simulator.TryLoad("reward"), Is.True);
                    simulator.Advance(0.0f);
                    Assert.That(simulator.TryShow("reward"), Is.True);
                    simulator.Advance(2.0f);
                    simulator.Advance(2.0f);
                    Assert.That(notifications.FindAll(snapshot => snapshot.Result == ERewardAdResult.Completed).Count, Is.EqualTo(1));
                }
                finally
                {
                    simulator.OnChanged -= notifications.Add;
                }
            }
        }

        [Test]
        public void CancelAndGlobalShowGateApplyAcrossPlacements()
        {
            using (var simulator = new RewardAdSimulator())
            {
                Assert.That(simulator.TryConfigure(new RewardAdOptions(0.0f, 1.0f)), Is.True);
                Assert.That(simulator.TryLoad("a"), Is.True);
                Assert.That(simulator.TryLoad("b"), Is.True);
                simulator.Advance(0.0f);
                Assert.That(simulator.TryShow("a"), Is.True);
                Assert.That(simulator.TryShow("b"), Is.False);
                Assert.That(simulator.TryCancel("a"), Is.True);
                Assert.That(simulator.TryCancel("a"), Is.False);
                Assert.That(simulator.TryShow("b"), Is.True);
            }
        }
    }
}
