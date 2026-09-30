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
