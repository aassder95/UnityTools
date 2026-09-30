using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using UnityEditor.PackageManager.UI;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CompatibilityValidation
{
    //============================================================
    // Logic
    //============================================================
    public static void ImportSamples()
    {
        File.WriteAllText("editor-version.txt", Application.unityVersion);
        string[] requested = File.ReadAllLines("ValidationSamples.txt");
        List<string> report = new List<string>();
        foreach (string name in requested)
        {
            PackageInfo package = Array.Find(PackageInfo.GetAllRegisteredPackages(), item => item.name == name);
            if (package == null)
            {
                Fail("검증 대상 패키지가 없습니다: " + name);
                return;
            }

            int cnt = 0;
            foreach (Sample sample in Sample.FindByPackage(package.name, package.version))
            {
                if (!sample.Import(Sample.ImportOptions.OverridePreviousImports | Sample.ImportOptions.HideImportWindow))
                {
                    Fail("샘플 Import 실패: " + sample.displayName);
                    return;
                }

                report.Add(package.name + ": " + sample.displayName + " -> " + sample.importPath);
                ++cnt;
            }

            if (cnt == 0)
            {
                Fail("등록된 샘플이 없습니다: " + name);
                return;
            }
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        File.WriteAllLines("sample-imports.txt", report);
    }

    public static void PrepareScenes()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
        foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var scene = EditorSceneManager.OpenScene(path);
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform tr in root.GetComponentsInChildren<Transform>(true))
                {
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(tr.gameObject) > 0)
                    {
                        Fail("샘플 장면의 스크립트 참조가 누락됐습니다: " + path + " / " + tr.name);
                        return;
                    }
                }
            }

            scenes.Add(new EditorBuildSettingsScene(path, true));
        }

        if (scenes.Count == 0)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("SmokeScene");
            EditorSceneManager.SaveScene(scene, "Assets/SmokeScene.unity");
            scenes.Add(new EditorBuildSettingsScene("Assets/SmokeScene.unity", true));
        }

        EditorBuildSettings.scenes = scenes.ToArray();
        File.WriteAllText("scenes-ready.txt", scenes.Count.ToString());
    }

    public static void BuildPlayer()
    {
        List<string> scenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
                scenes.Add(scene.path);
        }

        Directory.CreateDirectory("Build");
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes.ToArray(),
            locationPathName = "Build/Compatibility.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        });
        if (report.summary.result != BuildResult.Succeeded)
        {
            Fail("Windows Development Build 실패: " + report.summary.result);
            return;
        }

        File.WriteAllText("build-result.txt", Application.unityVersion + " | " + report.summary.result + " | " + report.summary.totalSize + " bytes");
    }

    //============================================================
    // Utilities
    //============================================================
    private static void Fail(string message)
    {
        Debug.LogError(message);
        EditorApplication.Exit(1);
    }
}
