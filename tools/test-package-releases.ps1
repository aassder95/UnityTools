param(
    [string]$RepositoryUrl = 'https://github.com/aassder95/UnityTools.git',
    [string]$BenchmarkTag = 'unitytools-benchmark/v1.0.0',
    [string]$PersistenceTag = 'unitytools-persistence/v1.0.0',
    [string[]]$UnityVersions = @('2022.3.62f3', '6000.3.20f1'),
    [string]$OutputBase = $env:TEMP
)

$ErrorActionPreference = 'Stop'
$utf8 = [Text.UTF8Encoding]::new($false)
$runRoot = Join-Path $OutputBase ('UTR-' + [Guid]::NewGuid().ToString('N').Substring(0, 12))
New-Item -ItemType Directory -Path $runRoot | Out-Null
$refs = @{}
foreach ($entry in @{ benchmark=$BenchmarkTag; persistence=$PersistenceTag }.GetEnumerator())
{
    $remote = @(git ls-remote --tags $RepositoryUrl "refs/tags/$($entry.Value)" "refs/tags/$($entry.Value)^{}")
    if ($LASTEXITCODE -ne 0 -or $remote.Count -eq 0) { throw "공개 태그 조회 실패: $($entry.Value)" }
    $peeled = @($remote | Where-Object { $_ -match '\^\{\}$' })
    $refs[$entry.Key] = (($peeled + $remote)[0] -split '\s+')[0]
}

$results = @()
foreach ($unityVersion in $UnityVersions)
{
    $versionOutput = Join-Path $runRoot $unityVersion
    New-Item -ItemType Directory -Path $versionOutput | Out-Null
    & (Join-Path $PSScriptRoot 'test-upm-compatibility.ps1') -Source Remote -RepositoryUrl $RepositoryUrl -BenchmarkRef $refs.benchmark -PersistenceRef $refs.persistence -UiRef 'unitytools-ui/v2.0.0' -Scenarios benchmark,persistence,ui-lab,save-lab -UnityVersion $unityVersion -OutputBase $versionOutput
    if ($LASTEXITCODE -ne 0) { throw "Unity 검증 실패: $unityVersion" }
    $files = @(Get-ChildItem -LiteralPath $versionOutput -Filter summary.json -Recurse -File)
    if ($files.Count -ne 1) { throw '검증 summary가 하나여야 합니다.' }
    $summaries = Get-Content -LiteralPath $files[0].FullName -Raw | ConvertFrom-Json
    if ($summaries.Count -ne 4) { throw '네 시나리오의 결과가 모두 필요합니다.' }
    foreach ($scenario in @('benchmark','persistence','ui-lab','save-lab'))
    {
        $matches = @($summaries | Where-Object { $_.Scenario -eq $scenario })
        if ($matches.Count -ne 1) { throw "시나리오 중복 또는 누락: $scenario" }
        $summary = $matches[0]
        [xml]$xml = Get-Content -LiteralPath "$($summary.Project)/results.xml" -Raw
        if ($summary.Unity -ne $unityVersion -or $summary.Build -ne 'Succeeded' -or $xml.'test-run'.result -ne 'Passed' -or [int]$xml.'test-run'.passed -ne [int]$summary.Total -or [int]$summary.Total -le 0 -or !(Test-Path -LiteralPath "$($summary.Project)/Build/Compatibility.exe")) { throw '테스트 또는 빌드 증거 불일치' }
        $name = if ($scenario -in @('benchmark','ui-lab')) { 'benchmark' } else { 'persistence' }
        $packageName = "com.aassder95.unitytools.$name"
        $lock = Get-Content -LiteralPath "$($summary.Project)/Packages/packages-lock.json" -Raw | ConvertFrom-Json
        $installed = $lock.dependencies.$packageName
        if ($installed.source -ne 'git' -or $installed.hash -ne $refs[$name]) { throw '공개 태그와 설치 lock hash 불일치' }
        $cached = @(Get-ChildItem -LiteralPath "$($summary.Project)/Library/PackageCache" -Directory -Filter "$packageName@*")
        if ($cached.Count -ne 1) { throw '설치 cache가 하나여야 합니다.' }
        $manifest = Get-Content -LiteralPath "$($cached[0].FullName)/package.json" -Raw | ConvertFrom-Json
        if ($manifest.name -ne $packageName -or $manifest.version -ne '1.0.0' -or @($manifest.dependencies.PSObject.Properties).Count -ne 0) { throw '패키지 이름, 버전 또는 dependency 불일치' }
        $results += $summary
    }
}
$report = [PSCustomObject]@{ Repository=$RepositoryUrl; BenchmarkTag=$BenchmarkTag; PersistenceTag=$PersistenceTag; Refs=$refs; Results=$results }
[IO.File]::WriteAllText("$runRoot/release-validation.json", ($report | ConvertTo-Json -Depth 8), $utf8)
Write-Host "공개 릴리스 검증 완료: $runRoot/release-validation.json"
