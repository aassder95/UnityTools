using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityTools.Vfx.Editor
{
    public class VfxThumbnailStorage
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly string _folderPath;

        //============================================================
        // Fields
        //============================================================
        private string _error = string.Empty;

        //============================================================
        // Properties
        //============================================================
        public string Error => _error;

        //============================================================
        // Constructors
        //============================================================
        public VfxThumbnailStorage(string folderPath = "Library/UnityTools/VfxThumbnails")
        {
            _folderPath = folderPath;
        }

        //============================================================
        // Persistence
        //============================================================
        public bool TryReadColor(string guid, string key, out EVfxColor color)
        {
            color = EVfxColor.Unanalyzed;
            if (!IsGuid(guid) || string.IsNullOrEmpty(key))
                return false;

            _error = string.Empty;
            try
            {
                string path = Path.Combine(_folderPath, guid + ".txt");
                if (!File.Exists(path))
                    return false;

                string[] lines = File.ReadAllLines(path);
                return lines.Length == 2 && lines[0] == key && Enum.TryParse(lines[1], out color) && Enum.IsDefined(typeof(EVfxColor), color) && color != EVfxColor.All && color != EVfxColor.Unanalyzed;
            }
            catch (IOException ex)
            {
                _error = "VFX 캐시 읽기에 실패했습니다: " + ex.Message;
            }
            catch (UnauthorizedAccessException ex)
            {
                _error = "VFX 캐시 읽기 권한이 없습니다: " + ex.Message;
            }

            return false;
        }

        public bool TryLoad(string guid, string key, out Texture2D texture, out EVfxColor color)
        {
            texture = null;
            if (!TryReadColor(guid, key, out color))
                return false;

            try
            {
                string path = Path.Combine(_folderPath, guid + ".png");
                if (!File.Exists(path))
                    return false;

                Texture2D candidate = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                candidate.hideFlags = HideFlags.HideAndDontSave;
                bool isLoaded = false;
                try
                {
                    isLoaded = candidate.LoadImage(File.ReadAllBytes(path)) && candidate.width == 96 && candidate.height == 96;
                    if (isLoaded)
                        texture = candidate;
                }
                finally
                {
                    if (!isLoaded)
                        UnityEngine.Object.DestroyImmediate(candidate);
                }

                return isLoaded;
            }
            catch (IOException ex)
            {
                _error = "VFX 썸네일 읽기에 실패했습니다: " + ex.Message;
            }
            catch (UnauthorizedAccessException ex)
            {
                _error = "VFX 썸네일 읽기 권한이 없습니다: " + ex.Message;
            }

            return false;
        }

        public void Save(string guid, string key, Texture2D texture, EVfxColor color)
        {
            if (!IsGuid(guid) || string.IsNullOrEmpty(key) || texture == null)
                return;

            _error = string.Empty;
            try
            {
                Directory.CreateDirectory(_folderPath);
                string imagePath = Path.Combine(_folderPath, guid + ".png");
                string infoPath = Path.Combine(_folderPath, guid + ".txt");
                // 메타데이터를 마지막에 기록하여 중단된 저장을 유효한 캐시로 읽지 않습니다.
                if (File.Exists(infoPath))
                    File.Delete(infoPath);

                File.WriteAllBytes(imagePath, texture.EncodeToPNG());
                File.WriteAllLines(infoPath, new[] { key, color.ToString() });
            }
            catch (IOException ex)
            {
                _error = "VFX 캐시 저장에 실패했습니다: " + ex.Message;
            }
            catch (UnauthorizedAccessException ex)
            {
                _error = "VFX 캐시 저장 권한이 없습니다: " + ex.Message;
            }
        }

        //============================================================
        // Logic
        //============================================================
        public static string BuildKey(string guid, float timeSec)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
            StringBuilder data = new StringBuilder("v2|96|");
            data.Append(Application.unityVersion).Append('|').Append(SystemInfo.graphicsDeviceType).Append('|').Append(QualitySettings.activeColorSpace).Append('|').Append(timeSec.ToString("R", CultureInfo.InvariantCulture));
            try
            {
                AppendDependencies(data, path);
                if (pipeline != null)
                {
                    data.Append('|').Append(pipeline.GetType().FullName);
                    AppendDependencies(data, AssetDatabase.GetAssetPath(pipeline));
                }
                else
                {
                    data.Append("|BuiltIn");
                }
            }
            catch (IOException)
            {
                return string.Empty;
            }
            catch (UnauthorizedAccessException)
            {
                return string.Empty;
            }

            return Hash128.Compute(data.ToString()).ToString();
        }

        //============================================================
        // Utilities
        //============================================================
        private static void AppendDependencies(StringBuilder data, string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            string[] paths = AssetDatabase.GetDependencies(path, true);
            Array.Sort(paths, StringComparer.Ordinal);
            using (SHA256 hash = SHA256.Create())
            {
                for (int idx = 0; idx < paths.Length; idx++)
                {
                    data.Append('|').Append(paths[idx]).Append('|').Append(AssetDatabase.GetAssetDependencyHash(paths[idx]));
                    if (!File.Exists(paths[idx]))
                        continue;

                    // 저장된 소스도 비교하여 import artifact에 아직 반영되지 않은 변경을 검출합니다.
                    using (FileStream stream = File.OpenRead(paths[idx]))
                    {
                        data.Append('|').Append(BitConverter.ToString(hash.ComputeHash(stream)));
                    }
                }
            }
        }

        private static bool IsGuid(string guid)
        {
            if (guid == null || guid.Length != 32)
                return false;

            for (int idx = 0; idx < guid.Length; idx++)
            {
                char ch = guid[idx];
                if (!(ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f'))
                    return false;
            }

            return true;
        }
    }
}
