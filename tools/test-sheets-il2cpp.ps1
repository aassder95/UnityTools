param(
    [Parameter(Mandatory = $true)]
    [string]$Project,
    [string]$UnityPath,
    [ValidateSet('Windows', 'Android')]
    [string]$Target = 'Windows'
)

$ErrorActionPreference = 'Stop'
$utf8 = [Text.UTF8Encoding]::new($false)
$Project = (Resolve-Path -LiteralPath $Project).ProviderPath
$summary = Get-Content -LiteralPath "$Project/summary.json" -Raw | ConvertFrom-Json
if ($summary.Source -ne 'Git' -or $summary.Player -ne 'Passed' -or $summary.GeneratedCode -ne 'Generated type compile and read: Passed') {
    throw '먼저 test-sheets-package.ps1로 Git 설치와 생성 코드·Player 검증을 완료하세요.'
}
if (!$UnityPath) { $UnityPath = "C:/Program Files/Unity/Hub/Editor/$($summary.Unity)/Editor/Unity.exe" }
if (!(Test-Path -LiteralPath $UnityPath)) { throw "Unity 실행 파일을 찾을 수 없습니다: $UnityPath" }
& (Join-Path $PSScriptRoot 'confirm-upm-source.ps1') -Project $Project -PackageName 'com.aassder95.unitytools.sheets' -SourceRef $summary.SourceRef
$runPath = Join-Path $Project "Il2Cpp-$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $runPath | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'SheetsIl2CppValidation.cs') -Destination "$Project/Assets/Editor/SheetsIl2CppValidation.cs"
[IO.File]::WriteAllText("$Project/Il2CppOutput.txt", $runPath, $utf8)
[IO.File]::WriteAllText("$Project/Il2CppTarget.txt", $Target, $utf8)
Write-Host "IL2CPP 검증 결과 경로: $runPath"
$editorArgs = @('-batchmode', '-projectPath', ('"' + $Project + '"'), '-executeMethod', 'SheetsIl2CppValidation.BuildPlayer', '-quit', '-logFile', ('"' + $runPath + '/build.log"'))
$editorArgs += @('-buildTarget', $(if ($Target -eq 'Android') { 'Android' } else { 'Win64' }))
$editor = Start-Process -FilePath $UnityPath -ArgumentList $editorArgs -PassThru -WindowStyle Hidden
$editor.WaitForExit()
$editor.Refresh()
if ($editor.ExitCode -ne 0 -or !(Test-Path -LiteralPath "$runPath/build-result.txt")) { throw "IL2CPP 빌드 실패: $runPath/build.log" }
$build = (Get-Content -LiteralPath "$runPath/build-result.txt" -Raw).Trim()
if (!$build.StartsWith($summary.Unity + ' | IL2CPP | High stripping | Succeeded |')) { throw "Editor 버전 또는 빌드 결과 불일치: $build" }
$playerResult = 'Not run (Android build only)'
if ($Target -eq 'Android') {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $apk = [IO.Compression.ZipFile]::OpenRead("$runPath/Sheets.apk")
    try {
        if (!$apk.GetEntry('lib/arm64-v8a/libil2cpp.so')) { throw 'APK에 ARM64 IL2CPP native library가 없습니다.' }
        if (@($apk.Entries | Where-Object { $_.FullName -match 'UnityTools\.Sheets.*\.dll$' }).Count -gt 0) { throw 'APK에 Sheets managed assembly가 남았습니다.' }
    }
    finally { $apk.Dispose() }
}
else {
    if (!(Test-Path -LiteralPath "$runPath/GameAssembly.dll")) { throw 'IL2CPP GameAssembly.dll이 없습니다.' }
    if (@(Get-ChildItem -LiteralPath $runPath -Recurse -Filter 'UnityTools.Sheets*.dll').Count -gt 0) { throw 'IL2CPP 빌드에 Sheets managed assembly가 남았습니다.' }
    $resultPath = Join-Path $runPath 'player-result.txt'
    $playerArgs = @('-batchmode', '-nographics', '-logFile', ('"' + $runPath + '/player.log"'), '--sheet-result', ('"' + $resultPath + '"'))
    $player = Start-Process -FilePath "$runPath/Sheets.exe" -ArgumentList $playerArgs -PassThru -WindowStyle Hidden
    if (!$player.WaitForExit(120000)) { Stop-Process -Id $player.Id; throw "IL2CPP Player 시간 초과: $runPath/player.log" }
    $player.Refresh()
    if ($player.ExitCode -ne 0 -or !(Test-Path -LiteralPath $resultPath) -or (Get-Content -LiteralPath $resultPath -Raw).Trim() -ne 'Passed') { throw "IL2CPP Player 검증 실패: $runPath/player.log" }
    $playerResult = 'Passed'
}
$result = [ordered]@{
    Unity = $summary.Unity
    SourceRef = $summary.SourceRef
    Backend = 'IL2CPP'
    Target = $Target
    Stripping = 'High'
    Build = $build
    Player = $playerResult
    NativeAssemblyVerified = $true
    ManagedSheetsAssemblyExcluded = $true
    Output = $runPath
}
[IO.File]::WriteAllText("$runPath/summary.json", ($result | ConvertTo-Json), $utf8)
$result | ConvertTo-Json
