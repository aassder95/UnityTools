param(
    [Parameter(Mandatory = $true)]
    [string]$Project,
    [string]$UnityPath,
    [switch]$FullValidation
)

$ErrorActionPreference = 'Stop'
$utf8 = [Text.UTF8Encoding]::new($false)
$Project = (Resolve-Path -LiteralPath $Project).ProviderPath
$summary = Get-Content -LiteralPath "$Project/summary.json" -Raw | ConvertFrom-Json
if ($summary.Source -ne 'Git' -or $summary.Passed -ne $summary.Total -or $summary.Total -le 0 -or $summary.Skipped -ne 0) { throw '먼저 test-vfx-package.ps1로 Git 설치 검증을 완료하세요.' }
$urpVersion = if ($summary.Unity.StartsWith('2022.3.')) { '14.0.12' } elseif ($summary.Unity.StartsWith('6000.3.')) { '17.3.0' } else { throw '검증 대상은 Unity 2022.3 또는 6000.3입니다.' }
if (!$UnityPath) { $UnityPath = "C:/Program Files/Unity/Hub/Editor/$($summary.Unity)/Editor/Unity.exe" }
if (!(Test-Path -LiteralPath $UnityPath)) { throw "Unity 실행 파일을 찾을 수 없습니다: $UnityPath" }
& (Join-Path $PSScriptRoot 'confirm-upm-source.ps1') -Project $Project -PackageName 'com.aassder95.unitytools.vfx' -SourceRef $summary.SourceRef
$isConfigured = Test-Path -LiteralPath "$Project/Assets/VfxValidationPipeline.asset"
$runPath = Join-Path $Project "Urp-$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $runPath | Out-Null
$manifest = Get-Content -LiteralPath "$Project/Packages/manifest.json" -Raw | ConvertFrom-Json
$manifest.dependencies | Add-Member -MemberType NoteProperty -Name 'com.unity.render-pipelines.universal' -Value $urpVersion -Force
[IO.File]::WriteAllText("$Project/Packages/manifest.json", ($manifest | ConvertTo-Json -Depth 10), $utf8)
[IO.File]::WriteAllText("$Project/UrpOutput.txt", $runPath, $utf8)
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'VfxUrpValidation.cs') -Destination "$Project/Assets/Editor/VfxUrpValidation.cs"
Write-Host "URP 검증 결과 경로: $runPath"
$steps = if ($isConfigured) { @('Validate') } else { @('Configure', 'Validate') }
foreach ($step in $steps) {
    $editorArgs = @('-batchmode', '-projectPath', ('"' + $Project + '"'), '-executeMethod', ('VfxUrpValidation.' + $step), '-quit', '-logFile', ('"' + $runPath + '/' + $step + '.log"'))
    $editor = Start-Process -FilePath $UnityPath -ArgumentList $editorArgs -PassThru -WindowStyle Hidden
    $editor.WaitForExit()
    $editor.Refresh()
    if ($editor.ExitCode -ne 0) { throw "URP $step 실패: $runPath/$step.log" }
}
& (Join-Path $PSScriptRoot 'confirm-upm-source.ps1') -Project $Project -PackageName 'com.aassder95.unitytools.vfx' -SourceRef $summary.SourceRef
$lock = Get-Content -LiteralPath "$Project/Packages/packages-lock.json" -Raw | ConvertFrom-Json
if ($lock.dependencies.'com.unity.render-pipelines.universal'.version -ne $urpVersion) { throw 'URP lock 버전 불일치' }
$report = (Get-Content -LiteralPath "$runPath/result.txt" -Raw).Trim()
if (!$report.StartsWith($summary.Unity + ' | ' + $urpVersion)) { throw '실행한 Unity 또는 URP 버전 불일치' }
if (Test-Path -LiteralPath "$Project/project-vfx-source.json") {
    $source = Get-Content -LiteralPath "$Project/project-vfx-source.json" -Raw | ConvertFrom-Json
    foreach ($entry in $source.sha256.PSObject.Properties) {
        if ((Get-FileHash -LiteralPath (Join-Path $source.source_assets $entry.Name) -Algorithm SHA256).Hash -ne $entry.Value -or (Get-FileHash -LiteralPath (Join-Path "$Project/Assets/ProjectVfx" $entry.Name) -Algorithm SHA256).Hash -ne $entry.Value) { throw "VFX 원본 또는 사본 변경: $($entry.Name)" }
    }
}
$result = [ordered]@{ Unity = $summary.Unity; Urp = $urpVersion; SourceRef = $summary.SourceRef; NativeUrpPreview = 'Passed'; Observations = $report; Output = $runPath }
if ($FullValidation) {
    $testArgs = @('-batchmode', '-projectPath', ('"' + $Project + '"'), '-runTests', '-testPlatform', 'EditMode', '-testFilter', 'UnityTools.Vfx.Editor.Tests', '-testResults', ('"' + $runPath + '/results.xml"'), '-logFile', ('"' + $runPath + '/tests.log"'))
    $editor = Start-Process -FilePath $UnityPath -ArgumentList $testArgs -PassThru -WindowStyle Hidden
    $editor.WaitForExit()
    $editor.Refresh()
    if ($editor.ExitCode -ne 0) { throw "URP 테스트 실행 실패: $runPath/tests.log" }
    [xml]$tests = Get-Content -LiteralPath "$runPath/results.xml" -Raw
    $run = $tests.'test-run'
    if ($run.result -ne 'Passed' -or [int]$run.passed -le 0 -or [int]$run.skipped -gt 0) { throw "URP 테스트 실패: $($run.result), failed=$($run.failed), skipped=$($run.skipped). $runPath/results.xml" }
    $buildArgs = @('-batchmode', '-projectPath', ('"' + $Project + '"'), '-executeMethod', 'VfxUrpValidation.BuildPlayer', '-quit', '-logFile', ('"' + $runPath + '/build.log"'))
    $editor = Start-Process -FilePath $UnityPath -ArgumentList $buildArgs -PassThru -WindowStyle Hidden
    $editor.WaitForExit()
    $editor.Refresh()
    if ($editor.ExitCode -ne 0) { throw "URP Player 빌드 실패: $runPath/build.log" }
    $build = (Get-Content -LiteralPath "$runPath/build-result.txt" -Raw).Trim()
    if (!$build.StartsWith($summary.Unity + ' | URP | Succeeded |')) { throw 'URP 빌드 결과 또는 실행 버전 불일치' }
    if (@(Get-ChildItem -LiteralPath "$runPath/Build" -Recurse -Filter 'UnityTools.Vfx*.dll').Count -gt 0) { throw 'URP Player에 Editor 전용 VFX assembly가 포함됐습니다.' }
    $result['Tests'] = "$($run.passed)/$($run.total)"
    $result['Skipped'] = [int]$run.skipped
    $result['Build'] = $build
    $result['EditorAssemblyExcluded'] = $true
}
[IO.File]::WriteAllText("$runPath/summary.json", ($result | ConvertTo-Json), $utf8)
$result | ConvertTo-Json
