using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UnityTools.Ui.Localization
{
    public interface IUiLocaleSource
    {
        event Action OnLocaleChanged;
        SystemLanguage Language { get; }
        bool TryResolveText(string key, IReadOnlyList<string> args, out string text);
        bool TryResolveFont(string design, out TMP_FontAsset font, out Material material);
    }
}
