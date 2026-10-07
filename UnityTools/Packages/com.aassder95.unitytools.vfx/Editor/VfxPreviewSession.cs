using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Vfx.Editor
{
    public class VfxPreviewSession : IDisposable
    {
        //============================================================
        // Constants
        //============================================================
        public const float MAX_PREVIEW_SEC = 30.0f;

        //============================================================
        // Readonly
        //============================================================
        private readonly Dictionary<Transform, Transform> _transforms = new Dictionary<Transform, Transform>();
        private readonly Dictionary<ParticleSystem, ParticleSystem> _systems = new Dictionary<ParticleSystem, ParticleSystem>();
        private readonly List<Renderer> _renderers = new List<Renderer>();
        private readonly HashSet<ParticleSystem> _subSystems = new HashSet<ParticleSystem>();

        //============================================================
        // Fields
        //============================================================
        private PreviewRenderUtility _utility;
        private GameObject _goRoot;
        private float _timeSec;
        private float _yaw = 35.0f;
        private float _pitch = 20.0f;
        private float _zoom = 1.0f;
        private Bounds _bounds;
        private bool _isDisposed;

        //============================================================
        // Properties
        //============================================================
        public float TimeSec => _timeSec;
        public int ParticleCnt
        {
            get
            {
                int cnt = 0;
                foreach (ParticleSystem system in _systems.Values)
                {
                    cnt += system.particleCount;
                }

                return cnt;
            }
        }

        //============================================================
        // Constructors
        //============================================================
        public VfxPreviewSession(GameObject prefab)
        {
            _utility = new PreviewRenderUtility(true);
            _goRoot = EditorUtility.CreateGameObjectWithHideFlags("VFX Preview", HideFlags.HideAndDontSave);
            _utility.AddSingleGO(_goRoot);
            CloneVisuals(prefab.transform, _goRoot.transform);
            _transforms[prefab.transform].gameObject.SetActive(true);
            RemapSystems();
            bool isPositioned = TrySeek(0.1f);
            if (isPositioned)
                Fit();
        }

        //============================================================
        // Init/Register
        //============================================================
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _utility.Cleanup();
            _utility = null;
            _goRoot = null;
            _transforms.Clear();
            _systems.Clear();
            _renderers.Clear();
            _subSystems.Clear();
        }

        //============================================================
        // Logic
        //============================================================
        public bool TrySeek(float timeSec)
        {
            if (_isDisposed || float.IsNaN(timeSec) || float.IsInfinity(timeSec) || timeSec < 0.0f || timeSec > MAX_PREVIEW_SEC)
                return false;

            foreach (ParticleSystem system in _systems.Values)
            {
                system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            // sub-emitter는 부모의 시뮬레이션에서 실행되므로 직접 중복 실행하지 않습니다.
            foreach (ParticleSystem system in _systems.Values)
            {
                if (system.gameObject.activeInHierarchy && !_subSystems.Contains(system))
                    system.Simulate(timeSec, false, true, true);
            }

            _timeSec = timeSec;
            return true;
        }

        public void Advance(float deltaSec)
        {
            if (_isDisposed || deltaSec <= 0.0f || float.IsNaN(deltaSec) || float.IsInfinity(deltaSec))
                return;

            float stepSec = Mathf.Min(deltaSec, MAX_PREVIEW_SEC - _timeSec);
            foreach (ParticleSystem system in _systems.Values)
            {
                if (system.gameObject.activeInHierarchy && !_subSystems.Contains(system))
                    system.Simulate(stepSec, false, false, true);
            }

            _timeSec += stepSec;
        }

        public void Fit()
        {
            bool hasBounds = false;
            for (int idx = 0; idx < _renderers.Count; idx++)
            {
                Renderer renderer = _renderers[idx];
                if (!renderer.enabled || !renderer.gameObject.activeInHierarchy)
                    continue;

                if (hasBounds)
                {
                    _bounds.Encapsulate(renderer.bounds);
                }
                else
                {
                    _bounds = renderer.bounds;
                    hasBounds = true;
                }
            }

            if (!hasBounds || _bounds.size.sqrMagnitude < 0.001f)
                _bounds = new Bounds(Vector3.zero, Vector3.one);

            _zoom = 1.0f;
        }

        public void Orbit(Vector2 delta)
        {
            _yaw += delta.x;
            _pitch = Mathf.Clamp(_pitch + delta.y, -85.0f, 85.0f);
        }

        public void FitTogether(VfxPreviewSession peer)
        {
            if (_isDisposed || peer == null || peer._isDisposed)
                return;

            Fit();
            peer.Fit();
            _bounds.Encapsulate(peer._bounds);
            peer._bounds = _bounds;
            peer._yaw = _yaw;
            peer._pitch = _pitch;
        }

        public void Zoom(float delta)
        {
            _zoom = Mathf.Clamp(_zoom * Mathf.Exp(delta * 0.08f), 0.2f, 5.0f);
        }

        public Texture Render(Rect rect)
        {
            _utility.BeginPreview(rect, GUIStyle.none);
            ConfigureCamera(rect);
            _utility.Render(true);
            return _utility.EndPreview();
        }

        public Texture2D Capture(int width, int height)
        {
            Rect rect = new Rect(0.0f, 0.0f, width, height);
            _utility.BeginStaticPreview(rect);
            ConfigureCamera(rect);
            _utility.Render(true);
            return _utility.EndStaticPreview();
        }

        //============================================================
        // Utilities
        //============================================================
        private void CloneVisuals(Transform trSource, Transform trParent)
        {
            GameObject go = EditorUtility.CreateGameObjectWithHideFlags(trSource.name, HideFlags.HideAndDontSave);
            go.transform.SetParent(trParent, false);
            go.transform.localPosition = trSource.localPosition;
            go.transform.localRotation = trSource.localRotation;
            go.transform.localScale = trSource.localScale;
            _transforms.Add(trSource, go.transform);
            if (trSource.TryGetComponent(out ParticleSystem sourceSystem))
            {
                ParticleSystem system = go.AddComponent<ParticleSystem>();
                EditorUtility.CopySerialized(sourceSystem, system);
                ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
                EditorUtility.CopySerialized(trSource.GetComponent<ParticleSystemRenderer>(), renderer);
                _systems.Add(sourceSystem, system);
                _renderers.Add(renderer);
            }

            if (trSource.TryGetComponent(out MeshFilter sourceMesh))
            {
                EditorUtility.CopySerialized(sourceMesh, go.AddComponent<MeshFilter>());
            }

            if (trSource.TryGetComponent(out MeshRenderer sourceRenderer))
            {
                MeshRenderer renderer = go.AddComponent<MeshRenderer>();
                EditorUtility.CopySerialized(sourceRenderer, renderer);
                _renderers.Add(renderer);
            }

            if (trSource.TryGetComponent(out SpriteRenderer sourceSprite))
            {
                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                EditorUtility.CopySerialized(sourceSprite, renderer);
                _renderers.Add(renderer);
            }

            go.SetActive(trSource.gameObject.activeSelf);
            for (int idx = 0; idx < trSource.childCount; idx++)
            {
                CloneVisuals(trSource.GetChild(idx), go.transform);
            }
        }

        private void RemapSystems()
        {
            foreach (KeyValuePair<ParticleSystem, ParticleSystem> pair in _systems)
            {
                ParticleSystem.MainModule main = pair.Value.main;
                main.playOnAwake = false;
                main.stopAction = ParticleSystemStopAction.None;
                main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
                pair.Value.useAutoRandomSeed = false;
                pair.Value.randomSeed = 1;
                if (main.customSimulationSpace != null && _transforms.TryGetValue(main.customSimulationSpace, out Transform trSpace))
                    main.customSimulationSpace = trSpace;

                ParticleSystem.SubEmittersModule emitters = pair.Value.subEmitters;
                for (int idx = emitters.subEmittersCount - 1; idx >= 0; idx--)
                {
                    if (_systems.TryGetValue(emitters.GetSubEmitterSystem(idx), out ParticleSystem system))
                    {
                        emitters.SetSubEmitterSystem(idx, system);
                        if (emitters.enabled)
                            _subSystems.Add(system);
                    }
                    else
                    {
                        emitters.RemoveSubEmitter(idx);
                    }
                }

                ParticleSystem.ShapeModule shape = pair.Value.shape;
                if (shape.meshRenderer != null && _transforms.TryGetValue(shape.meshRenderer.transform, out Transform trMesh))
                    shape.meshRenderer = trMesh.GetComponent<MeshRenderer>();

                if (shape.spriteRenderer != null && _transforms.TryGetValue(shape.spriteRenderer.transform, out Transform trSprite))
                    shape.spriteRenderer = trSprite.GetComponent<SpriteRenderer>();
            }
        }

        private void ConfigureCamera(Rect rect)
        {
            Camera camera = _utility.camera;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.06f, 0.07f, 0.1f, 1.0f);
            camera.fieldOfView = 35.0f;
            float aspect = Mathf.Max(0.1f, rect.width / Mathf.Max(1.0f, rect.height));
            float halfFov = Mathf.Atan(Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f) * Mathf.Min(1.0f, aspect));
            float radius = Mathf.Max(0.5f, _bounds.extents.magnitude);
            float dist = radius / Mathf.Sin(halfFov) * _zoom;
            Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0.0f);
            camera.transform.SetPositionAndRotation(_bounds.center - rot * Vector3.forward * dist, rot);
            camera.nearClipPlane = Mathf.Max(0.01f, dist - radius * 2.0f);
            camera.farClipPlane = dist + radius * 2.0f + 10.0f;
            _utility.lights[0].intensity = 1.2f;
            _utility.lights[0].transform.rotation = Quaternion.Euler(40.0f, 40.0f, 0.0f);
            _utility.lights[1].intensity = 0.7f;
        }
    }
}
