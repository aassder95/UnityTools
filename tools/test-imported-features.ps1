param([string]$UnityVersion = '2022.3.62f3', [string]$OutputBase = $env:TEMP)
$ErrorActionPreference = 'Stop'
$utf8 = [Text.UTF8Encoding]::new($false)
$repoRoot = Split-Path -Parent $PSScriptRoot
$unityPath = "C:\Program Files\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe"
if (!(Test-Path -LiteralPath $unityPath)) { throw "Unity 실행 파일을 찾을 수 없습니다: $unityPath" }
$project = Join-Path $OutputBase "UnityTools-Ports-$UnityVersion-$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path "$project/Assets/Editor", "$project/Packages", "$project/ProjectSettings" -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'CompatibilityValidation.cs') -Destination "$project/Assets/Editor/CompatibilityValidation.cs"
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'NextToolsBuildValidation.cs') -Destination "$project/Assets/Editor/NextToolsBuildValidation.cs"
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'NextToolsPlayerProbe.cs') -Destination "$project/Assets/NextToolsPlayerProbe.cs"
$dependencies = [ordered]@{
    'com.aassder95.unitytools.ui' = 'file:' + (Join-Path $repoRoot 'UnityTools/Packages/com.aassder95.unitytools.ui').Replace('\','/')
    'com.aassder95.unitytools.vfx' = 'file:' + (Join-Path $repoRoot 'UnityTools/Packages/com.aassder95.unitytools.vfx').Replace('\','/')
    'com.aassder95.unitytools.qa' = 'file:' + (Join-Path $repoRoot 'UnityTools/Packages/com.aassder95.unitytools.qa').Replace('\','/')
    'com.aassder95.unitytools.build' = 'file:' + (Join-Path $repoRoot 'UnityTools/Packages/com.aassder95.unitytools.build').Replace('\','/')
    'com.aassder95.unitytools.vat' = 'file:' + (Join-Path $repoRoot 'UnityTools/Packages/com.aassder95.unitytools.vat').Replace('\','/')
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
$manifest = @{ dependencies = $dependencies; testables = @('com.aassder95.unitytools.ui', 'com.aassder95.unitytools.vfx', 'com.aassder95.unitytools.qa', 'com.aassder95.unitytools.build', 'com.aassder95.unitytools.vat') }
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
    $filter = if ($platform -eq 'EditMode') { 'UnityTools.Ui.Editor.Tests;UnityTools.Vfx.Editor.Tests;UnityTools.Ui.Localization.Tests;UnityTools.Ui.Tests.EnumDisplayTests;UnityTools.Ui.Tests.UiAssetCatalogTests;UnityTools.Qa.Tests;UnityTools.Vat.Editor.Tests;UnityTools.Build.Editor.Tests' } else { 'UnityTools.Ui.Localization.Tests;UnityTools.Ui.Tests.EnumDisplayTests;UnityTools.Ui.Tests.UiAssetCatalogTests;UnityTools.Qa.Tests;UnityTools.Vat.Tests' }
    Invoke-PortsUnity $platform @('-runTests', '-testPlatform', $platform, '-testFilter', ('"' + $filter + '"'), '-testResults', ('"' + $project + '/' + $platform + '.xml"'))
    [xml]$result = Get-Content -LiteralPath "$project/$platform.xml" -Raw
    $run = $result.'test-run'
    if ($run.result -ne 'Passed' -or [int]$run.passed -le 0 -or [int]$run.skipped -ne 0) { throw "$platform 테스트 실패: $($run.result) / $($run.failed) / skipped=$($run.skipped)" }
    $summary[$platform] = @{ Passed = [int]$run.passed; Failed = [int]$run.failed; Skipped = [int]$run.skipped }
}
Invoke-PortsUnity 'build' @('-executeMethod', 'NextToolsBuildValidation.Build', '-quit')
$editorAssemblies = @(Get-ChildItem -LiteralPath "$project/Builds" -Recurse -Filter '*.dll' | Where-Object { $_.Name -like 'UnityTools.Vfx*' -or $_.Name -like 'UnityTools.Ui.Editor*' -or $_.Name -like 'UnityTools.Build*' -or $_.Name -like 'UnityTools.Vat.Editor*' -or $_.Name -like 'UnityTools.Qa.Editor*' })
if ($editorAssemblies.Count -gt 0) { throw 'Player 빌드에 Editor 전용 assembly가 포함됐습니다.' }
$player = Start-Process -FilePath "$project/Builds/Ports/Ports.exe" -ArgumentList @('-batchmode', '-logFile', ('"' + $project + '/player.log"')) -PassThru -WindowStyle Hidden
if (!$player.WaitForExit(60000)) { $player.Kill(); throw 'Player 검증 시간 초과' }
$player.Refresh()
if ($player.ExitCode -ne 0 -or !(Select-String -LiteralPath "$project/player.log" -SimpleMatch '이식 도구 Player 검증 통과' -Quiet)) { throw "Player 실행 검증 실패: $project/player.log" }
$summary['PlayerBuild'] = 'Passed'
$summary['PlayerProbe'] = 'Passed'
$summary['BuildPresetSettingsPreserved'] = $true
$summary['EditorAssemblyExcluded'] = $true
$summary | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath "$project/summary.json" -Encoding utf8
Get-Content -LiteralPath "$project/summary.json"
