param(
    [string]$SourceRef = 'HEAD',
    [string]$UiVersion = '2.1.0',
    [string]$TimerVersion = '1.1.0',
    [string[]]$UnityVersions = @('2022.3.62f3', '6000.3.20f1'),
    [string]$OutputBase = $env:TEMP
)

$ErrorActionPreference = 'Stop'
$utf8 = [Text.UTF8Encoding]::new($false)
$repoRoot = Split-Path -Parent $PSScriptRoot
$sourceCommit = (& git -C $repoRoot rev-parse --verify "$SourceRef^{commit}").Trim()
if ($LASTEXITCODE -ne 0) { throw '검증 대상 commit을 해석할 수 없습니다.' }
foreach ($version in @($UiVersion, $TimerVersion))
{
    if ($version -notmatch '^\d+\.\d+\.\d+$') { throw '후보 버전은 major.minor.patch 형식이어야 합니다.' }
}

$runRoot = Join-Path $OutputBase "ut-release-$([Guid]::NewGuid().ToString('N').Substring(0, 8))"
$candidate = Join-Path $runRoot 'candidate'
New-Item -ItemType Directory -Path $candidate -Force | Out-Null
$archive = Join-Path $runRoot 'source.zip'
& git -C $repoRoot archive --format=zip -o $archive $sourceCommit UnityTools/Packages/com.aassder95.unitytools.ui UnityTools/Packages/com.aassder95.unitytools.timer
if ($LASTEXITCODE -ne 0) { throw '커밋된 패키지 소스 복사에 실패했습니다.' }
Expand-Archive -LiteralPath $archive -DestinationPath $candidate
foreach ($entry in @(@('ui', $UiVersion), @('timer', $TimerVersion)))
{
    $name = $entry[0]
    $version = $entry[1]
    $packageRoot = Join-Path $candidate "UnityTools/Packages/com.aassder95.unitytools.$name"
    $manifestPath = Join-Path $packageRoot 'package.json'
    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    $oldVersion = $manifest.version
    $manifest.version = $version
    foreach ($field in @('documentationUrl', 'changelogUrl', 'licensesUrl'))
    {
        $manifest.$field = $manifest.$field.Replace("unitytools-$name/v$oldVersion", "unitytools-$name/v$version")
    }
    [IO.File]::WriteAllText($manifestPath, ($manifest | ConvertTo-Json -Depth 16) + "`n", $utf8)
    $changelogPath = Join-Path $packageRoot 'CHANGELOG.md'
    $changelog = [IO.File]::ReadAllText($changelogPath).Replace("`r`n", "`n")
    $candidateHeader = [regex]::Match($changelog, '(?m)^##(?: Unreleased| \[Unreleased\]| \[\d+\.\d+\.\d+\] - Unreleased)$')
    if (!$candidateHeader.Success) { throw "미게시 변경 기록이 없습니다: $name" }
    $changelog = $changelog.Remove($candidateHeader.Index, $candidateHeader.Length).Insert($candidateHeader.Index, "## [$version] - Unreleased")
    [IO.File]::WriteAllText($changelogPath, $changelog, $utf8)
}

& git -C $candidate init --quiet
& git -C $candidate -c core.autocrlf=false add -- UnityTools/Packages
& git -C $candidate -c user.name='UnityTools release validation' -c user.email='validation@localhost' commit --quiet -m 'chore(release): 패키지 버전 후보 준비'
if ($LASTEXITCODE -ne 0) { throw '임시 후보 commit 생성에 실패했습니다.' }
$candidateCommit = (& git -C $candidate rev-parse HEAD).Trim()
$sourceUrl = 'git+file:///' + $candidate.Replace('\', '/')
$report = [ordered]@{ SourceCommit = $sourceCommit; CandidateCommit = $candidateCommit; UiVersion = $UiVersion; TimerVersion = $TimerVersion; Published = $false; Results = @() }
Write-Host "후보 검증 경로: $runRoot"
foreach ($unityVersion in $UnityVersions)
{
    $versionOutput = Join-Path $runRoot $unityVersion
    New-Item -ItemType Directory -Path $versionOutput -Force | Out-Null
    & (Join-Path $PSScriptRoot 'test-upm-compatibility.ps1') -Source Remote -RepositoryUrl $sourceUrl -UiRef $candidateCommit -TimerRef $candidateCommit -Scenarios ui,ui-input,timer,timer-lab -UnityVersion $unityVersion -OutputBase $versionOutput
    $summaries = @(Get-ChildItem -LiteralPath $versionOutput -Recurse -Filter summary.json -File)
    if ($summaries.Count -ne 1) { throw '후보 검증 summary가 하나여야 합니다.' }
    $results = Get-Content -LiteralPath $summaries[0].FullName -Raw | ConvertFrom-Json
    if ($results.Count -ne 4) { throw '네 가지 검증 시나리오가 모두 필요합니다.' }
    foreach ($scenario in @('ui', 'ui-input', 'timer', 'timer-lab'))
    {
        if (@($results | Where-Object { $_.Scenario -eq $scenario }).Count -ne 1) { throw '검증 시나리오가 중복되거나 누락됐습니다.' }
    }
    foreach ($result in $results)
    {
        [xml]$xml = Get-Content -LiteralPath "$($result.Project)/results.xml" -Raw
        if ($result.Unity -ne $unityVersion -or $xml.'test-run'.result -ne 'Passed' -or [int]$xml.'test-run'.total -le 0 -or [int]$xml.'test-run'.passed -ne [int]$result.Total -or $result.Build -ne 'Succeeded' -or !(Test-Path -LiteralPath "$($result.Project)/Build/Compatibility.exe")) { throw '후보 테스트 또는 빌드가 통과하지 않았습니다.' }
        $lock = Get-Content -LiteralPath "$($result.Project)/Packages/packages-lock.json" -Raw | ConvertFrom-Json
        $packageName = if ($result.Scenario -like 'ui*') { 'ui' } else { 'timer' }
        $expectedVersion = if ($packageName -eq 'ui') { $UiVersion } else { $TimerVersion }
        $installed = $lock.dependencies."com.aassder95.unitytools.$packageName"
        if ($installed.source -ne 'git' -or $installed.hash -ne $candidateCommit) { throw 'Git 설치 소스와 고정 후보 commit이 다릅니다.' }
        $cached = @(Get-ChildItem -LiteralPath "$($result.Project)/Library/PackageCache" -Directory -Filter "com.aassder95.unitytools.$packageName@*")
        if ($cached.Count -ne 1) { throw '설치된 패키지 cache가 하나여야 합니다.' }
        $installedManifest = Get-Content -LiteralPath "$($cached[0].FullName)/package.json" -Raw | ConvertFrom-Json
        if ($installedManifest.name -ne "com.aassder95.unitytools.$packageName" -or $installedManifest.version -ne $expectedVersion) { throw '설치된 후보 package 이름 또는 version이 다릅니다.' }
        $report.Results += $result
    }
}
[IO.File]::WriteAllText((Join-Path $runRoot 'release-validation.json'), ($report | ConvertTo-Json -Depth 16), $utf8)
Write-Host "UI/Timer 미게시 후보 검증 완료: $runRoot"
