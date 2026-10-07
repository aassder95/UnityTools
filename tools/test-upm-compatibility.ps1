param(
    [string]$UnityVersion = '6000.3.20f1',
    [string]$UnityPath,
    [ValidateSet('Local', 'Remote')]
    [string]$Source = 'Remote',
    [string]$RepositoryUrl = 'https://github.com/aassder95/UnityTools.git',
    [string]$UiRef = '18665ab98b9bc97c9f664a6a0407ba1353f59145',
    [string]$TimerRef = '18665ab98b9bc97c9f664a6a0407ba1353f59145',
    [string]$BenchmarkRef = '18665ab98b9bc97c9f664a6a0407ba1353f59145',
    [string]$PersistenceRef = '472a08ef24418e81045dcc66109c1a9b7c86606b',
    [string]$OutputBase = $env:TEMP,
    [string[]]$Scenarios = @('timer', 'ui', 'ui-input', 'benchmark', 'persistence', 'ui-lab')
)

$ErrorActionPreference = 'Stop'
$utf8 = [Text.UTF8Encoding]::new($false)
$repoRoot = Split-Path -Parent $PSScriptRoot
if (!$UnityPath) { $UnityPath = "C:\Program Files\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe" }
if (!(Test-Path -LiteralPath $UnityPath)) { throw "Unity 실행 파일을 찾을 수 없습니다: $UnityPath" }
$runRoot = Join-Path $OutputBase "UnityTools-Compatibility-$UnityVersion-$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $runRoot -Force | Out-Null
Write-Host "검증 결과 경로: $runRoot"

