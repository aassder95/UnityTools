namespace UnityTools.Vfx.Editor
{
    public class VfxPrefabInfo
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly string _guid;
        private readonly string _assetPath;
        private readonly string _name;
        private readonly int _particleSystemCnt;
        private readonly bool _isLooping;

        //============================================================
        // Properties
        //============================================================
        public string Guid => _guid;
        public string AssetPath => _assetPath;
        public string Name => _name;
        public int ParticleSystemCnt => _particleSystemCnt;
        public bool IsLooping => _isLooping;

        //============================================================
        // Constructors
        //============================================================
        public VfxPrefabInfo(string guid, string assetPath, string name, int particleSystemCnt, bool isLooping)
        {
            _guid = guid;
            _assetPath = assetPath;
            _name = name;
            _particleSystemCnt = particleSystemCnt;
            _isLooping = isLooping;
        }
    }
}
