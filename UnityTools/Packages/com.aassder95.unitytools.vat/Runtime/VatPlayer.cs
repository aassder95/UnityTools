using UnityEngine;

namespace UnityTools.Vat
{
    [DisallowMultipleComponent]
    public class VatPlayer : MonoBehaviour
    {
        //============================================================
        // Readonly
        //============================================================
        private static readonly int _positionsId = Shader.PropertyToID("_VatPositions");
        private static readonly int _frameId = Shader.PropertyToID("_VatFrame");

        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private VatClip _clip;
        [SerializeField] private bool _shouldLoop = true;
        [SerializeField] private bool _hasUnscaledTime;

        //============================================================
        // Fields
        //============================================================
        private MaterialPropertyBlock _properties;
        private float _timeSec;
        private float _speed = 1.0f;
        private bool _isPlaying;

        //============================================================
        // Properties
        //============================================================
        public float TimeSec => _timeSec;
        public bool IsPlaying => _isPlaying;

        //============================================================
        // Unity Methods
        //============================================================
        private void Update()
        {
            if (!_isPlaying)
                return;

            float elapsedSec = _timeSec + (_hasUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) * _speed;
            if (_shouldLoop)
            {
                _timeSec = elapsedSec % _clip.DurationSec;
            }
            else
            {
                _timeSec = Mathf.Min(elapsedSec, _clip.DurationSec);
                _isPlaying = _timeSec < _clip.DurationSec;
            }

            ApplyFrame();
        }

        private void OnDisable()
        {
            _isPlaying = false;
        }

        private void OnDestroy()
        {
            Release();
        }

        //============================================================
        // Init/Register
        //============================================================
        public void Init()
        {
            _properties = new MaterialPropertyBlock();
            _timeSec = 0.0f;
            _isPlaying = false;
            _meshFilter.sharedMesh = _clip.Mesh;
            ApplyFrame();
        }

        public void Release()
        {
            _isPlaying = false;
            _properties = null;
        }

        //============================================================
        // Logic
        //============================================================
        public bool TryPlay(float speed = 1.0f)
        {
            if (_properties == null || float.IsNaN(speed) || float.IsInfinity(speed) || speed < 0.0f)
                return false;

            _speed = speed;
            _isPlaying = true;
            return true;
        }

        public bool TrySeek(float timeSec)
        {
            if (_properties == null || float.IsNaN(timeSec) || float.IsInfinity(timeSec) || timeSec < 0.0f || timeSec > _clip.DurationSec)
                return false;

            _timeSec = timeSec;
            ApplyFrame();
            return true;
        }

        public void Pause()
        {
            _isPlaying = false;
        }

        //============================================================
        // Utilities
        //============================================================
        private void ApplyFrame()
        {
            _renderer.GetPropertyBlock(_properties);
            _properties.SetTexture(_positionsId, _clip.Positions);
            _properties.SetFloat(_frameId, _timeSec / _clip.DurationSec * (_clip.FrameCnt - 1));
            _renderer.SetPropertyBlock(_properties);
        }
    }
}
