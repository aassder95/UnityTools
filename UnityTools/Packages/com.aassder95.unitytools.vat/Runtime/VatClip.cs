using UnityEngine;

namespace UnityTools.Vat
{
    public class VatClip : ScriptableObject
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private Mesh _mesh;
        [SerializeField] private Texture2D _positions;
        [SerializeField] private Texture2D _normals;
        [SerializeField] private float _durationSec;
        [SerializeField] private int _frameCnt;

        //============================================================
        // Properties
        //============================================================
        public Mesh Mesh => _mesh;
        public Texture2D Positions => _positions;
        public Texture2D Normals => _normals;
        public float DurationSec => _durationSec;
        public int FrameCnt => _frameCnt;

        //============================================================
        // Init/Register
        //============================================================
        public bool TryConfigure(Mesh mesh, Texture2D positions, Texture2D normals, float durationSec, int frameCnt)
        {
            if (mesh == null || positions == null || normals == null || float.IsNaN(durationSec) || float.IsInfinity(durationSec) || durationSec <= 0.0f || frameCnt < 2 || positions.width != mesh.vertexCount || normals.width != mesh.vertexCount || positions.height != frameCnt || normals.height != frameCnt)
                return false;

            _mesh = mesh;
            _positions = positions;
            _normals = normals;
            _durationSec = durationSec;
            _frameCnt = frameCnt;
            return true;
        }
    }
}
