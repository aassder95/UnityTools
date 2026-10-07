using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityTools.Vfx.Editor;

public static class VfxUrpValidation
{
    //============================================================
    // Logic
    //============================================================
    public static void Configure()
    {
        UniversalRendererData renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
        AssetDatabase.CreateAsset(renderer, "Assets/VfxValidationRenderer.asset");
        UniversalRenderPipelineAsset pipeline = UniversalRenderPipelineAsset.Create(renderer);
        AssetDatabase.CreateAsset(pipeline, "Assets/VfxValidationPipeline.asset");
        GraphicsSettings.defaultRenderPipeline = pipeline;
        QualitySettings.renderPipeline = pipeline;
        AssetDatabase.SaveAssets();
    }

    public static void Validate()
    {
        string outputPath = File.ReadAllText("UrpOutput.txt").Trim();
        if (!(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset))
        {
            Debug.LogError("URP 검증 파이프라인이 활성화되지 않았습니다.");
            EditorApplication.Exit(1);
            return;
        }

        StringBuilder report = new StringBuilder();
        report.AppendLine(Application.unityVersion + " | " + UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(UniversalRenderPipelineAsset).Assembly).version);
        Material material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        GameObject go = new GameObject("URP green probe", typeof(ParticleSystem));
        try
        {
            ParticleSystem ps = go.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.startColor = Color.green;
            main.startSize = 0.5f;
            ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            using (VfxThumbnailIndex thumbnails = new VfxThumbnailIndex())
            {
                if (!thumbnails.TryCapture("urp", go, 1.0f) || !thumbnails.TryGetTexture("urp", out Texture2D texture) || thumbnails.ColorOf("urp") != EVfxColor.Green)
                {
                    Debug.LogError("URP 파티클의 녹색 썸네일 검증에 실패했습니다.");
                    EditorApplication.Exit(1);
                    return;
                }

                File.WriteAllBytes(Path.Combine(outputPath, "UrpGreen.png"), texture.EncodeToPNG());
                report.AppendLine("URP Particles/Unlit | Green | Passed");
            }

            using (VfxPreviewSession preview = new VfxPreviewSession(go))
            {
                bool isPositioned = preview.TrySeek(1.0f);
                preview.Orbit(new Vector2(20.0f, 10.0f));
                preview.Zoom(-1.0f);
                Texture2D texture = preview.Capture(192, 192);
                try
                {
                    if (!isPositioned || VfxColorAnalyzer.Analyze(texture.GetPixels(), texture.GetPixel(0, 0)) != EVfxColor.Green)
                    {
                        Debug.LogError("URP 시간 탐색·카메라 조작 후 캡처 검증에 실패했습니다.");
                        EditorApplication.Exit(1);
                        return;
                    }

                    File.WriteAllBytes(Path.Combine(outputPath, "UrpOrbit.png"), texture.EncodeToPNG());
                    report.AppendLine("URP seek/orbit/zoom | Green | Passed");
                }
                finally { Object.DestroyImmediate(texture); }
            }

            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets/ProjectVfx" }))
            {
                Material legacy = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if (legacy.shader.name != "Molip/UI_Additive")
                    continue;

                renderer.sharedMaterial = legacy;
                using (VfxThumbnailIndex thumbnails = new VfxThumbnailIndex())
                {
                    bool isCaptured = thumbnails.TryCapture("legacy", go, 1.0f);
                    report.AppendLine("Molip/UI_Additive | " + thumbnails.ColorOf("legacy") + " | observed, shader supported=" + legacy.shader.isSupported);
                    if (isCaptured && thumbnails.TryGetTexture("legacy", out Texture2D texture))
                        File.WriteAllBytes(Path.Combine(outputPath, "LegacyShader.png"), texture.EncodeToPNG());
                }
            }

            using (VfxThumbnailIndex thumbnails = new VfxThumbnailIndex())
            {
                foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/ProjectVfx" }))
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                    bool isCaptured = thumbnails.TryCapture(guid, prefab, 1.0f);
                    report.AppendLine(prefab.name + " | " + thumbnails.ColorOf(guid) + " | observed");
                    if (isCaptured && thumbnails.TryGetTexture(guid, out Texture2D texture))
                        File.WriteAllBytes(Path.Combine(outputPath, prefab.name + ".png"), texture.EncodeToPNG());
                }
            }
        }
        finally
        {
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(material);
        }

        File.WriteAllText(Path.Combine(outputPath, "result.txt"), report.ToString());
    }

    public static void BuildPlayer()
    {
        string outputPath = File.ReadAllText("UrpOutput.txt").Trim();
        if (!(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset))
        {
            Debug.LogError("URP 빌드 파이프라인이 활성화되지 않았습니다.");
            EditorApplication.Exit(1);
            return;
        }

        CompatibilityValidation.PrepareScenes();
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int idx = 0; idx < scenes.Length; idx++)
        {
            scenes[idx] = EditorBuildSettings.scenes[idx].path;
        }

        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = Path.Combine(outputPath, "Build/Compatibility.exe"),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        });
        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError("URP Windows 빌드 실패: " + report.summary.result);
            EditorApplication.Exit(1);
            return;
        }

        File.WriteAllText(Path.Combine(outputPath, "build-result.txt"), Application.unityVersion + " | URP | " + report.summary.result + " | " + report.summary.totalSize + " bytes");
    }
}
