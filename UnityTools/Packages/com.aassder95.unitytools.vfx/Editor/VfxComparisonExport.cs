using System;
using System.IO;
using UnityEngine;

namespace UnityTools.Vfx.Editor
{
    public static class VfxComparisonExport
    {
        //============================================================
        // Persistence
        //============================================================
        public static bool TrySave(VfxComparison comparison, string path, int sideSize, out string error)
        {
            error = string.Empty;
            if (comparison == null || !comparison.HasPreviews || string.IsNullOrWhiteSpace(path) || !string.Equals(Path.GetExtension(path), ".png", StringComparison.OrdinalIgnoreCase) || sideSize < 64 || sideSize > 1024)
            {
                error = "비교 미리보기와 PNG 경로, 64~1024 크기가 필요합니다.";
                return false;
            }

            Texture2D left = null;
            Texture2D right = null;
            Texture2D combined = null;
            try
            {
                left = comparison.Left.Capture(sideSize, sideSize);
                right = comparison.Right.Capture(sideSize, sideSize);
                if (left == null || right == null)
                {
                    error = "비교 이미지를 캡처하지 못했습니다.";
                    return false;
                }

                combined = new Texture2D(sideSize * 2, sideSize, TextureFormat.RGBA32, false);
                combined.SetPixels(0, 0, sideSize, sideSize, left.GetPixels());
                combined.SetPixels(sideSize, 0, sideSize, sideSize, right.GetPixels());
                combined.Apply();
                File.WriteAllBytes(path, combined.EncodeToPNG());
                return true;
            }
            catch (IOException ex)
            {
                error = "PNG 저장에 실패했습니다: " + ex.Message;
            }
            catch (UnauthorizedAccessException ex)
            {
                error = "PNG 저장 권한이 없습니다: " + ex.Message;
            }
            catch (UnityException ex)
            {
                error = "비교 이미지 렌더링에 실패했습니다: " + ex.Message;
            }
            finally
            {
                if (left != null)
                    UnityEngine.Object.DestroyImmediate(left);

                if (right != null)
                    UnityEngine.Object.DestroyImmediate(right);

                if (combined != null)
                    UnityEngine.Object.DestroyImmediate(combined);
            }

            return false;
        }
    }
}
