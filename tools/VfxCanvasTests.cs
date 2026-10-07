using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityTools.Vfx.Editor;
using Object = UnityEngine.Object;

public class VfxCanvasTests
{
    //============================================================
    // Logic
    //============================================================
    [UnityTest]
    public IEnumerator CanvasShaderMasksAndPreviewScope()
    {
        string outputPath = File.ReadAllText("CanvasOutput.txt").Trim();
        bool isBuiltIn = File.ReadAllText("CanvasPipeline.txt").Trim() == "BuiltIn";
        RenderPipelineAsset pipeline = GraphicsSettings.defaultRenderPipeline;
        RenderPipelineAsset qualityPipeline = QualitySettings.renderPipeline;
        GameObject goCanvas = null;
        GameObject goCamera = null;
        RenderTexture target = null;
        try
        {
            if (isBuiltIn)
            {
                GraphicsSettings.defaultRenderPipeline = null;
                QualitySettings.renderPipeline = null;
            }

            Assert.That(GraphicsSettings.currentRenderPipeline == null, Is.EqualTo(isBuiltIn));
            Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/ProjectVfx/3.Resources/Materials/UIAdditive.mat");
            Assert.That(material, Is.Not.Null);
            Assert.That(material.shader.name, Is.EqualTo("Molip/UI_Additive"));
            goCamera = new GameObject("Canvas validation camera", typeof(Camera));
            Camera camera = goCamera.GetComponent<Camera>();
            camera.transform.position = new Vector3(0.0f, 0.0f, -5.0f);
            camera.orthographic = true;
            camera.orthographicSize = 2.0f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.cullingMask = 1 << 30;
            target = new RenderTexture(256, 256, 24);
            camera.targetTexture = target;
            goCanvas = new GameObject("Canvas validation", typeof(RectTransform), typeof(Canvas));
            goCanvas.layer = 30;
            Canvas canvas = goCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = camera;
            goCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(4.0f, 4.0f);
            GameObject goMask = new GameObject("Mask", typeof(RectTransform), typeof(Image));
            goMask.layer = 30;
            goMask.transform.SetParent(goCanvas.transform, false);
            goMask.GetComponent<RectTransform>().sizeDelta = Vector2.one;
            goMask.GetComponent<Image>().color = Color.white;
            GameObject goImage = new GameObject("Additive image", typeof(RectTransform), typeof(Image));
            goImage.layer = 30;
            goImage.transform.SetParent(goMask.transform, false);
            goImage.GetComponent<RectTransform>().sizeDelta = new Vector2(2.0f, 2.0f);
            Image image = goImage.GetComponent<Image>();
            image.color = Color.green;
            image.material = material;
            yield return null;
            yield return null;
            Canvas.ForceUpdateCanvases();
            int unmaskedCnt = Capture(camera, outputPath, "CanvasUnmasked");
            Assert.That(unmaskedCnt, Is.GreaterThan(1000));

            Mask mask = goMask.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            yield return null;
            yield return null;
            Canvas.ForceUpdateCanvases();
            Assert.That(image.materialForRendering.GetFloat("_StencilComp"), Is.EqualTo((float)CompareFunction.Equal));
            int stencilCnt = Capture(camera, outputPath, "CanvasStencil");
            Assert.That(stencilCnt, Is.InRange(unmaskedCnt / 8, unmaskedCnt / 2));

            Object.DestroyImmediate(mask);
            goMask.GetComponent<Image>().enabled = false;
            goMask.AddComponent<RectMask2D>();
            yield return null;
            yield return null;
            Canvas.ForceUpdateCanvases();
            int rectCnt = Capture(camera, outputPath, "CanvasRectMask");
            Assert.That(rectCnt, Is.InRange(unmaskedCnt / 8, unmaskedCnt / 2));

            using (VfxPreviewSession preview = new VfxPreviewSession(goCanvas))
            {
                Texture2D texture = preview.Capture(96, 96);
                try
                {
                    Assert.That(VfxColorAnalyzer.Analyze(texture.GetPixels(), texture.GetPixel(0, 0)), Is.EqualTo(EVfxColor.Invisible));
                    File.WriteAllBytes(Path.Combine(outputPath, "CanvasPreviewExcluded.png"), texture.EncodeToPNG());
                }
                finally { Object.DestroyImmediate(texture); }
            }

            File.WriteAllText(Path.Combine(outputPath, "canvas-result.txt"), Application.unityVersion + " | " + (isBuiltIn ? "BuiltIn" : pipeline.GetType().Name) + " | unmasked=" + unmaskedCnt + " | stencil=" + stencilCnt + " | rect=" + rectCnt + " | Canvas preview excluded | Passed");
        }
        finally
        {
            if (goCamera != null)
                Object.DestroyImmediate(goCamera);

            if (goCanvas != null)
                Object.DestroyImmediate(goCanvas);

            if (target != null)
                Object.DestroyImmediate(target);

            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = qualityPipeline;
        }
    }

    //============================================================
    // Utilities
    //============================================================
    private static int Capture(Camera camera, string outputPath, string name)
    {
        camera.Render();
        RenderTexture previous = RenderTexture.active;
        Texture2D texture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
        try
        {
            RenderTexture.active = camera.targetTexture;
            texture.ReadPixels(new Rect(0.0f, 0.0f, 256.0f, 256.0f), 0, 0);
            texture.Apply();
            File.WriteAllBytes(Path.Combine(outputPath, name + ".png"), texture.EncodeToPNG());
            Color[] pixels = texture.GetPixels();
            int cnt = 0;
            for (int idx = 0; idx < pixels.Length; idx++)
            {
                Color pixel = pixels[idx];
                if (pixel.g > 0.4f && pixel.g > pixel.r * 2.0f && pixel.g > pixel.b * 2.0f)
                    ++cnt;
            }

            return cnt;
        }
        finally
        {
            RenderTexture.active = previous;
            Object.DestroyImmediate(texture);
        }
    }
}
