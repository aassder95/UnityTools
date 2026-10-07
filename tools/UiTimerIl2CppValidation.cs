using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityTools.Ui;

public static class UiTimerIl2CppValidation
{
    //============================================================
    // Logic
    //============================================================
    public static void BuildPlayer()
    {
        string outputPath = File.ReadAllText("UiTimerOutput.txt").Trim();
        bool isAndroid = File.ReadAllText("UiTimerTarget.txt").Trim() == "Android";
        bool isIl2Cpp = File.ReadAllText("UiTimerBackend.txt").Trim() == "IL2CPP";
        BuildTargetGroup group = isAndroid ? BuildTargetGroup.Android : BuildTargetGroup.Standalone;
        PlayerSettings.SetScriptingBackend(group, isIl2Cpp ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);
        PlayerSettings.SetManagedStrippingLevel(group, isIl2Cpp ? ManagedStrippingLevel.High : ManagedStrippingLevel.Disabled);
        PlayerSettings.SetApplicationIdentifier(group, "com.unitytools.uitimervalidation");
        if (isAndroid)
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EventSystem eventSystem = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
        GameObject goButton = new GameObject("RepeatButton", typeof(RectTransform), typeof(Image), typeof(UiRepeatButton), typeof(UiButtonPressScale));
        UiRepeatButton button = goButton.GetComponent<UiRepeatButton>();
        button.targetGraphic = goButton.GetComponent<Image>();
        UiTimerPlayerProbe probe = new GameObject("PlayerProbe").AddComponent<UiTimerPlayerProbe>();
        SerializedObject settings = new SerializedObject(probe);
        settings.FindProperty("_btnRepeat").objectReferenceValue = button;
        settings.FindProperty("_pressScale").objectReferenceValue = goButton.GetComponent<UiButtonPressScale>();
        settings.FindProperty("_eventSystem").objectReferenceValue = eventSystem;
        settings.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(scene, "Assets/UiTimerProbe.unity");
        List<string> scenes = new List<string> { "Assets/UiTimerProbe.unity" };
        foreach (EditorBuildSettingsScene entry in EditorBuildSettings.scenes)
        {
            if (entry.enabled && entry.path != "Assets/UiTimerProbe.unity")
                scenes.Add(entry.path);
        }

        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes.ToArray(),
            locationPathName = Path.Combine(outputPath, isAndroid ? "UiTimer.apk" : "UiTimer.exe"),
            target = isAndroid ? BuildTarget.Android : BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        });
        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError("UI·Timer 검증 빌드 실패: " + report.summary.result);
            EditorApplication.Exit(1);
            return;
        }

        string backend = isIl2Cpp ? "IL2CPP" : "Mono";
        string stripping = PlayerSettings.GetManagedStrippingLevel(group).ToString();
        File.WriteAllText(Path.Combine(outputPath, "build-result.txt"), Application.unityVersion + " | " + backend + " | " + stripping + " stripping | Succeeded | " + report.summary.totalSize + " bytes");
    }
}
