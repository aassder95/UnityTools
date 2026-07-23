using UnityEngine;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Core.Pooling;
using UnityTools.Util.Utilities;

namespace UnityTools.Samples
{
    [AddComponentMenu("UnityTools/Samples/Spawner Square")]
    public class SpawnerSquare : Spawner<Square>
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Square Spawn")]
        [SerializeField] private Vector2 _range;

        //============================================================
        // Utilities
        //============================================================
        protected override bool ValidateSpawnConfig()
        {
            if(_range.x >= 0.0f && _range.y >= 0.0f)
                return true;

            DebugLogger.LogError("SpawnerSquare의 Range는 0 이상이어야 합니다. 값=" + _range, this);
            return false;
        }

        protected override Vector3 GetSpawnPos()
        {
            return RandomUtils.GetRandomPosInRange(transform.position, _range);
        }
    }
}
