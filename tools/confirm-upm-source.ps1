param(
    [Parameter(Mandatory = $true)][string]$Project,
    [Parameter(Mandatory = $true)]
    [ValidateSet('com.aassder95.unitytools.sheets', 'com.aassder95.unitytools.vfx')]
    [string]$PackageName,
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[a-f0-9]{40}$')]
    [string]$SourceRef
)

$ErrorActionPreference = 'Stop'
$expectedUrl = "https://github.com/aassder95/UnityTools.git?path=/UnityTools/Packages/$PackageName#$SourceRef"
$manifest = Get-Content -LiteralPath "$Project/Packages/manifest.json" -Raw | ConvertFrom-Json
$lock = Get-Content -LiteralPath "$Project/Packages/packages-lock.json" -Raw | ConvertFrom-Json
$entry = $lock.dependencies.$PackageName
if ($manifest.dependencies.$PackageName -ne $expectedUrl -or $entry.source -ne 'git' -or $entry.version -ne $expectedUrl -or $entry.hash -ne $SourceRef) {
    throw '설치 manifest 또는 Git lock이 요청한 소스와 일치하지 않습니다.'
}
$caches = @(Get-ChildItem -LiteralPath "$Project/Library/PackageCache" -Directory -Filter "$PackageName@*")
if ($caches.Count -ne 1) { throw '설치된 package cache를 하나로 확인할 수 없습니다.' }
$package = Get-Content -LiteralPath (Join-Path $caches[0].FullName 'package.json') -Raw | ConvertFrom-Json
if ($package.name -ne $PackageName -or $package.version -ne '0.1.0' -or @($package.dependencies.PSObject.Properties).Count -ne 0) {
    throw '설치된 패키지 이름, 버전 또는 독립 의존성 계약이 일치하지 않습니다.'
}
$report = [ordered]@{ Package = $PackageName; Version = $package.version; SourceRef = $SourceRef; Url = $expectedUrl; Cache = $caches[0].FullName; Status = 'Passed' }
[IO.File]::WriteAllText("$Project/git-source-result.json", ($report | ConvertTo-Json), [Text.UTF8Encoding]::new($false))
