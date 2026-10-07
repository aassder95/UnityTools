param(
    [Parameter(Mandatory = $true)][string]$Project,
    [ValidateSet('Windows', 'Android')][string]$Target = 'Windows',
    [ValidateSet('IL2CPP', 'Mono')][string]$Backend = 'IL2CPP'
)

$ErrorActionPreference = 'Stop'
$utf8 = [Text.UTF8Encoding]::new($false)
$Project = (Resolve-Path -LiteralPath $Project).ProviderPath
$unityVersion = (Get-Content "$Project/editor-version.txt" -Raw).Trim()
$unityPath = "C:/Program Files/Unity/Hub/Editor/$unityVersion/Editor/Unity.exe"
if (!(Test-Path -LiteralPath $unityPath)) { throw "Unity 실행 파일이 없습니다: $unityPath" }
if ($Target -eq 'Android' -and $Backend -ne 'IL2CPP') { throw 'Android 검증은 ARM64 IL2CPP만 지원합니다.' }
if ($Target -eq 'Windows' -and $Backend -eq 'IL2CPP') {
    $vswherePath = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
    if (!(Test-Path -LiteralPath $vswherePath)) { throw 'Windows IL2CPP에는 Visual Studio C++ 컴파일 도구와 Windows SDK가 필요합니다.' }
    $cppInstallations = @(& $vswherePath -all -products '*' -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath)
    if ($LASTEXITCODE -ne 0 -or $cppInstallations.Count -eq 0) { throw 'Windows IL2CPP C++ 컴파일 도구가 없습니다. Visual Studio Installer에서 C++ 빌드 도구와 Windows SDK를 설치한 후 다시 실행하세요.' }
}
[xml]$tests = Get-Content "$Project/results.xml" -Raw
if ($tests.'test-run'.result -ne 'Passed' -or [int]$tests.'test-run'.total -ne 46 -or [int]$tests.'test-run'.skipped -ne 0) { throw '공개 태그 Timer Lab 46개 테스트를 먼저 완료하세요.' }
$lock = Get-Content "$Project/Packages/packages-lock.json" -Raw | ConvertFrom-Json
$sourceRef = 'cf83ef4377611d7c80a78544da34be3f2f60b33d'
if ($lock.dependencies.'com.aassder95.unitytools.timer'.hash -ne $sourceRef -or $lock.dependencies.'com.aassder95.unitytools.timer'.source -ne 'git') { throw 'Timer 공개 태그 설치 기록 불일치' }
$manifest = Get-Content "$Project/Packages/manifest.json" -Raw | ConvertFrom-Json
$manifest.dependencies | Add-Member -NotePropertyName 'com.aassder95.unitytools.ui' -NotePropertyValue 'https://github.com/aassder95/UnityTools.git?path=/UnityTools/Packages/com.aassder95.unitytools.ui#unitytools-ui/v2.2.0' -Force
[IO.File]::WriteAllText("$Project/Packages/manifest.json", ($manifest | ConvertTo-Json -Depth 8), $utf8)
Copy-Item -LiteralPath "$PSScriptRoot/UiTimerIl2CppValidation.cs" -Destination "$Project/Assets/Editor/UiTimerIl2CppValidation.cs"
Copy-Item -LiteralPath "$PSScriptRoot/UiTimerPlayerProbe.cs" -Destination "$Project/Assets/UiTimerPlayerProbe.cs"
$runPath = Join-Path $Project "UiTimer-$Backend-$Target-$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $runPath | Out-Null
[IO.File]::WriteAllText("$Project/UiTimerOutput.txt", $runPath, $utf8)
[IO.File]::WriteAllText("$Project/UiTimerTarget.txt", $Target, $utf8)
[IO.File]::WriteAllText("$Project/UiTimerBackend.txt", $Backend, $utf8)
Write-Host "UI·Timer $Backend 검증 경로: $runPath"
$editorArgs = @('-batchmode', '-projectPath', ('"' + $Project + '"'), '-buildTarget', $(if ($Target -eq 'Android') { 'Android' } else { 'Win64' }), '-executeMethod', 'UiTimerIl2CppValidation.BuildPlayer', '-quit', '-logFile', ('"' + $runPath + '/build.log"'))
$editor = Start-Process -FilePath $unityPath -ArgumentList $editorArgs -PassThru -WindowStyle Hidden
$editor.WaitForExit()
$editor.Refresh()
if ($editor.ExitCode -ne 0 -or !(Test-Path "$runPath/build-result.txt")) { throw "$Backend 빌드 실패: $runPath/build.log" }
$build = (Get-Content "$runPath/build-result.txt" -Raw).Trim()
$stripping = if ($Backend -eq 'IL2CPP') { 'High' } else { 'Disabled' }
if (!$build.StartsWith("$unityVersion | $Backend | $stripping stripping | Succeeded |")) { throw '빌드 버전·backend 결과 불일치' }
$lock = Get-Content "$Project/Packages/packages-lock.json" -Raw | ConvertFrom-Json
foreach ($name in @('ui','timer')) {
    $installed = $lock.dependencies."com.aassder95.unitytools.$name"
    if ($installed.source -ne 'git' -or $installed.hash -ne $sourceRef) { throw "공개 tag hash 불일치: $name" }
}
$playerResult = 'Not run (Android build only)'
if ($Target -eq 'Android') {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $apk = [IO.Compression.ZipFile]::OpenRead("$runPath/UiTimer.apk")
    try {
        if (!$apk.GetEntry('lib/arm64-v8a/libil2cpp.so')) { throw 'ARM64 IL2CPP native library 누락' }
        if (@($apk.Entries | Where-Object { $_.FullName -match 'UnityTools\.(Ui|Timer).*\.dll$' }).Count -gt 0) { throw 'managed package assembly 잔재' }
    }
    finally { $apk.Dispose() }
}
else {
    if ($Backend -eq 'IL2CPP') {
        if (!(Test-Path "$runPath/GameAssembly.dll")) { throw 'GameAssembly.dll 누락' }
        if (@(Get-ChildItem -LiteralPath $runPath -Recurse -Filter 'UnityTools.*.dll').Count -gt 0) { throw 'managed package assembly 잔재' }
    }
    $resultPath = Join-Path $runPath 'player-result.txt'
    $playerArgs = @('-batchmode', '-nographics', '-logFile', ('"' + $runPath + '/player.log"'), '--probe-result', ('"' + $resultPath + '"'))
    $player = Start-Process -FilePath "$runPath/UiTimer.exe" -ArgumentList $playerArgs -PassThru -WindowStyle Hidden
    if (!$player.WaitForExit(120000)) { Stop-Process -Id $player.Id; throw "Player 시간 초과: $runPath/player.log" }
    $player.Refresh()
    if ($player.ExitCode -ne 0 -or !(Test-Path $resultPath) -or (Get-Content $resultPath -Raw).Trim() -ne 'Passed') { throw "Player 검증 실패: $runPath/player.log" }
    $playerResult = 'Passed'
}
$result = [ordered]@{ Unity=$unityVersion; SourceRef=$sourceRef; Backend=$Backend; Stripping=$stripping; Target=$Target; Build=$build; Player=$playerResult; NativeAssemblyVerified=($Backend -eq 'IL2CPP'); Output=$runPath }
[IO.File]::WriteAllText("$runPath/summary.json", ($result | ConvertTo-Json), $utf8)
$result | ConvertTo-Json
