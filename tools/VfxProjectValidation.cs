using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityTools.Vfx.Editor;

public static class VfxProjectValidation
{
    //============================================================
    // Logic
    //============================================================
    public static void Validate()
    {
        StringBuilder report = new StringBuilder();
        Directory.CreateDirectory("ProjectVfxImages");
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/ProjectVfx" });
        if (guids.Length == 0)
        {
            Debug.LogError("검증할 실제 prefab이 없습니다.");
            EditorApplication.Exit(1);
            return;
        }

        using (VfxThumbnailIndex thumbnails = new VfxThumbnailIndex())
        {
            for (int idx = 0; idx < guids.Length; idx++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[idx]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!thumbnails.TryCapture(guids[idx], prefab, 1.0f) || !thumbnails.TryGetTexture(guids[idx], out Texture2D texture) || thumbnails.ColorOf(guids[idx]) == EVfxColor.Invisible)
                {
                    Debug.LogError("실제 prefab의 썸네일에 표시되는 색상이 없습니다: " + path);
                    EditorApplication.Exit(1);
                    return;
                }

                File.WriteAllBytes("ProjectVfxImages/" + prefab.name + ".png", texture.EncodeToPNG());
                report.AppendLine(prefab.name + " | " + thumbnails.ColorOf(guids[idx]) + " | passed");
            }
        }

        string[] materials = AssetDatabase.FindAssets("t:Material", new[] { "Assets/ProjectVfx" });
        bool hasCustomShader = false;
        for (int idx = 0; idx < materials.Length; idx++)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(materials[idx]));
            if (material.shader == null || !material.shader.isSupported || ShaderUtil.ShaderHasError(material.shader))
            {
                Debug.LogError("머티리얼 shader를 사용할 수 없습니다: " + material.name);
                EditorApplication.Exit(1);
                return;
            }

            if (material.shader.name != "Molip/UI_Additive")
                continue;

            hasCustomShader = true;
            GameObject go = new GameObject("Custom shader probe", typeof(ParticleSystem));
            try
            {
                ParticleSystem ps = go.GetComponent<ParticleSystem>();
                ParticleSystem.MainModule main = ps.main;
                main.startColor = Color.green;
                main.startSize = 0.5f;
                ps.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
                using (VfxThumbnailIndex thumbnails = new VfxThumbnailIndex())
                {
                    if (!thumbnails.TryCapture("probe", go, 1.0f) || !thumbnails.TryGetTexture("probe", out Texture2D texture) || thumbnails.ColorOf("probe") != EVfxColor.Green)
                    {
                        Debug.LogError("커스텀 shader의 녹색 파티클 렌더링 검증에 실패했습니다.");
                        EditorApplication.Exit(1);
                        return;
                    }

                    File.WriteAllBytes("ProjectVfxImages/CustomShader.png", texture.EncodeToPNG());
                    report.AppendLine(material.shader.name + " | green probe | passed");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        if (!hasCustomShader)
        {
            Debug.LogError("커스텀 shader 검증용 머티리얼이 없습니다.");
            EditorApplication.Exit(1);
            return;
        }

        File.WriteAllText("project-vfx-result.txt", report.ToString());
    }
}
