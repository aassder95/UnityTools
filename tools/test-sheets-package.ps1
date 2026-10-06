param(
    [string]$UnityVersion = '6000.3.20f1',
    [string]$UnityPath,
    [string]$OutputBase = $env:TEMP
)

$ErrorActionPreference = 'Stop'
$utf8 = [Text.UTF8Encoding]::new($false)
$repoRoot = Split-Path -Parent $PSScriptRoot
if (!$UnityPath) { $UnityPath = "C:\Program Files\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe" }
if (!(Test-Path -LiteralPath $UnityPath)) { throw "Unity 실행 파일을 찾을 수 없습니다: $UnityPath" }
$project = Join-Path $OutputBase "UnityTools-Sheets-$UnityVersion-$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path "$project/Assets/Editor", "$project/Packages", "$project/ProjectSettings" -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'CompatibilityValidation.cs') -Destination "$project/Assets/Editor/CompatibilityValidation.cs"
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'SheetsCompatibilityValidation.cs') -Destination "$project/Assets/Editor/SheetsCompatibilityValidation.cs"
$dependencies = [ordered]@{
    'com.aassder95.unitytools.sheets' = 'file:' + (Join-Path $repoRoot 'UnityTools/Packages/com.aassder95.unitytools.sheets').Replace('\', '/')
    'com.unity.test-framework' = '1.1.33'
    'com.unity.modules.particlesystem' = '1.0.0'
    'com.unity.modules.physics' = '1.0.0'
    'com.unity.modules.audio' = '1.0.0'
    'com.unity.modules.animation' = '1.0.0'
    'com.unity.modules.imgui' = '1.0.0'
    'com.unity.modules.imageconversion' = '1.0.0'
    'com.unity.modules.jsonserialize' = '1.0.0'
}
$manifest = @{ dependencies = $dependencies; testables = @('com.aassder95.unitytools.sheets') }
[IO.File]::WriteAllText("$project/Packages/manifest.json", ($manifest | ConvertTo-Json -Depth 5), $utf8)
[IO.File]::WriteAllText("$project/ProjectSettings/ProjectVersion.txt", "m_EditorVersion: $UnityVersion", $utf8)
[IO.File]::WriteAllText("$project/ValidationSamples.txt", '', $utf8)
Write-Host "검증 결과 경로: $project"

function Invoke-SheetsEditor([string]$step, [string[]]$extra)
{
    $log = "$project/$step.log"
    $arguments = @('-batchmode', '-projectPath', ('"' + $project + '"'), '-logFile', ('"' + $log + '"'))
    $arguments += $extra
    $process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -PassThru -WindowStyle Hidden
    $process.WaitForExit()
    $process.Refresh()
    if ($process.ExitCode -ne 0) { throw "$step 실패 (exit=$($process.ExitCode)). 로그: $log" }
}

Invoke-SheetsEditor 'import' @('-executeMethod', 'CompatibilityValidation.ImportSamples', '-quit')
Invoke-SheetsEditor 'generate' @('-executeMethod', 'SheetsCompatibilityValidation.GenerateFixture', '-quit')
Invoke-SheetsEditor 'verify-generated' @('-executeMethod', 'SheetsCompatibilityValidation.VerifyGenerated', '-quit')
Invoke-SheetsEditor 'tests-edit' @('-runTests', '-testPlatform', 'EditMode', '-testFilter', 'UnityTools.Sheets', '-testResults', ('"' + $project + '/results-edit.xml"'))
Invoke-SheetsEditor 'tests-play' @('-runTests', '-testPlatform', 'PlayMode', '-testFilter', 'UnityTools.Sheets.Tests', '-testResults', ('"' + $project + '/results-play.xml"'))
$results = @{}
foreach ($platform in @('edit', 'play')) {
    [xml]$testResult = Get-Content -LiteralPath "$project/results-$platform.xml" -Raw
    $run = $testResult.'test-run'
    if ($run.result -ne 'Passed' -or [int]$run.passed -le 0 -or [int]$run.skipped -gt 0) { throw "Sheets $platform 테스트 실패: $($run.result) / $($run.failed) / skip=$($run.skipped)" }
    $results[$platform] = "$($run.passed)/$($run.total)"
}
Invoke-SheetsEditor 'scenes' @('-executeMethod', 'SheetsCompatibilityValidation.PrepareScene', '-quit')
Invoke-SheetsEditor 'build' @('-executeMethod', 'CompatibilityValidation.BuildPlayer', '-quit')
$editorAssemblies = @(Get-ChildItem -LiteralPath "$project/Build" -Recurse -Filter 'UnityTools.Sheets.Editor*.dll')
if ($editorAssemblies.Count -gt 0) { throw 'Player 빌드에 Editor 전용 Sheets assembly가 포함됐습니다.' }
$resultPath = "$project/player-result.txt"
$player = Start-Process -FilePath "$project/Build/Compatibility.exe" -ArgumentList @('-batchmode', '-nographics', '-logFile', ('"' + $project + '/player.log"'), '--sheet-result', ('"' + $resultPath + '"')) -PassThru -WindowStyle Hidden
if (!$player.WaitForExit(120000)) { Stop-Process -Id $player.Id; throw "Player 검증 시간 초과: $project/player.log" }
$player.Refresh()
if ($player.ExitCode -ne 0 -or !(Test-Path -LiteralPath $resultPath) -or (Get-Content -LiteralPath $resultPath -Raw).Trim() -ne 'Passed') { throw "Player 검증 실패: $project/player.log" }
$summary = [ordered]@{
    Unity = $UnityVersion
    Source = 'Local'
    EditMode = $results['edit']
    PlayMode = $results['play']
    GeneratedCode = (Get-Content -LiteralPath "$project/generated-result.txt" -Raw).Trim()
    Build = (Get-Content -LiteralPath "$project/build-result.txt" -Raw).Trim()
    Player = (Get-Content -LiteralPath $resultPath -Raw).Trim()
    EditorAssemblyExcluded = $true
    Project = $project
}
[IO.File]::WriteAllText("$project/summary.json", ($summary | ConvertTo-Json -Depth 4), $utf8)
$summary | ConvertTo-Json -Depth 4
