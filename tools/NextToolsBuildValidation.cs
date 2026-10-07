using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityTools.Build.Editor;
using UnityTools.Vat;

public static class NextToolsBuildValidation
{
    //============================================================
    // Logic
    //============================================================
    public static void Build()
    {
        string before = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
        bool wasBundle = EditorUserBuildSettings.buildAppBundle;
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        var mesh = new Mesh();
        mesh.vertices = new[] { new Vector3(-0.5f, -0.5f, 0.0f), new Vector3(0.0f, 0.5f, 0.0f), new Vector3(0.5f, -0.5f, 0.0f) };
        mesh.triangles = new[] { 0, 1, 2 };
        mesh.uv = new[] { Vector2.zero, Vector2.up, Vector2.right };
        mesh.bounds = new Bounds(new Vector3(0.5f, 0.0f, 0.0f), new Vector3(2.0f, 1.0f, 1.0f));
        var positions = new Texture2D(3, 2, TextureFormat.RGBAFloat, false, true) { filterMode = FilterMode.Point };
        var normals = new Texture2D(3, 2, TextureFormat.RGBAFloat, false, true);
        var pixels = new Color[6];
        for (int idx = 0; idx < 3; idx++)
        {
            Vector3 vertex = mesh.vertices[idx];
            pixels[idx] = new Color(vertex.x, vertex.y, vertex.z, 1.0f);
            pixels[idx + 3] = new Color(vertex.x + 1.0f, vertex.y, vertex.z, 1.0f);
        }

        positions.SetPixels(pixels);
        positions.Apply();
        var clip = ScriptableObject.CreateInstance<VatClip>();
        if (!clip.TryConfigure(mesh, positions, normals, 1.0f, 2))
        {
            Debug.LogError("VAT 빌드 데이터 실패");
            EditorApplication.Exit(1);
            return;
        }

        AssetDatabase.CreateAsset(clip, "Assets/ProbeVat.asset");
        AssetDatabase.AddObjectToAsset(mesh, clip);
        AssetDatabase.AddObjectToAsset(positions, clip);
        AssetDatabase.AddObjectToAsset(normals, clip);
        var material = new Material(Shader.Find("UnityTools/VAT/Unlit"));
        AssetDatabase.CreateAsset(material, "Assets/ProbeVat.mat");
        var go = new GameObject("VAT Probe", typeof(MeshFilter), typeof(MeshRenderer), typeof(VatPlayer), typeof(NextToolsPlayerProbe));
        var renderer = go.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        var player = go.GetComponent<VatPlayer>();
        var playerData = new SerializedObject(player);
        playerData.FindProperty("_meshFilter").objectReferenceValue = go.GetComponent<MeshFilter>();
        playerData.FindProperty("_renderer").objectReferenceValue = renderer;
        playerData.FindProperty("_clip").objectReferenceValue = clip;
        playerData.FindProperty("_shouldLoop").boolValue = false;
        playerData.ApplyModifiedPropertiesWithoutUndo();
        var probeData = new SerializedObject(go.GetComponent<NextToolsPlayerProbe>());
        probeData.FindProperty("_player").objectReferenceValue = player;
        probeData.ApplyModifiedPropertiesWithoutUndo();
        var camera = new GameObject("Camera", typeof(Camera)).GetComponent<Camera>();
        camera.transform.position = new Vector3(0.5f, 0.0f, -4.0f);
        EditorSceneManager.SaveScene(scene, "Assets/PortsPlayer.unity");
        AssetDatabase.SaveAssets();
        var preset = ScriptableObject.CreateInstance<BuildPreset>();
        var data = new SerializedObject(preset);
        data.FindProperty("_outputPath").stringValue = "Builds/Ports/Ports.exe";
        var scenes = data.FindProperty("_scenes");
        scenes.arraySize = 1;
        scenes.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/PortsPlayer.unity");
        var defines = data.FindProperty("_defines");
        defines.arraySize = 1;
        defines.GetArrayElementAtIndex(0).stringValue = "UNITYTOOLS_PORTS_QA";
        data.ApplyModifiedPropertiesWithoutUndo();
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
        bool isBuilt = BuildPresetRunner.TryBuild(preset, out var report, out string error);
        if (!isBuilt || PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone) != before || EditorUserBuildSettings.buildAppBundle != wasBundle)
        {
            Debug.LogError("프리셋 빌드·설정 보존 검증 실패: " + error);
            EditorApplication.Exit(1);
            return;
        }

        File.WriteAllText("preset-build-result.txt", report.summary.result + " / " + report.summary.totalSize + " bytes");
    }
}
