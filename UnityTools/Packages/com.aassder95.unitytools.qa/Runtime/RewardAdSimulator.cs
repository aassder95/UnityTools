using System;
using System.Collections.Generic;

namespace UnityTools.Qa
{
    public class RewardAdSimulator : IDisposable
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly Dictionary<string, Placement> _placements = new Dictionary<string, Placement>(StringComparer.Ordinal);
        private readonly List<RewardAdSnapshot> _notifications = new List<RewardAdSnapshot>();

        //============================================================
        // Fields
        //============================================================
        private RewardAdOptions _options;
        private bool _isDisposed;
        private bool _isAdvancing;
        private string _showingPlacement;

        //============================================================
        // Events
        //============================================================
        private event Action<RewardAdSnapshot> _onChanged;
        public event Action<RewardAdSnapshot> OnChanged { add => _onChanged += value; remove => _onChanged -= value; }

        //============================================================
        // Init/Register
        //============================================================
        public bool TryConfigure(RewardAdOptions options)
        {
            if (_isDisposed || options == null || !IsFinite(options.LoadDelaySec) || !IsFinite(options.WatchDurationSec) || options.LoadDelaySec < 0.0f || options.WatchDurationSec <= 0.0f)
                return false;

            if (options.LoadResult != ERewardAdResult.Loaded && options.LoadResult != ERewardAdResult.NoFill && options.LoadResult != ERewardAdResult.NetworkError)
                return false;

            if (options.ShowResult != ERewardAdResult.Completed && options.ShowResult != ERewardAdResult.Skipped && options.ShowResult != ERewardAdResult.ShowFailed)
                return false;

            if (_isAdvancing)
                return false;

            foreach (Placement placement in _placements.Values)
            {
                if (placement.State == ERewardAdState.Loading || placement.State == ERewardAdState.Showing)
                    return false;
            }

            _options = options;
            return true;
        }

        public void Dispose()
        {
            _isDisposed = true;
            _placements.Clear();
            _notifications.Clear();
            _showingPlacement = null;
            _onChanged = null;
        }

        //============================================================
        // Logic
        //============================================================
        public bool TryLoad(string key)
        {
            if (_isDisposed || _options == null || string.IsNullOrWhiteSpace(key))
                return false;

            if (!_placements.TryGetValue(key, out Placement placement))
            {
                placement = new Placement();
                _placements.Add(key, placement);
                if (_notifications.Capacity < _placements.Count)
                    _notifications.Capacity = _placements.Count;
            }

            if (placement.State != ERewardAdState.Idle)
                return false;

            placement.State = ERewardAdState.Loading;
            placement.ElapsedSec = 0.0f;
            placement.Result = ERewardAdResult.None;
            _onChanged?.Invoke(Snapshot(key, placement));
            return true;
        }

        public bool TryShow(string key)
        {
            if (_isDisposed || key == null || _showingPlacement != null || !_placements.TryGetValue(key, out Placement placement) || placement.State != ERewardAdState.Ready)
                return false;

            _showingPlacement = key;
            placement.State = ERewardAdState.Showing;
            placement.ElapsedSec = 0.0f;
            placement.Result = ERewardAdResult.None;
            _onChanged?.Invoke(Snapshot(key, placement));
            return true;
        }

        public bool TryCancel(string key)
        {
            if (_isDisposed || key == null || !_placements.TryGetValue(key, out Placement placement) || (placement.State != ERewardAdState.Loading && placement.State != ERewardAdState.Showing))
                return false;

            placement.State = ERewardAdState.Idle;
            placement.Result = ERewardAdResult.Canceled;
            if (_showingPlacement == key)
                _showingPlacement = null;

            _onChanged?.Invoke(Snapshot(key, placement));
            return true;
        }

        public bool TryGetSnapshot(string key, out RewardAdSnapshot snapshot)
        {
            snapshot = default;
            if (_isDisposed || key == null || !_placements.TryGetValue(key, out Placement placement))
                return false;

            snapshot = Snapshot(key, placement);
            return true;
        }

        public void Advance(float deltaSec)
        {
            if (_isDisposed || _isAdvancing || _options == null || !IsFinite(deltaSec) || deltaSec < 0.0f)
                return;

            _isAdvancing = true;
            try
            {
                _notifications.Clear();
                foreach (KeyValuePair<string, Placement> pair in _placements)
                {
                    Placement placement = pair.Value;
                    if (placement.State != ERewardAdState.Loading && placement.State != ERewardAdState.Showing)
                        continue;

                    bool isLoading = placement.State == ERewardAdState.Loading;
                    float durationSec = isLoading ? _options.LoadDelaySec : _options.WatchDurationSec;
                    placement.ElapsedSec = Math.Min(durationSec, placement.ElapsedSec + deltaSec);
                    if (placement.ElapsedSec < durationSec)
                        continue;

                    placement.Result = isLoading ? _options.LoadResult : _options.ShowResult;
                    placement.State = placement.Result == ERewardAdResult.Loaded ? ERewardAdState.Ready : ERewardAdState.Idle;
                    if (_showingPlacement == pair.Key)
                        _showingPlacement = null;

                    _notifications.Add(Snapshot(pair.Key, placement));
                }

                for (int idx = 0; idx < _notifications.Count && !_isDisposed; idx++)
                {
                    _onChanged?.Invoke(_notifications[idx]);
                }
            }
            finally
            {
                _isAdvancing = false;
                _notifications.Clear();
            }
        }

        //============================================================
        // Utilities
        //============================================================
        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static RewardAdSnapshot Snapshot(string key, Placement placement)
        {
            return new RewardAdSnapshot(key, placement.State, placement.Result, placement.ElapsedSec);
        }

        //============================================================
        // Nested Types
        //============================================================
        private class Placement
        {
            public ERewardAdState State { get; set; }
            public ERewardAdResult Result { get; set; }
            public float ElapsedSec { get; set; }
        }
    }
}
