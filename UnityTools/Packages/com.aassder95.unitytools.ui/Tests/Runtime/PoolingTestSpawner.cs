using UnityEngine;
using UnityTools.Util.Core.Pooling;

namespace UnityTools.Util.Tests.Lifecycle
{
    public class PoolingTestSpawner : Spawner<PoolingTestItem>
    {
        //============================================================
        // Utilities
        //============================================================
        protected override Vector3 GetSpawnPos()
        {
            return Vector3.zero;
        }
    }
}
