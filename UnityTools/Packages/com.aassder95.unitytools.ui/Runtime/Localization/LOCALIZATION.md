# TMP localization adapter

Add `UiLocalizedText`, explicitly assign its TMP Text reference, set Key/Args/Design and the Apply Text/Apply Font options. Call `Init(IUiLocaleSource)` from your UI owner after injecting the game's locale source; pair it with `Release`. There is no global context search or implicit provider/component discovery. Existing prefabs/scenes are unchanged; new components require Inspector wiring.

The source owns current language, text lookup/formatting and font/material lookup. Implement `TryResolveText` using your existing localization table (including Unity Localization or Sheets if appropriate), and `TryResolveFont` using the game's per-language style catalog. Return false for unavailable keys/styles. Argument values are strings; no runtime object casts or formatting exceptions are introduced by the component. SetContent changes key/arguments without mutating the provider.

Enabled components subscribe once, refresh on language changes and unsubscribe when disabled, destroyed, released or reinitialized. Reenabling refreshes the latest language. Text and optional font/material are resolved before applying either. Missing data keeps the previous display, sets IsResolved=false and emits a Korean warning on automatic refresh; explicit TryRefresh returns false without logging. Text-only and font-only operation are configurable. This adapter does not provide translation content, download services or language selection UI.

Use the `UnityTools.Ui.Localization` assembly for TMP adapters; core UI assembly references remain unchanged. No source Module/Pizza-Idle assets or serialization names are changed.
