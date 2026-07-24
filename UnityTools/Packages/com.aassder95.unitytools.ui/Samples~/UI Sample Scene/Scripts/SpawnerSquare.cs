using UnityEngine;
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
        protected override Vector3 GetSpawnPos()
        {
            return RandomUtils.GetRandomPosInRange(transform.position, _range);
        }
    }
}
