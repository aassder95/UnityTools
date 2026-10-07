param([string]$UnityVersion = '2022.3.62f3', [string]$OutputBase = $env:TEMP)
$ErrorActionPreference = 'Stop'
$utf8 = [Text.UTF8Encoding]::new($false)
$repoRoot = Split-Path -Parent $PSScriptRoot
$unityPath = "C:\Program Files\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe"
if (!(Test-Path -LiteralPath $unityPath)) { throw "Unity 실행 파일을 찾을 수 없습니다: $unityPath" }
$project = Join-Path $OutputBase "UnityTools-Ports-$UnityVersion-$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path "$project/Assets/Editor", "$project/Packages", "$project/ProjectSettings" -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'CompatibilityValidation.cs') -Destination "$project/Assets/Editor/CompatibilityValidation.cs"
$dependencies = [ordered]@{
    'com.aassder95.unitytools.ui' = 'file:' + (Join-Path $repoRoot 'UnityTools/Packages/com.aassder95.unitytools.ui').Replace('\','/')
    'com.aassder95.unitytools.vfx' = 'file:' + (Join-Path $repoRoot 'UnityTools/Packages/com.aassder95.unitytools.vfx').Replace('\','/')
    'com.unity.test-framework' = '1.1.33'
    'com.unity.modules.ui' = '1.0.0'
    'com.unity.modules.imgui' = '1.0.0'
    'com.unity.modules.jsonserialize' = '1.0.0'
    'com.unity.modules.particlesystem' = '1.0.0'
    'com.unity.modules.physics' = '1.0.0'
    'com.unity.modules.audio' = '1.0.0'
    'com.unity.modules.animation' = '1.0.0'
    'com.unity.modules.imageconversion' = '1.0.0'
}
$manifest = @{ dependencies = $dependencies; testables = @('com.aassder95.unitytools.ui', 'com.aassder95.unitytools.vfx') }
[IO.File]::WriteAllText("$project/Packages/manifest.json", ($manifest | ConvertTo-Json -Depth 5), $utf8)
[IO.File]::WriteAllText("$project/ProjectSettings/ProjectVersion.txt", "m_EditorVersion: $UnityVersion", $utf8)
[IO.File]::WriteAllText("$project/ValidationSamples.txt", '', $utf8)
Write-Host "검증 결과 경로: $project"
function Invoke-PortsUnity([string]$step, [string[]]$extra)
{
    $arguments = @('-batchmode', '-projectPath', ('"' + $project + '"'), '-logFile', ('"' + $project + '/' + $step + '.log"')) + $extra
    $process = Start-Process -FilePath $unityPath -ArgumentList $arguments -PassThru -WindowStyle Hidden
    $process.WaitForExit()
    $process.Refresh()
    if ($process.ExitCode -ne 0) { throw "$step 실패 (exit=$($process.ExitCode)). 로그: $project/$step.log" }
}
Invoke-PortsUnity 'import' @('-executeMethod', 'CompatibilityValidation.ImportSamples', '-quit')
$summary = [ordered]@{ Unity = $UnityVersion; Source = 'Local'; Project = $project }
foreach ($platform in @('EditMode', 'PlayMode'))
{
    $filter = if ($platform -eq 'EditMode') { 'UnityTools.Ui.Editor.Tests;UnityTools.Vfx.Editor.Tests;UnityTools.Ui.Localization.Tests;UnityTools.Ui.Tests.EnumDisplayTests' } else { 'UnityTools.Ui.Localization.Tests;UnityTools.Ui.Tests.EnumDisplayTests' }
    Invoke-PortsUnity $platform @('-runTests', '-testPlatform', $platform, '-testFilter', ('"' + $filter + '"'), '-testResults', ('"' + $project + '/' + $platform + '.xml"'))
    [xml]$result = Get-Content -LiteralPath "$project/$platform.xml" -Raw
    $run = $result.'test-run'
    if ($run.result -ne 'Passed' -or [int]$run.passed -le 0 -or [int]$run.skipped -ne 0) { throw "$platform 테스트 실패: $($run.result) / $($run.failed) / skipped=$($run.skipped)" }
    $summary[$platform] = @{ Passed = [int]$run.passed; Failed = [int]$run.failed; Skipped = [int]$run.skipped }
}
Invoke-PortsUnity 'scenes' @('-executeMethod', 'CompatibilityValidation.PrepareScenes', '-quit')
Invoke-PortsUnity 'build' @('-executeMethod', 'CompatibilityValidation.BuildPlayer', '-quit')
$editorAssemblies = @(Get-ChildItem -LiteralPath "$project/Build" -Recurse -Filter '*.dll' | Where-Object { $_.Name -like 'UnityTools.Vfx*' -or $_.Name -like 'UnityTools.Ui.Editor*' })
if ($editorAssemblies.Count -gt 0) { throw 'Player 빌드에 Editor 전용 assembly가 포함됐습니다.' }
$summary['PlayerBuild'] = 'Passed'
$summary['EditorAssemblyExcluded'] = $true
$summary | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath "$project/summary.json" -Encoding utf8
Get-Content -LiteralPath "$project/summary.json"
