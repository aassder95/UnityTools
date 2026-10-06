$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$packageScripts = (Resolve-Path -LiteralPath (Join-Path $repoRoot 'UnityTools/Packages/com.aassder95.unitytools.ui/Samples~/UI Sample Scene/Scripts')).Path
$projectScripts = (Resolve-Path -LiteralPath (Join-Path $repoRoot 'UnityTools/Assets/Samples')).Path
$sourceFiles = @(Get-ChildItem -LiteralPath $packageScripts -Recurse -File -Filter '*.cs')

foreach ($sourceFile in $sourceFiles)
{
    $relativePath = $sourceFile.FullName.Substring($packageScripts.Length + 1)
    $projectFile = Join-Path $projectScripts $relativePath
    if (!(Test-Path -LiteralPath $projectFile))
    {
        throw "Mirrored sample is missing: $relativePath"
    }

    $sourceHash = (Get-FileHash -LiteralPath $sourceFile.FullName -Algorithm SHA256).Hash
    $projectHash = (Get-FileHash -LiteralPath $projectFile -Algorithm SHA256).Hash
    if ($sourceHash -ne $projectHash)
    {
        throw "Mirrored sample differs: $relativePath"
    }
}

Write-Host "UI sample C# mirrors match: $($sourceFiles.Count) files"

$labSource = (Resolve-Path -LiteralPath (Join-Path $repoRoot 'UnityTools/Packages/com.aassder95.unitytools.benchmark/Samples~/UI Performance Lab')).Path
$labMirror = (Resolve-Path -LiteralPath (Join-Path $repoRoot 'UnityTools/Assets/PerformanceLab')).Path
$labFiles = @(Get-ChildItem -LiteralPath $labSource -Recurse -File)
foreach ($sourceFile in $labFiles)
{
    $relativePath = $sourceFile.FullName.Substring($labSource.Length + 1)
    $projectFile = Join-Path $labMirror $relativePath
    if (!(Test-Path -LiteralPath $projectFile) -or (Get-FileHash -LiteralPath $sourceFile.FullName -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath $projectFile -Algorithm SHA256).Hash)
    {
        throw "Performance Lab sample differs or is missing: $relativePath"
    }
}

Write-Host "Performance Lab sample mirrors match: $($labFiles.Count) files"

$saveSource = (Resolve-Path -LiteralPath (Join-Path $repoRoot 'UnityTools/Packages/com.aassder95.unitytools.persistence/Samples~/Save Recovery Lab')).Path
$saveMirror = (Resolve-Path -LiteralPath (Join-Path $repoRoot 'UnityTools/Assets/SaveRecoveryLab')).Path
$saveFiles = @(Get-ChildItem -LiteralPath $saveSource -Recurse -File)
foreach ($sourceFile in $saveFiles)
{
    $relativePath = $sourceFile.FullName.Substring($saveSource.Length + 1)
    $projectFile = Join-Path $saveMirror $relativePath
    if (!(Test-Path -LiteralPath $projectFile) -or (Get-FileHash -LiteralPath $sourceFile.FullName -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath $projectFile -Algorithm SHA256).Hash)
    {
        throw "Save Recovery Lab sample differs or is missing: $relativePath"
    }
}

Write-Host "Save Recovery Lab sample mirrors match: $($saveFiles.Count) files"

$timerSource = (Resolve-Path -LiteralPath (Join-Path $repoRoot 'UnityTools/Packages/com.aassder95.unitytools.timer/Samples~/Timer Simulation Lab')).Path
$timerMirror = (Resolve-Path -LiteralPath (Join-Path $repoRoot 'UnityTools/Assets/TimerSimulationLab')).Path
$timerFiles = @(Get-ChildItem -LiteralPath $timerSource -Recurse -File)
foreach ($sourceFile in $timerFiles)
{
    $relativePath = $sourceFile.FullName.Substring($timerSource.Length + 1)
    $projectFile = Join-Path $timerMirror $relativePath
    if (!(Test-Path -LiteralPath $projectFile) -or (Get-FileHash -LiteralPath $sourceFile.FullName -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath $projectFile -Algorithm SHA256).Hash)
    {
        throw "Timer Simulation Lab sample differs or is missing: $relativePath"
    }
}

Write-Host "Timer Simulation Lab sample mirrors match: $($timerFiles.Count) files"

$rewardSource = (Resolve-Path -LiteralPath (Join-Path $repoRoot 'UnityTools/Packages/com.aassder95.unitytools.ui/Samples~/Reward Flyer Sample')).Path
$rewardMirror = (Resolve-Path -LiteralPath (Join-Path $repoRoot 'UnityTools/Assets/RewardFlyerSample')).Path
$rewardFiles = @(Get-ChildItem -LiteralPath $rewardSource -Recurse -File)
foreach ($sourceFile in $rewardFiles)
{
    $relativePath = $sourceFile.FullName.Substring($rewardSource.Length + 1)
    $projectFile = Join-Path $rewardMirror $relativePath
    if (!(Test-Path -LiteralPath $projectFile) -or (Get-FileHash -LiteralPath $sourceFile.FullName -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath $projectFile -Algorithm SHA256).Hash)
    {
        throw "Reward Flyer sample differs or is missing: $relativePath"
    }
}

Write-Host "Reward Flyer sample mirrors match: $($rewardFiles.Count) files"
