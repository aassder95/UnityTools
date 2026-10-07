using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Build.Editor
{
    public class BuildPresetWindow : EditorWindow
    {
        //============================================================
        // Fields
        //============================================================
        private BuildPreset _preset;
        private string _status;

        //============================================================
        // Init/Register
        //============================================================
        [MenuItem("Tools/UnityTools/Build/Preset Runner")]
        public static void Open()
        {
            GetWindow<BuildPresetWindow>("Build Preset");
        }

        //============================================================
        // Unity Methods
        //============================================================
        private void OnGUI()
        {
            _preset = (BuildPreset)EditorGUILayout.ObjectField("Preset", _preset, typeof(BuildPreset), false);
            bool isValid = BuildPresetRunner.TryPrepare(_preset, out BuildPlayerOptions options, out string error);
            EditorGUILayout.HelpBox(isValid ? options.target + " / " + options.locationPathName + " / " + options.scenes.Length + " scenes" : error, isValid ? MessageType.Info : MessageType.Warning);
            using (new EditorGUI.DisabledScope(!isValid))
            {
                if (GUILayout.Button("Build"))
                {
                    bool hasOutput = File.Exists(options.locationPathName) || Directory.Exists(options.locationPathName);
                    if (hasOutput && !EditorUtility.DisplayDialog("기존 빌드 출력", "기존 출력에 빌드를 실행하시겠습니까?\n" + options.locationPathName, "빌드", "취소"))
                        return;

                    bool isBuilt = BuildPresetRunner.TryBuild(_preset, out var report, out error);
                    _status = isBuilt ? "빌드 완료: " + report.summary.totalSize + " bytes" : error;
                }
            }

            if (!string.IsNullOrEmpty(_status))
                EditorGUILayout.HelpBox(_status, MessageType.Info);
        }
    }
}
