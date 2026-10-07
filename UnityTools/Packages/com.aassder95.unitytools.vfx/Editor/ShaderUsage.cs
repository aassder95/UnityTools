using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Vfx.Editor
{
    public class ShaderUsage
    {
        //============================================================
        // Properties
        //============================================================
        public Shader Shader { get; }
        public IReadOnlyList<Material> Materials { get; }

        //============================================================
        // Constructors
        //============================================================
        public ShaderUsage(Shader shader, IReadOnlyList<Material> materials)
        {
            Shader = shader;
            Materials = materials;
        }
    }
}
