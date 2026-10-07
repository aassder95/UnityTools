using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace UnityTools.Build.Editor
{
    public static class BuildPresetRunner
    {
        //============================================================
        // Logic
        //============================================================
        public static bool TryPrepare(BuildPreset preset, out BuildPlayerOptions options, out string error)
        {
            options = default;
            error = null;
            if (preset == null)
            {
                error = "빌드 프리셋을 선택하세요.";
                return false;
            }

            BuildTarget target = preset.Target;
            if (target != BuildTarget.Android && target != BuildTarget.iOS && target != BuildTarget.StandaloneWindows64)
            {
                error = "지원 대상은 Windows64, Android, iOS입니다.";
                return false;
            }

            SceneAsset[] scenes = preset.Scenes;
            if (scenes == null || scenes.Length == 0)
            {
                error = "빌드 Scene을 지정하세요.";
                return false;
            }

            string[] paths = new string[scenes.Length];
            var seenScenes = new HashSet<string>(StringComparer.Ordinal);
            for (int idx = 0; idx < scenes.Length; idx++)
            {
                paths[idx] = AssetDatabase.GetAssetPath(scenes[idx]);
                if (string.IsNullOrEmpty(paths[idx]) || !paths[idx].StartsWith("Assets/", StringComparison.Ordinal) || !paths[idx].EndsWith(".unity", StringComparison.OrdinalIgnoreCase) || !seenScenes.Add(paths[idx]))
                {
                    error = "Scene 연결·중복·Assets 경로를 확인하세요: " + idx;
                    return false;
                }
            }

            string[] defines = preset.Defines;
            var seenDefines = new HashSet<string>(StringComparer.Ordinal);
            foreach (string define in defines)
            {
                if (string.IsNullOrEmpty(define) || !Regex.IsMatch(define, "^[A-Za-z_][A-Za-z0-9_]*$") || !seenDefines.Add(define))
                {
                    error = "Define 이름 또는 중복을 확인하세요.";
                    return false;
                }
            }

            if (preset.HasScriptDebugging && !preset.IsDevelopment)
            {
                error = "Script Debugging에는 Development Build가 필요합니다.";
                return false;
            }

            string output = preset.OutputPath;
            if (string.IsNullOrWhiteSpace(output) || Path.IsPathRooted(output) || output.Contains("\\") || !output.StartsWith("Builds/", StringComparison.Ordinal))
            {
                error = "출력은 프로젝트 내부 Builds/ 경로여야 합니다.";
                return false;
            }

            string fullPath;
            try
            {
                string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../Builds")) + Path.DirectorySeparatorChar;
                fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", output));
                if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                {
                    error = "출력 경로가 Builds 폴더를 벗어납니다.";
                    return false;
                }
            }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException || ex is PathTooLongException)
            {
                error = "출력 경로가 유효하지 않습니다: " + ex.Message;
                return false;
            }

            string extension = Path.GetExtension(fullPath);
            bool hasExpectedExtension = target == BuildTarget.iOS ? string.IsNullOrEmpty(extension) : target == BuildTarget.Android ? extension == (preset.IsAndroidBundle ? ".aab" : ".apk") : extension == ".exe";
            if (!hasExpectedExtension)
            {
                error = "출력 확장자는 Windows .exe, Android .apk/.aab, iOS 폴더여야 합니다.";
                return false;
            }

            options = new BuildPlayerOptions
            {
                scenes = paths,
                target = target,
                locationPathName = fullPath,
                extraScriptingDefines = defines,
                options = (preset.IsDevelopment ? BuildOptions.Development : BuildOptions.None) | (preset.HasScriptDebugging ? BuildOptions.AllowDebugging : BuildOptions.None)
            };
            return true;
        }

        public static bool TryBuild(BuildPreset preset, out BuildReport report, out string error)
        {
            report = null;
            if (!TryPrepare(preset, out BuildPlayerOptions options, out error))
                return false;

            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || BuildPipeline.isBuildingPlayer || EditorUserBuildSettings.activeBuildTarget != options.target)
            {
                error = "Play Mode·컴파일·다른 빌드를 종료하고 프리셋의 플랫폼으로 먼저 전환하세요.";
                return false;
            }

            bool wasBundle = EditorUserBuildSettings.buildAppBundle;
            try
            {
                if (options.target == BuildTarget.Android)
                    EditorUserBuildSettings.buildAppBundle = preset.IsAndroidBundle;

                Directory.CreateDirectory(Path.GetDirectoryName(options.locationPathName));
                report = BuildPipeline.BuildPlayer(options);
                if (report == null || report.summary.result != BuildResult.Succeeded)
                {
                    error = "빌드 실패: " + (report == null ? "보고서 없음" : report.summary.result.ToString());
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                error = "빌드 실행 실패: " + ex.Message;
                return false;
            }
            finally
            {
                EditorUserBuildSettings.buildAppBundle = wasBundle;
            }
        }
    }
}
