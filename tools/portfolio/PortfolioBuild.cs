using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityTools.Benchmark.Samples;
using UnityTools.Benchmark.Samples.Editor;

public static class PortfolioBuild
{
    //============================================================
    // Logic
    //============================================================
    public static void Build()
    {
        UiLabSceneBuilder.BuildValidationScene();
        UiLabController controller = Object.FindFirstObjectByType<UiLabController>();
        SerializedObject source = new SerializedObject(controller);
        PortfolioCapture capture = controller.gameObject.AddComponent<PortfolioCapture>();
        SerializedObject dest = new SerializedObject(capture);
        dest.FindProperty("_controller").objectReferenceValue = controller;
        GameObject goCamera = new GameObject("Portfolio Capture Camera", typeof(Camera));
        Camera camCapture = goCamera.GetComponent<Camera>();
        camCapture.clearFlags = CameraClearFlags.SolidColor;
        camCapture.backgroundColor = new Color(0.04f, 0.05f, 0.08f, 1.0f);
        camCapture.cullingMask = -1;
        camCapture.enabled = false;
        dest.FindProperty("_camCapture").objectReferenceValue = camCapture;
        dest.FindProperty("_canvas").objectReferenceValue = Object.FindFirstObjectByType<Canvas>();
        foreach (string name in new[] { "_inputItemCnt", "_inputPairCnt", "_scenarioChoice", "_orderChoice" })
        {
            dest.FindProperty(name).objectReferenceValue = source.FindProperty(name).objectReferenceValue;
        }

        source.FindProperty("_warmupFrames").intValue = 120;
        source.FindProperty("_sampleFrames").intValue = 600;
        source.FindProperty("_timeoutSec").floatValue = 300.0f;
        source.ApplyModifiedPropertiesWithoutUndo();
        dest.ApplyModifiedPropertiesWithoutUndo();
        PlayerSettings.runInBackground = true;
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        EditorSceneManager.SaveScene(controller.gameObject.scene);
        Directory.CreateDirectory("PortfolioBuild");
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/UiPerformanceLab.unity" },
            locationPathName = "PortfolioBuild/UnityTools-Portfolio.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        });
        File.WriteAllText("portfolio-build-result.txt", report.summary.result.ToString());
        if (report.summary.result != BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }
}
