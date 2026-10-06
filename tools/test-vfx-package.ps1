param(
    [string]$UnityVersion = '6000.3.20f1',
    [string]$UnityPath,
    [string]$OutputBase = $env:TEMP,
    [switch]$NoGraphics
)

$ErrorActionPreference = 'Stop'
$utf8 = [Text.UTF8Encoding]::new($false)
$repoRoot = Split-Path -Parent $PSScriptRoot
if (!$UnityPath) { $UnityPath = "C:\Program Files\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe" }
if (!(Test-Path -LiteralPath $UnityPath)) { throw "Unity 실행 파일을 찾을 수 없습니다: $UnityPath" }
$project = Join-Path $OutputBase "UnityTools-Vfx-$UnityVersion-$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path "$project/Assets/Editor", "$project/Packages", "$project/ProjectSettings" -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'CompatibilityValidation.cs') -Destination "$project/Assets/Editor/CompatibilityValidation.cs"
$dependencies = [ordered]@{
    'com.aassder95.unitytools.vfx' = 'file:' + (Join-Path $repoRoot 'UnityTools/Packages/com.aassder95.unitytools.vfx').Replace('\', '/')
    'com.unity.test-framework' = '1.1.33'
    'com.unity.modules.particlesystem' = '1.0.0'
    'com.unity.modules.physics' = '1.0.0'
    'com.unity.modules.audio' = '1.0.0'
    'com.unity.modules.animation' = '1.0.0'
    'com.unity.modules.imgui' = '1.0.0'
    'com.unity.modules.imageconversion' = '1.0.0'
    'com.unity.modules.jsonserialize' = '1.0.0'
}
$manifest = @{ dependencies = $dependencies; testables = @('com.aassder95.unitytools.vfx') }
[IO.File]::WriteAllText("$project/Packages/manifest.json", ($manifest | ConvertTo-Json -Depth 5), $utf8)
[IO.File]::WriteAllText("$project/ProjectSettings/ProjectVersion.txt", "m_EditorVersion: $UnityVersion", $utf8)
[IO.File]::WriteAllText("$project/ValidationSamples.txt", '', $utf8)
Write-Host "검증 결과 경로: $project"

function Invoke-VfxEditor([string]$step, [string[]]$extra)
{
    $log = "$project/$step.log"
    $arguments = @('-batchmode', '-projectPath', ('"' + $project + '"'), '-logFile', ('"' + $log + '"'))
    if ($NoGraphics) { $arguments += '-nographics' }
    $arguments += $extra
    $process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -PassThru -WindowStyle Hidden
    $process.WaitForExit()
    $process.Refresh()
    if ($process.ExitCode -ne 0) { throw "$step 실패 (exit=$($process.ExitCode)). 로그: $log" }
}

Invoke-VfxEditor 'import' @('-executeMethod', 'CompatibilityValidation.ImportSamples', '-quit')
Invoke-VfxEditor 'tests' @('-runTests', '-testPlatform', 'EditMode', '-testFilter', 'UnityTools.Vfx.Editor.Tests', '-testResults', ('"' + $project + '/results.xml"'))
[xml]$testResult = Get-Content -LiteralPath "$project/results.xml" -Raw
$run = $testResult.'test-run'
if ($run.result -ne 'Passed' -or [int]$run.passed -le 0) { throw "VFX 테스트 실패: $($run.result) / $($run.failed)" }
Invoke-VfxEditor 'scenes' @('-executeMethod', 'CompatibilityValidation.PrepareScenes', '-quit')
Invoke-VfxEditor 'build' @('-executeMethod', 'CompatibilityValidation.BuildPlayer', '-quit')
$playerAssemblies = @(Get-ChildItem -LiteralPath "$project/Build" -Recurse -Filter 'UnityTools.Vfx*.dll')
if ($playerAssemblies.Count -gt 0) { throw 'Player 빌드에 Editor 전용 VFX assembly가 포함됐습니다.' }
$images = @($testResult.SelectNodes('//test-case/output') | ForEach-Object { if ($_.InnerText -match 'Preview image: ([^\r\n]+)') { $Matches[1] } })
$summary = [ordered]@{
    Unity = $UnityVersion
    Source = 'Local'
    Passed = [int]$run.passed
    Total = [int]$run.total
    Skipped = [int]$run.skipped
    Build = (Get-Content -LiteralPath "$project/build-result.txt" -Raw).Trim()
    EditorAssemblyExcluded = $true
    Project = $project
    PreviewImages = $images
}
[IO.File]::WriteAllText("$project/summary.json", ($summary | ConvertTo-Json -Depth 4), $utf8)
$summary | ConvertTo-Json -Depth 4
