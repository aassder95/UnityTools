# UnityTools Core

`UnityTools` ?꾨줈?앺듃?먯꽌 怨듯넻?쇰줈 ?ъ궗??媛?ν븳 ?좏떥由ы떚 肄붾뱶瑜?異붿텧??UPM ?⑦궎吏?낅땲??

## ?ы븿 踰붿쐞

- Runtime
  - Timer ?좏떥由ы떚 (`TaskTimer`, `PeriodTimer`, `Handle`, `StateMachine`)
  - 怨듯넻 肄붿뼱 (`ObjectPool`, `Deque`, `Singleton`, `EventDispatcher`)
  - UI ?좏떥 (`DynamicScrollView` 怨꾩뿴)
  - ??μ냼 ?좏떥 (`IStorage`, `PlayerPrefsStorage`, `FileStorage`)
  - ?뺤옣/?ы띁 (`CollectionExtensions`, `CoroutineHelper`, `DateTimeUtils` ??
- Editor
  - ?곗씠???뺣━ 硫붾돱 (`PlayerPrefs`, `PersistentData`)

## ?ㅼ튂

### 1. Git URL 諛⑹떇 (沅뚯옣)

`Package Manager > Add package from git URL...` ???꾨옒 ?뺥깭濡?異붽??⑸땲??

```text
https://<git-repo-url>.git?path=/Packages/com.unitytools.core#v0.1.0
```

### 2. 濡쒖뺄 ?뚯씪 寃쎈줈 諛⑹떇

媛쒕컻 ?덊룷瑜??쒕툕紐⑤뱢/?숈씪 ?뚰겕?ㅽ럹?댁뒪濡???寃쎌슦:

```json
"com.unitytools.core": "file:../../Packages/com.unitytools.core"
```

## 留덉씠洹몃젅?댁뀡 媛?대뱶

- 怨듯넻 肄붾뱶: ?⑦궎吏?먯꽌 ?쒓났
- ?꾨줈?앺듃 ?꾩슜 肄붾뱶: 湲곗〈 ?꾨줈?앺듃 `Assets` ?덉씠?댁뿉 ?좎?
  - ?? 留ㅻ땲? 寃고빀 肄붾뱶, 寃뚯엫 ?꾨찓?몃퀎 ?뚯뒪???덈룄??
?꾩옱 ?덊룷?먯꽌???꾨옒 ?뚯씪???꾨줈?앺듃 ?꾩슜(Adapter)?쇰줈 ?좎??⑸땲??

- `Assets/Scripts/Util/Core/Singletons.cs`
- `Assets/Scripts/Util/Editor/TaskTimerTestWindow.cs`
- `Assets/Scripts/Util/Editor/PeriodTimerTestWindow.cs`

## ?섑뵆

`Samples~/QuickStart` ?대뜑??理쒖냼 ?ㅽ뻾 ?덉젣媛 ?ы븿?섏뼱 ?덉뒿?덈떎.

## 由대━??
?꾨옒 ?ㅽ겕由쏀듃濡?`package.json` 踰꾩쟾怨??쒓렇瑜??쇱튂?쒖폒 由대━?ㅽ븷 ???덉뒿?덈떎.

```powershell
pwsh ./Tools/Package/release-unitytools-core.ps1 -Version 0.1.0
```

?먭꺽 ?쒓렇 ?몄떆源뚯? ??踰덉뿉 ?섑뻾?섎젮硫?

```powershell
pwsh ./Tools/Package/release-unitytools-core.ps1 -Version 0.1.0 -Push
```
