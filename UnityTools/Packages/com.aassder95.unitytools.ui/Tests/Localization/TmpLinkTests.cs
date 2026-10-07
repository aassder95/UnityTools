using System;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityTools.Ui.Localization.Tests
{
    public class TmpLinkTests
    {
        //============================================================
        // Logic
        //============================================================
        [Test]
        public void PointerPositionActivatesOnlyTheClickedLink()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas));
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(canvasGo.transform, false);
            var eventGo = new GameObject("Events", typeof(EventSystem));
            var font = ScriptableObject.CreateInstance<TMP_FontAsset>();
            JsonUtility.FromJsonOverwrite("{\"m_Version\":\"1.1.0\"}", font);
            var texture = new Texture2D(4, 4);
            var material = new Material(Shader.Find("UI/Default"));
            material.mainTexture = texture;
            font.atlasTextures = new[] { texture };
            font.material = material;
            font.faceInfo = new UnityEngine.TextCore.FaceInfo { pointSize = 10, scale = 1.0f, ascentLine = 10.0f, descentLine = 0.0f, lineHeight = 10.0f };
            var glyph = new UnityEngine.TextCore.Glyph(1, new UnityEngine.TextCore.GlyphMetrics(10.0f, 10.0f, 0.0f, 10.0f, 10.0f), new UnityEngine.TextCore.GlyphRect(0, 0, 4, 4), 1.0f, 0);
            font.glyphTable.Add(glyph);
            foreach (uint code in new uint[] { 65, 0x2026, 0x5F })
            {
                font.characterTable.Add(new TMP_Character(code, glyph));
            }

            var settingsField = typeof(TMP_Settings).GetField("s_Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var previousSettings = (TMP_Settings)settingsField.GetValue(null);
            var settings = ScriptableObject.CreateInstance<TMP_Settings>();
            typeof(TMP_Settings).GetField("m_defaultFontAsset", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(settings, font);
            settingsField.SetValue(null, settings);
            try
            {
                var text = go.AddComponent<TextMeshProUGUI>();
                text.font = font;
                text.fontSize = 20.0f;
                text.rectTransform.sizeDelta = new Vector2(200.0f, 80.0f);
                text.text = "<link=\"target\">A</link>";
                Canvas.ForceUpdateCanvases();
                text.ForceMeshUpdate();
                Assert.That(text.textInfo.linkCount, Is.EqualTo(1));
                var handler = go.AddComponent<TmpLinkHandler>();
                typeof(TmpLinkHandler).GetField("_txt", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(handler, text);
                string clicked = null;
                Action<string> listener = id => clicked = id;
                handler.OnLinkClicked += listener;
                try
                {
                    TMP_CharacterInfo character = text.textInfo.characterInfo[0];
                    Vector3 localPos = (character.bottomLeft + character.topRight) * 0.5f;
                    Vector2 position = RectTransformUtility.WorldToScreenPoint(null, text.transform.TransformPoint(localPos));
                    var pointer = new PointerEventData(eventGo.GetComponent<EventSystem>()) { position = position, button = PointerEventData.InputButton.Right };
                    handler.OnPointerClick(pointer);
                    Assert.That(clicked, Is.Null);
                    pointer.button = PointerEventData.InputButton.Left;
                    handler.OnPointerClick(pointer);
                    Assert.That(clicked, Is.EqualTo("target"));
                    clicked = null;
                    pointer.position = new Vector2(-10000.0f, -10000.0f);
                    handler.OnPointerClick(pointer);
                    Assert.That(clicked, Is.Null);
                }
                finally
                {
                    handler.OnLinkClicked -= listener;
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(canvasGo);
                UnityEngine.Object.DestroyImmediate(eventGo);
                UnityEngine.Object.DestroyImmediate(font);
                UnityEngine.Object.DestroyImmediate(material);
                UnityEngine.Object.DestroyImmediate(texture);
                settingsField.SetValue(null, previousSettings);
                UnityEngine.Object.DestroyImmediate(settings);
            }
        }
    }
}
