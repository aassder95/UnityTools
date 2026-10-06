using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityTools.Ui;
using UnityTools.Ui.Samples.Buttons;
using UnityTools.Ui.Samples.Buttons.Editor;

public static class ButtonCompatibilityValidation
{
    public static void PrepareSample()
    {
        ButtonInputSampleBuilder.BuildScene();
        ButtonInputSample sample = UnityEngine.Object.FindObjectOfType<ButtonInputSample>();
        SerializedObject data = new SerializedObject(sample);
        string[] names = { "_btnRepeat", "_btnSingle", "_btnToggle", "_btnPause", "_txtStatus" };
        for (int idx = 0; idx < names.Length; idx++)
        {
            if (data.FindProperty(names[idx]).objectReferenceValue == null)
            {
                Debug.LogError("샘플 참조가 누락됐습니다: " + names[idx]);
                EditorApplication.Exit(1);
                return;
            }
        }

        UiRepeatButton btn = (UiRepeatButton)data.FindProperty("_btnRepeat").objectReferenceValue;
        UnityEditor.Editor inspector = UnityEditor.Editor.CreateEditor(btn);
        bool isInspectorValid = inspector.GetType().Name == "UiRepeatButtonInspector";
        UnityEngine.Object.DestroyImmediate(inspector);
        if (!isInspectorValid)
        {
            Debug.LogError("반복 버튼 Inspector가 연결되지 않았습니다.");
            EditorApplication.Exit(1);
            return;
        }

        File.WriteAllText("sample-result.txt", "Sample references and repeat Inspector: Passed");
        File.WriteAllText("Assets/ButtonPlayerSmoke.cs", PlayerSource());
        AssetDatabase.Refresh();
    }

    public static void AttachSmoke()
    {
        var scene = EditorSceneManager.OpenScene("Assets/ButtonInputSample/ButtonInputSample.unity");
        ButtonInputSample sample = UnityEngine.Object.FindObjectOfType<ButtonInputSample>();
        SerializedObject sampleData = new SerializedObject(sample);
        GameObject go = new GameObject("Player Smoke");
        Component smoke = go.AddComponent(Type.GetType("ButtonPlayerSmoke, Assembly-CSharp"));
        SerializedObject smokeData = new SerializedObject(smoke);
        smokeData.FindProperty("_btnRepeat").objectReferenceValue = sampleData.FindProperty("_btnRepeat").objectReferenceValue;
        smokeData.FindProperty("_btnSingle").objectReferenceValue = sampleData.FindProperty("_btnSingle").objectReferenceValue;
        smokeData.FindProperty("_sample").objectReferenceValue = sample;
        smokeData.FindProperty("_events").objectReferenceValue = UnityEngine.Object.FindObjectOfType<EventSystem>();
        smokeData.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(scene);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scene.path, true) };
    }

    private static string PlayerSource()
    {
        return @"using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityTools.Ui;
using UnityTools.Ui.Samples.Buttons;
public class ButtonPlayerSmoke : MonoBehaviour
{
    [SerializeField] private UiRepeatButton _btnRepeat;
    [SerializeField] private Button _btnSingle;
    [SerializeField] private ButtonInputSample _sample;
    [SerializeField] private EventSystem _events;
    private Coroutine _coValidate;
    private void OnEnable() { _coValidate = StartCoroutine(CoValidate()); }
    private void OnDisable() { if (_coValidate != null) StopCoroutine(_coValidate); _coValidate = null; }
    private IEnumerator CoValidate()
    {
        yield return null;
        Time.timeScale = 0.0f;
        PointerEventData pointer = new PointerEventData(_events) { pointerId = -1, button = PointerEventData.InputButton.Left };
        ExecuteEvents.Execute(_btnSingle.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(_btnSingle.gameObject, pointer, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(_btnSingle.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        if (_sample.SingleCnt != 1) { Finish(""일반 클릭 횟수가 일치하지 않습니다.""); yield break; }
        if (!_btnRepeat.TryConfigure(0.05f, 0.04f, 0.01f, 0.5f)) { Finish(""반복 설정에 실패했습니다.""); yield break; }
        ExecuteEvents.Execute(_btnRepeat.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        double timeoutSec = Time.unscaledTimeAsDouble + 3.0;
        while (_btnRepeat.RepeatCnt < 4 && Time.unscaledTimeAsDouble < timeoutSec) { yield return null; }
        ExecuteEvents.Execute(_btnRepeat.gameObject, pointer, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(_btnRepeat.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        if (_btnRepeat.RepeatCnt < 4 || _sample.RepeatCnt != _btnRepeat.RepeatCnt) { Finish(""반복 또는 release click 횟수가 일치하지 않습니다.""); yield break; }
        yield return new WaitForSecondsRealtime(0.15f);
        if (_btnRepeat.transform.localScale != Vector3.one) { Finish(""스케일을 복원하지 못했습니다.""); yield break; }
        int cnt = _sample.RepeatCnt;
        ExecuteEvents.Execute(_btnRepeat.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(_btnRepeat.gameObject, pointer, ExecuteEvents.pointerExitHandler);
        yield return new WaitForSecondsRealtime(0.1f);
        if (_sample.RepeatCnt != cnt || _btnRepeat.IsHeld) { Finish(""포인터 이탈 후 반복이 계속됐습니다.""); yield break; }
        _coValidate = null;
        Finish(string.Empty);
    }
    private void Finish(string error)
    {
        string[] args = Environment.GetCommandLineArgs();
        int idx = Array.IndexOf(args, ""--button-result"");
        if (idx >= 0 && idx + 1 < args.Length) File.WriteAllText(args[idx + 1], error.Length == 0 ? ""Passed"" : error);
        Application.Quit(error.Length == 0 ? 0 : 1);
    }
}";
    }
}
