using System;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityTools.Ui.Localization.Tests
{
    public class UiLocalizedTextTests
    {
        //============================================================
        // Logic
        //============================================================
        [Test]
        public void ReinitDisableEnableAndReleaseKeepOneSubscription()
        {
            var go = new GameObject("Locale", typeof(RectTransform), typeof(TextMeshProUGUI));
            try
            {
                var component = go.AddComponent<UiLocalizedText>();
                TMP_Text text = go.GetComponent<TMP_Text>();
                typeof(UiLocalizedText).GetField("_txt", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(component, text);
                typeof(UiLocalizedText).GetField("_shouldApplyFont", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(component, false);
                component.SetContent("hello", "user");
                var first = new LocaleSource();
                var second = new LocaleSource();
                component.Init(first);
                Assert.That(text.text, Is.EqualTo("English:hello:user"));
                Assert.That(first.SubscriberCnt, Is.EqualTo(1));
                component.Init(second);
                Assert.That(first.SubscriberCnt, Is.Zero);
                component.enabled = false;
                Assert.That(second.SubscriberCnt, Is.Zero);
                second.ChangeLanguage(SystemLanguage.Korean);
                component.enabled = true;
                Assert.That(second.SubscriberCnt, Is.EqualTo(1));
                Assert.That(text.text, Is.EqualTo("Korean:hello:user"));
                Assert.That(component.IsResolved, Is.True);
                component.Release();
                Assert.That(second.SubscriberCnt, Is.Zero);
                Assert.That(component.TryRefresh(), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ResolutionFailureDoesNotPartiallyReplaceText()
        {
            var go = new GameObject("Locale", typeof(RectTransform), typeof(TextMeshProUGUI));
            try
            {
                var component = go.AddComponent<UiLocalizedText>();
                TMP_Text text = go.GetComponent<TMP_Text>();
                typeof(UiLocalizedText).GetField("_txt", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(component, text);
                text.text = "previous";
                var source = new LocaleSource();
                LogAssert.Expect(LogType.Warning, "다국어 문구 또는 폰트를 찾지 못했습니다:  / English");
                component.Init(source);
                Assert.That(component.IsResolved, Is.False);
                Assert.That(text.text, Is.EqualTo("previous"));
                UnityEngine.Object.DestroyImmediate(component);
                Assert.That(source.SubscriberCnt, Is.Zero);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void LanguageChangeAppliesTextFontAndMaterialTogether()
        {
            var go = new GameObject("Locale", typeof(RectTransform), typeof(TextMeshProUGUI));
            var texture = new Texture2D(4, 4);
            var firstFont = ScriptableObject.CreateInstance<TMP_FontAsset>();
            var secondFont = ScriptableObject.CreateInstance<TMP_FontAsset>();
            var firstMaterial = new Material(Shader.Find("UI/Default"));
            var secondMaterial = new Material(Shader.Find("UI/Default"));
            firstMaterial.mainTexture = texture;
            secondMaterial.mainTexture = texture;
            JsonUtility.FromJsonOverwrite("{\"m_Version\":\"1.1.0\"}", firstFont);
            JsonUtility.FromJsonOverwrite("{\"m_Version\":\"1.1.0\"}", secondFont);
            firstFont.name = "First Test Font";
            secondFont.name = "Second Test Font";
            firstFont.atlasTextures = new[] { texture };
            secondFont.atlasTextures = new[] { texture };
            var glyph = new UnityEngine.TextCore.Glyph { index = 1 };
            firstFont.glyphTable.Add(glyph);
            secondFont.glyphTable.Add(glyph);
            firstFont.characterTable.Add(new TMP_Character(0x2026, glyph));
            secondFont.characterTable.Add(new TMP_Character(0x2026, glyph));
            firstFont.characterTable.Add(new TMP_Character(0x5F, glyph));
            secondFont.characterTable.Add(new TMP_Character(0x5F, glyph));
            firstFont.material = firstMaterial;
            secondFont.material = secondMaterial;
            try
            {
                var component = go.AddComponent<UiLocalizedText>();
                TMP_Text text = go.GetComponent<TMP_Text>();
                typeof(UiLocalizedText).GetField("_txt", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(component, text);
                component.SetContent("hello");
                var source = new LocaleSource(firstFont, firstMaterial);
                component.Init(source);
                Assert.That(component.IsResolved, Is.True);
                Assert.That(text.font, Is.SameAs(firstFont));
                Assert.That(text.fontSharedMaterial, Is.SameAs(firstMaterial));
                source.SetFont(secondFont, secondMaterial);
                source.ChangeLanguage(SystemLanguage.Korean);
                Assert.That(text.text, Does.StartWith("Korean:"));
                Assert.That(text.font, Is.SameAs(secondFont));
                Assert.That(text.fontSharedMaterial, Is.SameAs(secondMaterial));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                UnityEngine.Object.DestroyImmediate(firstFont);
                UnityEngine.Object.DestroyImmediate(secondFont);
                UnityEngine.Object.DestroyImmediate(firstMaterial);
                UnityEngine.Object.DestroyImmediate(secondMaterial);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        //============================================================
        // Nested Types
        //============================================================
        private class LocaleSource : IUiLocaleSource
        {
            private event Action _onLocaleChanged;
            private TMP_FontAsset _font;
            private Material _material;

            public LocaleSource(TMP_FontAsset font = null, Material material = null)
            {
                _font = font;
                _material = material;
            }

            public void SetFont(TMP_FontAsset font, Material material)
            {
                _font = font;
                _material = material;
            }

            private SystemLanguage _language = SystemLanguage.English;
            public event Action OnLocaleChanged { add => _onLocaleChanged += value; remove => _onLocaleChanged -= value; }
            public SystemLanguage Language => _language;
            public int SubscriberCnt => _onLocaleChanged?.GetInvocationList().Length ?? 0;

            public bool TryResolveText(string key, IReadOnlyList<string> args, out string text)
            {
                text = _language + ":" + key + ":" + (args.Count == 0 ? "" : args[0]);
                return true;
            }

            public bool TryResolveFont(string design, out TMP_FontAsset font, out Material material)
            {
                font = _font;
                material = _material;
                return _font != null && _material != null;
            }

            public void ChangeLanguage(SystemLanguage language)
            {
                _language = language;
                _onLocaleChanged?.Invoke();
            }
        }
    }
}
