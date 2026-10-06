using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Vfx.Editor
{
    public class VfxThumbnailIndex : IDisposable
    {
        //============================================================
        // Constants
        //============================================================
        public const int MAX_TEXTURE_CNT = 24;

        //============================================================
        // Readonly
        //============================================================
        private readonly Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();
        private readonly Dictionary<string, EVfxColor> _colors = new Dictionary<string, EVfxColor>();
        private readonly Queue<string> _textureGuids = new Queue<string>();

        //============================================================
        // Properties
        //============================================================
        public int TextureCnt => _textures.Count;
        public int AnalyzedCnt => _colors.Count;

        //============================================================
        // Logic
        //============================================================
        public bool TryCapture(string guid, GameObject prefab, float timeSec)
        {
            if (string.IsNullOrEmpty(guid) || prefab == null || float.IsNaN(timeSec) || float.IsInfinity(timeSec) || timeSec < 0.0f || timeSec > VfxPreviewSession.MAX_PREVIEW_SEC || SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
                return false;

            Texture2D texture;
            using (VfxPreviewSession preview = new VfxPreviewSession(prefab))
            {
                if (!preview.TrySeek(timeSec))
                    return false;

                preview.Fit();
                texture = preview.Capture(96, 96);
            }

            if (texture == null)
                return false;

            texture.hideFlags = HideFlags.HideAndDontSave;
            EVfxColor color;
            bool isAnalyzed = false;
            try
            {
                color = VfxColorAnalyzer.Analyze(texture.GetPixels(), texture.GetPixel(0, 0));
                isAnalyzed = true;
            }
            finally
            {
                if (!isAnalyzed)
                    UnityEngine.Object.DestroyImmediate(texture);
            }
            if (_textures.TryGetValue(guid, out Texture2D previous))
            {
                UnityEngine.Object.DestroyImmediate(previous);
            }
            else
            {
                if (_textures.Count >= MAX_TEXTURE_CNT)
                {
                    string oldest = _textureGuids.Dequeue();
                    UnityEngine.Object.DestroyImmediate(_textures[oldest]);
                    _textures.Remove(oldest);
                }

                _textureGuids.Enqueue(guid);
            }

            _textures[guid] = texture;
            _colors[guid] = color;
            return true;
        }

        public bool TryGetTexture(string guid, out Texture2D texture)
        {
            texture = null;
            return guid != null && _textures.TryGetValue(guid, out texture);
        }

        public EVfxColor ColorOf(string guid)
        {
            return guid != null && _colors.TryGetValue(guid, out EVfxColor color) ? color : EVfxColor.Unanalyzed;
        }

        public void Dispose()
        {
            foreach (Texture2D texture in _textures.Values)
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }

            _textures.Clear();
            _colors.Clear();
            _textureGuids.Clear();
        }
    }
}
