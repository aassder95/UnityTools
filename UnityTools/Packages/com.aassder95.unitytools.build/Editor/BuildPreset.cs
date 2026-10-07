using System;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Build.Editor
{
    [CreateAssetMenu(menuName = "UnityTools/Build/Preset")]
    public class BuildPreset : ScriptableObject
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Player")]
        [SerializeField] private BuildTarget _target = BuildTarget.StandaloneWindows64;
        [SerializeField] private SceneAsset[] _scenes = Array.Empty<SceneAsset>();
        [SerializeField] private string _outputPath = "Builds/QA/Game.exe";
        [Header("Build Mode")]
        [SerializeField] private bool _isDevelopment = true;
        [SerializeField] private bool _hasScriptDebugging;
        [SerializeField] private bool _isAndroidBundle;
        [SerializeField] private string[] _defines = Array.Empty<string>();

        //============================================================
        // Properties
        //============================================================
        public BuildTarget Target => _target;
        public string OutputPath => _outputPath;
        public bool IsDevelopment => _isDevelopment;
        public bool HasScriptDebugging => _hasScriptDebugging;
        public bool IsAndroidBundle => _isAndroidBundle;
        public SceneAsset[] Scenes => _scenes == null ? Array.Empty<SceneAsset>() : (SceneAsset[])_scenes.Clone();
        public string[] Defines => _defines == null ? Array.Empty<string>() : (string[])_defines.Clone();
    }
}
