param(
    [ValidateSet('Local', 'Remote')]
    [string]$Source = 'Local',
    [string]$UnityPath = 'C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe',
    [string]$RepositoryUrl = 'https://github.com/aassder95/UnityTools.git',
    [string]$UiRef = 'unitytools-ui/v2.0.0',
    [string]$TimerRef = 'unitytools-timer/v1.0.0',
    [string]$OutputBase = $env:TEMP,
    [switch]$KeepProjects
)

$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()
$OutputEncoding = [Console]::OutputEncoding
$repoRoot = Split-Path -Parent $PSScriptRoot
$runRoot = Join-Path $OutputBase "UnityTools-UPM-$([Guid]::NewGuid().ToString('N'))"
$uiPackagePath = 'UnityTools/Packages/com.aassder95.unitytools.ui'
$timerPackagePath = 'UnityTools/Packages/com.aassder95.unitytools.timer'

function Get-PackageReference([string]$packagePath, [string]$ref)
{
    if ($Source -eq 'Remote')
    {
        return "$RepositoryUrl`?path=/$packagePath#$ref"
    }

    $absolutePath = Join-Path $repoRoot $packagePath
    return 'file:' + $absolutePath.Replace('\', '/')
}

function New-ValidationProject([string]$name, [System.Collections.IDictionary]$dependencies, [string[]]$testables)
{
    $projectPath = Join-Path $runRoot $name
    $assetsPath = Join-Path $projectPath 'Assets'
    $packagesPath = Join-Path $projectPath 'Packages'
    $settingsPath = Join-Path $projectPath 'ProjectSettings'
    New-Item -ItemType Directory -Path $assetsPath, $packagesPath, $settingsPath -Force | Out-Null

    $manifest = [ordered]@{
        dependencies = $dependencies
        testables = $testables
    }

    $manifestJson = $manifest | ConvertTo-Json -Depth 8
    [System.IO.File]::WriteAllText((Join-Path $packagesPath 'manifest.json'), $manifestJson, [System.Text.UTF8Encoding]::new($false))

    $projectVersion = @"
m_EditorVersion: 2022.3.62f3
m_EditorVersionWithRevision: 2022.3.62f3 (96770f904ca7)
"@
    [System.IO.File]::WriteAllText((Join-Path $settingsPath 'ProjectVersion.txt'), $projectVersion, [System.Text.UTF8Encoding]::new($false))
    return $projectPath
}

function Invoke-PackageTests([string]$name, [System.Collections.IDictionary]$dependencies, [string[]]$testables, [string]$testFilter)
{
    $projectPath = New-ValidationProject $name $dependencies $testables
    $logPath = Join-Path $runRoot "$name.log"
    $resultPath = Join-Path $runRoot "$name-results.xml"
    $arguments = @(
        '-batchmode',
        '-nographics',
        '-projectPath', $projectPath,
        '-runTests',
        '-testPlatform', 'PlayMode',
        '-testFilter', $testFilter,
        '-testResults', $resultPath,
        '-logFile', $logPath
    )

    $process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -PassThru -Wait -WindowStyle Hidden
    if (!(Test-Path -LiteralPath $resultPath))
    {
        $logTail = Get-Content -LiteralPath $logPath -Tail 80
        throw "$name 결과 파일이 생성되지 않았습니다.`n$($logTail -join [Environment]::NewLine)"
    }

    [xml]$results = Get-Content -LiteralPath $resultPath -Raw
    $run = $results.'test-run'
    if ($process.ExitCode -ne 0 -or $run.result -ne 'Passed' -or [int]$run.total -le 0)
    {
        throw "$name 검증 실패: exit=$($process.ExitCode), result=$($run.result), total=$($run.total), passed=$($run.passed), failed=$($run.failed)"
    }

    return [PSCustomObject]@{
        Scenario = $name
        Result = $run.result
        Total = [int]$run.total
        Passed = [int]$run.passed
        Failed = [int]$run.failed
    }
}

if (!(Test-Path -LiteralPath $UnityPath))
{
    throw "Unity 실행 파일을 찾을 수 없습니다: $UnityPath"
}

New-Item -ItemType Directory -Path $runRoot -Force | Out-Null
try
{
    $uiReference = Get-PackageReference $uiPackagePath $UiRef
    $timerReference = Get-PackageReference $timerPackagePath $TimerRef
    $commonDependencies = [ordered]@{
        'com.unity.test-framework' = '1.1.33'
    }

    $timerDependencies = [ordered]@{}
    foreach ($entry in $commonDependencies.GetEnumerator())
    {
        $timerDependencies[$entry.Key] = $entry.Value
    }
    $timerDependencies['com.aassder95.unitytools.timer'] = $timerReference

    $uiDependencies = [ordered]@{}
    foreach ($entry in $commonDependencies.GetEnumerator())
    {
        $uiDependencies[$entry.Key] = $entry.Value
    }
    $uiDependencies['com.aassder95.unitytools.ui'] = $uiReference

    $uiInputDependencies = [ordered]@{}
    foreach ($entry in $uiDependencies.GetEnumerator())
    {
        $uiInputDependencies[$entry.Key] = $entry.Value
    }
    $uiInputDependencies['com.unity.inputsystem'] = '1.14.0'

    $summaries = @()
    $summaries += Invoke-PackageTests 'timer-only' $timerDependencies @('com.aassder95.unitytools.timer') 'UnityTools.Timer.Tests'
    $summaries += Invoke-PackageTests 'ui-without-input-system' $uiDependencies @('com.aassder95.unitytools.ui') 'UnityTools.Ui.Tests'
    $summaries += Invoke-PackageTests 'ui-with-input-system' $uiInputDependencies @('com.aassder95.unitytools.ui') 'UnityTools.Ui.Tests'
    $summaries | Format-Table -AutoSize
    Write-Host "UPM package validation passed. Source=$Source"
}
finally
{
    if ($KeepProjects)
    {
        Write-Host "Validation projects kept at $runRoot"
    }
    elseif (Test-Path -LiteralPath $runRoot)
    {
        Remove-Item -LiteralPath $runRoot -Recurse -Force
    }
}
