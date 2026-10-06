using UnityEngine;

namespace UnityTools.Ui
{
    [CreateAssetMenu(menuName = "UnityTools/UI/Reward Motion", fileName = "RewardMotion")]
    public class UiRewardMotion : ScriptableObject
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Spread")]
        [SerializeField, Min(0.0f)] private float _spreadRadius = 80.0f;
        [SerializeField, Min(0.0f)] private float _spreadDurationSec = 0.2f;
        [SerializeField, Min(0.0f)] private float _waitDurationSec = 0.1f;
        [Header("Flight")]
        [SerializeField, Min(0.0f)] private float _flyDurationSec = 0.5f;
        [SerializeField, Min(0.0f)] private float _staggerSec = 0.03f;
        [SerializeField] private bool _isUnscaledTime = true;
        [Header("Scale")]
        [SerializeField, Min(0.0f)] private float _startScale = 0.6f;
        [SerializeField, Min(0.0f)] private float _spreadScale = 1.0f;
        [SerializeField, Min(0.0f)] private float _endScale = 0.4f;

        //============================================================
        // Properties
        //============================================================
        public float SpreadRadius => _spreadRadius;
        public float SpreadDurationSec => _spreadDurationSec;
        public float WaitDurationSec => _waitDurationSec;
        public float FlyDurationSec => _flyDurationSec;
        public float StaggerSec => _staggerSec;
        public bool IsUnscaledTime => _isUnscaledTime;
        public float StartScale => _startScale;
        public float SpreadScale => _spreadScale;
        public float EndScale => _endScale;
    }
}
