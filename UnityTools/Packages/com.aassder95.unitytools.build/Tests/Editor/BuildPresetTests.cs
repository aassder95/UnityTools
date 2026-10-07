using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Build.Editor.Tests
{
    public class BuildPresetTests
    {
        //============================================================
        // Logic
        //============================================================
        [Test]
        public void PrepareRejectsTraversalAndDoesNotMutateGlobalDefines()
        {
            string folder = "Assets/BuildPreset_" + System.Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", folder.Substring(7));
            var preset = ScriptableObject.CreateInstance<BuildPreset>();
            try
            {
                string path = folder + "/scene.unity";
                System.IO.File.WriteAllText(path, "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n");
                AssetDatabase.ImportAsset(path);
                var serialized = new SerializedObject(preset);
                var scenes = serialized.FindProperty("_scenes");
                scenes.arraySize = 1;
                scenes.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                var defines = serialized.FindProperty("_defines");
                defines.arraySize = 1;
                defines.GetArrayElementAtIndex(0).stringValue = "QA";
                serialized.ApplyModifiedPropertiesWithoutUndo();
                string before = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
                Assert.That(BuildPresetRunner.TryPrepare(preset, out var options, out _), Is.True);
                Assert.That(options.extraScriptingDefines, Is.EqualTo(new[] { "QA" }));
                Assert.That(PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone), Is.EqualTo(before));
                serialized.FindProperty("_outputPath").stringValue = "Builds/../../outside.exe";
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.That(BuildPresetRunner.TryPrepare(preset, out _, out _), Is.False);
                serialized.FindProperty("_outputPath").stringValue = "Builds/QA/Game.exe";
                defines.GetArrayElementAtIndex(0).stringValue = "QA;RELEASE";
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.That(BuildPresetRunner.TryPrepare(preset, out _, out _), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(preset);
                AssetDatabase.DeleteAsset(folder);
            }
        }
    }
}
