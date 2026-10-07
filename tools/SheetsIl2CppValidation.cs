using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class SheetsIl2CppValidation
{
    //============================================================
    // Logic
    //============================================================
    public static void BuildPlayer()
    {
        string outputPath = File.ReadAllText("Il2CppOutput.txt").Trim();
        bool isAndroid = File.ReadAllText("Il2CppTarget.txt").Trim() == "Android";
        BuildTargetGroup group = isAndroid ? BuildTargetGroup.Android : BuildTargetGroup.Standalone;
        PlayerSettings.SetScriptingBackend(group, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetManagedStrippingLevel(group, ManagedStrippingLevel.High);
        if (isAndroid)
        {
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetApplicationIdentifier(group, "com.unitytools.sheetsvalidation");
        }

        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/SmokeScene.unity" },
            locationPathName = Path.Combine(outputPath, isAndroid ? "Sheets.apk" : "Sheets.exe"),
            target = isAndroid ? BuildTarget.Android : BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        });
        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError("Sheets IL2CPP 빌드 실패: " + report.summary.result);
            EditorApplication.Exit(1);
            return;
        }

        File.WriteAllText(Path.Combine(outputPath, "build-result.txt"), Application.unityVersion + " | IL2CPP | High stripping | " + report.summary.result + " | " + report.summary.totalSize + " bytes");
    }
}
