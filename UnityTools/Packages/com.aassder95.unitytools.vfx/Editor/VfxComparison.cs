using System;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Vfx.Editor
{
    public class VfxComparison : IDisposable
    {
        //============================================================
        // Fields
        //============================================================
        private VfxPreviewSession _left;
        private VfxPreviewSession _right;

        //============================================================
        // Properties
        //============================================================
        public VfxPreviewSession Left => _left;
        public VfxPreviewSession Right => _right;
        public bool HasPreviews => _left != null && _right != null;
        public float TimeSec => HasPreviews ? _left.TimeSec : 0.0f;

        //============================================================
        // Init/Register
        //============================================================
        public void Dispose()
        {
            _left?.Dispose();
            _right?.Dispose();
            _left = null;
            _right = null;
        }

        //============================================================
        // Logic
        //============================================================
        public bool TrySetPrefabs(GameObject left, GameObject right)
        {
            if (left == null || right == null || !PrefabUtility.IsPartOfPrefabAsset(left) || !PrefabUtility.IsPartOfPrefabAsset(right) || left.GetComponentsInChildren<ParticleSystem>(true).Length == 0 || right.GetComponentsInChildren<ParticleSystem>(true).Length == 0)
                return false;

            Dispose();
            _left = new VfxPreviewSession(left);
            _right = new VfxPreviewSession(right);
            bool isSought = TrySeek(0.0f);
            if (isSought)
                _left.FitTogether(_right);

            return isSought;
        }

        public bool TrySeek(float timeSec)
        {
            if (!HasPreviews || float.IsNaN(timeSec) || float.IsInfinity(timeSec) || timeSec < 0.0f || timeSec > VfxPreviewSession.MAX_PREVIEW_SEC)
                return false;

            return _left.TrySeek(timeSec) && _right.TrySeek(timeSec);
        }

        public void Advance(float deltaSec)
        {
            if (!HasPreviews)
                return;

            _left.Advance(deltaSec);
            _right.Advance(deltaSec);
        }

        public void Fit()
        {
            if (HasPreviews)
                _left.FitTogether(_right);
        }

        public void Orbit(Vector2 delta)
        {
            if (!HasPreviews)
                return;

            _left.Orbit(delta);
            _right.Orbit(delta);
        }

        public void Zoom(float delta)
        {
            if (!HasPreviews)
                return;

            _left.Zoom(delta);
            _right.Zoom(delta);
        }
    }
}