function Get-Reference([string]$name, [string]$ref)
{
    $path = "UnityTools/Packages/com.aassder95.unitytools.$name"
    if ($Source -eq 'Remote') { return "$RepositoryUrl`?path=/$path#$ref" }
    return 'file:' + (Join-Path $repoRoot $path).Replace('\', '/')
}

function Invoke-Unity([string]$project, [string]$step, [string[]]$extra)
{
    $log = Join-Path $project "$step.log"
    $arguments = @('-batchmode', '-nographics', '-projectPath', ('"' + $project + '"'), '-logFile', ('"' + $log + '"')) + $extra
    $process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -PassThru -WindowStyle Hidden
    $process.WaitForExit()
    $process.Refresh()
    if ($process.ExitCode -ne 0) { throw "$step 실패 (exit=$($process.ExitCode)). 로그: $log" }
}

$summaries = @()
foreach ($scenario in $Scenarios)
{
    if ($scenario -notin @('timer','ui','ui-input','benchmark','persistence','ui-lab','save-lab','timer-lab','timer-dashboard','showcase')) { throw "알 수 없는 시나리오: $scenario" }
    if ($scenario -eq 'showcase' -and $Source -ne 'Local') { throw 'Showcase는 현재 로컬 소스로만 검증합니다. -Source Local을 지정하세요.' }
    $project = Join-Path $runRoot $scenario
    New-Item -ItemType Directory -Path "$project/Assets/Editor", "$project/Packages", "$project/ProjectSettings" -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'CompatibilityValidation.cs') -Destination "$project/Assets/Editor/CompatibilityValidation.cs"
    $dependencies = [ordered]@{
        'com.unity.test-framework' = '1.1.33'
        'com.unity.modules.ui' = '1.0.0'
        'com.unity.modules.imgui' = '1.0.0'
        'com.unity.modules.jsonserialize' = '1.0.0'
    }
    $packages = @()
    $samples = @()
    $filter = ''
    switch ($scenario)
    {
        'timer' { $packages = @('timer'); $samples = @('timer'); $filter = 'UnityTools.Timer.Tests' }
        'ui' { $packages = @('ui'); $filter = 'UnityTools.Ui.Tests' }
        'ui-input' { $packages = @('ui'); $samples = @('ui'); $filter = 'UnityTools.Ui.Tests'; $dependencies['com.unity.inputsystem'] = '1.14.0' }
        'benchmark' { $packages = @('benchmark'); $filter = 'UnityTools.Benchmark.Tests' }
        'persistence' { $packages = @('persistence'); $filter = 'UnityTools.Persistence.Tests' }
        'timer-dashboard' { $packages = @('timer','ui'); $filter = 'UnityTools.TimerDashboard.Tests' }
        'timer-lab' { $packages = @('timer'); $samples = @('timer'); $filter = 'UnityTools.Timer'; $dependencies['com.unity.ugui'] = '1.0.0' }
        'save-lab' { $packages = @('persistence'); $samples = @('persistence'); $filter = 'UnityTools.Persistence'; $dependencies['com.unity.ugui'] = '1.0.0' }
        'ui-lab' { $packages = @('benchmark','ui'); $samples = @('benchmark'); $filter = 'UnityTools.Benchmark' }
        'showcase' { $packages = @('ui','benchmark','persistence','timer'); $samples = @('benchmark','persistence','timer'); $filter = 'UnityTools.Showcase.Tests' }
    }
    foreach ($package in $packages)
    {
        $ref = switch ($package) { 'ui' { $UiRef }; 'timer' { $TimerRef }; 'benchmark' { $BenchmarkRef }; 'persistence' { $PersistenceRef } }
        $dependencies["com.aassder95.unitytools.$package"] = Get-Reference $package $ref
    }
    $manifest = @{ dependencies = $dependencies; testables = @($packages | ForEach-Object { "com.aassder95.unitytools.$_" }) }
    [IO.File]::WriteAllText("$project/Packages/manifest.json", ($manifest | ConvertTo-Json -Depth 8), $utf8)
    [IO.File]::WriteAllText("$project/ProjectSettings/ProjectVersion.txt", "m_EditorVersion: $UnityVersion", $utf8)
    [IO.File]::WriteAllLines("$project/ValidationSamples.txt", [string[]]@($samples | ForEach-Object { "com.aassder95.unitytools.$_" }), $utf8)
    if ($scenario -eq 'timer') { [IO.File]::WriteAllLines("$project/ValidationSampleNames.txt", [string[]]@('Timer Sample Scene'), $utf8) }
    if ($scenario -eq 'showcase') { [IO.File]::WriteAllLines("$project/ValidationSampleNames.txt", [string[]]@('UI Performance Lab', 'Save Recovery Lab', 'Timer Simulation Lab'), $utf8) }
    Write-Host "$scenario : Git 설치 및 샘플 Import"
    Invoke-Unity $project 'import' @('-executeMethod','CompatibilityValidation.ImportSamples','-quit')
    if (!(Test-Path "$project/sample-imports.txt")) { throw "Import 완료 기록 누락: $project" }
    if ((Get-Content -LiteralPath "$project/editor-version.txt" -Raw) -ne $UnityVersion) { throw "설정한 Unity 버전과 실행한 Editor 버전이 다릅니다: $project" }
    $lock = Get-Content -LiteralPath "$project/Packages/packages-lock.json" -Raw | ConvertFrom-Json
    foreach ($package in $packages)
    {
        $installed = $lock.dependencies."com.aassder95.unitytools.$package"
        if ($Source -eq 'Remote' -and ($installed.source -ne 'git' -or !$installed.hash)) { throw "Git 패키지 설치 기록 누락: $package" }
    }
    if ($scenario -eq 'showcase')
    {
        Copy-Item -LiteralPath (Join-Path $repoRoot 'UnityTools/Assets/Showcase') -Destination "$project/Assets/Showcase" -Recurse
        Invoke-Unity $project 'showcase' @('-executeMethod','UnityTools.Showcase.Editor.ShowcaseSceneBuilder.BuildValidationScene','-quit')
        if (!(Test-Path "$project/Assets/Showcase/Showcase.unity")) { throw "Showcase 생성 장면 누락: $project" }
        [IO.File]::WriteAllText("$project/ValidationEntryScene.txt", 'Assets/Showcase/Showcase.unity', $utf8)
    }
    if ($scenario -eq 'timer-dashboard')
    {
        if ($Source -ne 'Local') { throw 'Timer Dashboard는 -Source Local로 검증합니다.' }
        Copy-Item -LiteralPath (Join-Path $repoRoot 'UnityTools/Assets/TimerDashboard') -Destination "$project/Assets/TimerDashboard" -Recurse
        Invoke-Unity $project 'dashboard' @('-executeMethod','UnityTools.TimerDashboard.Editor.TimerDashboardSceneBuilder.BuildValidationScene','-quit')
        [IO.File]::WriteAllText("$project/ValidationEntryScene.txt", 'Assets/TimerDashboard/TimerDashboard.unity', $utf8)
    }
    Invoke-Unity $project 'scenes' @('-executeMethod','CompatibilityValidation.PrepareScenes','-quit')
    if (!(Test-Path "$project/scenes-ready.txt")) { throw "장면 검증 기록 누락: $project" }
    $resultPath = "$project/results.xml"
    Write-Host "$scenario : Play Mode 테스트"
    Invoke-Unity $project 'tests' @('-runTests','-testPlatform','PlayMode','-testFilter',$filter,'-testResults',('"' + $resultPath + '"'))
    [xml]$result = Get-Content -LiteralPath $resultPath -Raw
    $run = $result.'test-run'
    if ($run.result -ne 'Passed' -or [int]$run.total -le 0) { throw "$scenario 테스트 실패: $($run.result) / $($run.failed)" }
    Write-Host "$scenario : Windows Development Build"
    Invoke-Unity $project 'build' @('-executeMethod','CompatibilityValidation.BuildPlayer','-quit')
    if (!(Test-Path "$project/build-result.txt") -or !(Test-Path "$project/Build/Compatibility.exe")) { throw "빌드 결과 누락: $project" }
    $summary = [PSCustomObject]@{ Scenario=$scenario; Unity=$UnityVersion; Source=$Source; Passed=[int]$run.passed; Total=[int]$run.total; Build='Succeeded'; Project=$project }
    $summaries += $summary
    [IO.File]::WriteAllText("$runRoot/summary.json", (ConvertTo-Json -InputObject @($summaries) -Depth 4), $utf8)
    $summary | Format-Table -AutoSize
}
Write-Host "패키지 호환성 검증 완료: $runRoot"
