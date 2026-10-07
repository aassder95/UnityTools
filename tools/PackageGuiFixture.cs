using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityTools.Sheets.Editor;
using UnityTools.Vfx.Editor;

public static class PackageGuiFixture
{
    //============================================================
    // Logic
    //============================================================
    public static void Prepare()
    {
        Directory.CreateDirectory("Assets/GuiValidation");
        File.WriteAllText("Assets/GuiValidation/Window.csv", "Days,Ids\nMonday|Tuesday,1|2");
        Material material = new Material(Shader.Find("Particles/Standard Unlit"));
        AssetDatabase.CreateAsset(material, "Assets/GuiValidation/Orange.mat");
        for (int idx = 0; idx < 15; idx++)
        {
            GameObject go = new GameObject("GUI Spark " + idx.ToString("D2"), typeof(ParticleSystem));
            ParticleSystem ps = go.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.startColor = new Color(1.0f, 0.3f, 0.04f);
            main.startSize = 0.4f;
            main.loop = idx % 2 == 0;
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
            PrefabUtility.SaveAsPrefabAsset(go, "Assets/GuiValidation/" + go.name + ".prefab");
            Object.DestroyImmediate(go);
        }

        AssetDatabase.Refresh();
        File.WriteAllText("gui-fixture-result.txt", Application.unityVersion + " | CSV and 15 prefabs | Prepared");
    }

    [MenuItem("Tools/UnityTools/GUI Validation/Open Windows")]
    public static void Open()
    {
        CsvGeneratorWindow csv = ScriptableObject.CreateInstance<CsvGeneratorWindow>();
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(CsvGeneratorWindow).GetField("_csv", flags).SetValue(csv, AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/GuiValidation/Window.csv"));
        typeof(CsvGeneratorWindow).GetField("_className", flags).SetValue(csv, "GuiData");
        csv.titleContent = new GUIContent("CSV GUI Validation");
        csv.position = new Rect(100.0f, 100.0f, 700.0f, 650.0f);
        csv.ShowUtility();
        VfxBrowserWindow vfx = ScriptableObject.CreateInstance<VfxBrowserWindow>();
        typeof(VfxBrowserWindow).GetField("_rootPath", flags).SetValue(vfx, "Assets/GuiValidation");
        typeof(VfxBrowserWindow).GetField("_folder", flags).SetValue(vfx, AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets/GuiValidation"));
        typeof(VfxBrowserWindow).GetMethod("RefreshCatalog", flags).Invoke(vfx, null);
        vfx.titleContent = new GUIContent("VFX GUI Validation");
        vfx.position = new Rect(820.0f, 100.0f, 860.0f, 650.0f);
        vfx.ShowUtility();
    }
}
