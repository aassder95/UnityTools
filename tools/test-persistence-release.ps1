param(
    [ValidateSet('Candidate', 'Remote')]
    [string]$Source = 'Candidate',
    [string]$Version = '1.0.0',
    [string]$RepositoryUrl = 'https://github.com/aassder95/UnityTools.git',
    [string]$Tag = 'unitytools-persistence/v1.0.0',
    [string[]]$UnityVersions = @('2022.3.62f3', '6000.3.20f1'),
    [string]$OutputBase = $env:TEMP,
    [string]$ResumeFrom
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$packagePath = 'UnityTools/Packages/com.aassder95.unitytools.persistence'
$utf8 = [Text.UTF8Encoding]::new($false)
$strictUtf8 = [Text.UTF8Encoding]::new($false, $true)
$runRoot = if ($ResumeFrom) { [IO.Path]::GetFullPath($ResumeFrom) } else { Join-Path $OutputBase ('UTP-' + [Guid]::NewGuid().ToString('N').Substring(0, 12)) }
if ($ResumeFrom)
{
    if ($Source -ne 'Candidate' -or !(Test-Path -LiteralPath "$runRoot/candidate.git/.git") -or !(Test-Path -LiteralPath "$runRoot/source-files.json")) { throw '재개할 고정 후보 검증 경로가 아닙니다.' }
}
else
{
    New-Item -ItemType Directory -Path $runRoot | Out-Null
}
Write-Host "릴리스 검증 결과 경로: $runRoot"

if ($Source -eq 'Candidate')
{
    $snapshot = Join-Path $runRoot 'candidate.git'
    $packageRoot = if ($ResumeFrom) { Join-Path $snapshot $packagePath } else { Join-Path $repoRoot $packagePath }
    $manifest = Get-Content -LiteralPath "$packageRoot/package.json" -Raw | ConvertFrom-Json
    if ($manifest.version -ne $Version) { throw '로컬 패키지 버전이 검증 버전과 다릅니다.' }
    if (!$ResumeFrom)
    {
        $dest = Join-Path $snapshot $packagePath
        New-Item -ItemType Directory -Path (Split-Path -Parent $dest) -Force | Out-Null
        Copy-Item -LiteralPath $packageRoot -Destination $dest -Recurse
        [IO.File]::WriteAllText("$snapshot/.gitattributes", "* -text`n", $utf8)
        $fingerprints = @(Get-ChildItem -LiteralPath $dest -Recurse -File | Sort-Object FullName | ForEach-Object {
            [PSCustomObject]@{ Path=$_.FullName.Substring($dest.Length + 1).Replace('\','/'); Sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
        })
        [IO.File]::WriteAllText("$runRoot/source-files.json", (ConvertTo-Json -InputObject $fingerprints -Depth 4), $utf8)
        git init --quiet $snapshot
        if ($LASTEXITCODE -ne 0) { throw '후보 검증용 Git 저장소 생성 실패' }
        git -C $snapshot add -- $packagePath .gitattributes
        if ($LASTEXITCODE -ne 0) { throw '후보 검증용 파일 등록 실패' }
        $messagePath = Join-Path $runRoot 'snapshot-message.txt'
        [IO.File]::WriteAllText($messagePath, 'test(persistence): 릴리스 검증 소스 고정', $utf8)
        git -C $snapshot -c user.name=UnityToolsValidation -c user.email=validation@localhost commit --quiet -F $messagePath
        if ($LASTEXITCODE -ne 0) { throw '후보 검증용 commit 생성 실패' }
    }
    else
    {
        $dirty = @(git -C $snapshot status --porcelain)
        if ($LASTEXITCODE -ne 0 -or $dirty.Count -gt 0) { throw '재개 대상 후보 소스가 변경됐습니다.' }
        $fingerprints = Get-Content -LiteralPath "$runRoot/source-files.json" -Raw | ConvertFrom-Json
        $snapshotFiles = @(Get-ChildItem -LiteralPath $packageRoot -Recurse -File)
        if ($snapshotFiles.Count -ne $fingerprints.Count) { throw '재개 대상 후보 파일 구성이 변경됐습니다.' }
        foreach ($fingerprint in $fingerprints)
        {
            $filePath = Join-Path $packageRoot $fingerprint.Path
            if (!(Test-Path -LiteralPath $filePath -PathType Leaf) -or (Get-FileHash -LiteralPath $filePath -Algorithm SHA256).Hash -ne $fingerprint.Sha256) { throw '재개 대상 후보 파일 내용이 변경됐습니다.' }
        }
    }
    $gitRef = (git -C $snapshot rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0) { throw '후보 검증용 commit 조회 실패' }
    $sourceUrl = 'git+' + ([Uri]$snapshot).AbsoluteUri
}
else
{
    $tagRefs = @(git ls-remote --tags $RepositoryUrl "refs/tags/$Tag" "refs/tags/$Tag^{}")
    if ($LASTEXITCODE -ne 0 -or $tagRefs.Count -eq 0) { throw '공개 릴리스 tag를 찾을 수 없습니다.' }
    $peeled = @($tagRefs | Where-Object { $_ -match '\^\{\}$' })
    $gitRef = (($peeled + $tagRefs)[0] -split '\s+')[0]
    $sourceUrl = $RepositoryUrl
}

$results = @()
foreach ($unityVersion in $UnityVersions)
{
    $versionOutput = Join-Path $runRoot $unityVersion
    New-Item -ItemType Directory -Path $versionOutput -Force | Out-Null
    $existing = @(Get-ChildItem -LiteralPath $versionOutput -Filter summary.json -Recurse -File)
    if (!$ResumeFrom -or $existing.Count -eq 0)
    {
        & (Join-Path $PSScriptRoot 'test-upm-compatibility.ps1') -Source Remote -RepositoryUrl $sourceUrl -PersistenceRef $gitRef -Scenarios persistence,save-lab -UnityVersion $unityVersion -OutputBase $versionOutput
        if ($LASTEXITCODE -ne 0) { throw "Unity 검증 실패: $unityVersion" }
    }
    $summaryFiles = @(Get-ChildItem -LiteralPath $versionOutput -Filter summary.json -Recurse -File)
    if ($summaryFiles.Count -ne 1) { throw '호환성 검증 summary가 하나여야 합니다.' }
    $summaries = Get-Content -LiteralPath $summaryFiles[0].FullName -Raw | ConvertFrom-Json
    if ($summaries.Count -ne 2) { throw '코어 단독과 샘플 검증 결과가 모두 필요합니다.' }
    if (@($summaries | Where-Object { $_.Scenario -eq 'persistence' }).Count -ne 1 -or @($summaries | Where-Object { $_.Scenario -eq 'save-lab' }).Count -ne 1) { throw '검증 시나리오가 중복되거나 누락됐습니다.' }
    foreach ($summary in $summaries)
    {
        [xml]$testResult = Get-Content -LiteralPath "$($summary.Project)/results.xml" -Raw
        $testRun = $testResult.'test-run'
        if ($summary.Unity -ne $unityVersion -or $summary.Build -ne 'Succeeded' -or $testRun.result -ne 'Passed' -or [int]$testRun.passed -ne [int]$summary.Total -or [int]$summary.Total -le 0 -or !(Test-Path -LiteralPath "$($summary.Project)/Build/Compatibility.exe")) { throw '테스트 또는 빌드 검증 증거가 일치하지 않습니다.' }
        $lock = Get-Content -LiteralPath "$($summary.Project)/Packages/packages-lock.json" -Raw | ConvertFrom-Json
        $installed = $lock.dependencies.'com.aassder95.unitytools.persistence'
        if ($installed.source -ne 'git' -or $installed.hash -ne $gitRef) { throw '검증 대상 Git commit과 lock 기록이 다릅니다.' }
        $cached = @(Get-ChildItem -LiteralPath "$($summary.Project)/Library/PackageCache" -Directory -Filter 'com.aassder95.unitytools.persistence@*')
        if ($cached.Count -ne 1) { throw '설치된 Persistence package cache가 하나여야 합니다.' }
        $installedManifest = Get-Content -LiteralPath "$($cached[0].FullName)/package.json" -Raw | ConvertFrom-Json
        if ($installedManifest.name -ne 'com.aassder95.unitytools.persistence' -or $installedManifest.version -ne $Version) { throw '설치된 패키지 이름 또는 버전이 다릅니다.' }
        if (@($installedManifest.dependencies.PSObject.Properties).Count -ne 0) { throw '코어 패키지에 외부 dependency가 있습니다.' }
        if ($Source -eq 'Candidate')
        {
            $cachedFiles = @(Get-ChildItem -LiteralPath $cached[0].FullName -Recurse -File)
            if ($cachedFiles.Count -ne $fingerprints.Count) { throw '설치된 후보 파일 구성이 고정 소스와 다릅니다.' }
            foreach ($fingerprint in $fingerprints)
            {
                $filePath = Join-Path $cached[0].FullName $fingerprint.Path
                if (!(Test-Path -LiteralPath $filePath -PathType Leaf)) { throw "설치된 후보 파일이 누락됐습니다: $($fingerprint.Path)" }
                if ((Get-FileHash -LiteralPath $filePath -Algorithm SHA256).Hash -ne $fingerprint.Sha256)
                {
                    $sourceText = $strictUtf8.GetString([IO.File]::ReadAllBytes((Join-Path $packageRoot $fingerprint.Path))).Replace("`r`n", "`n")
                    $cachedText = $strictUtf8.GetString([IO.File]::ReadAllBytes($filePath)).Replace("`r`n", "`n")
                    if ($fingerprint.Path -eq 'package.json')
                    {
                        $sourceJson = $sourceText | ConvertFrom-Json
                        $cachedJson = $cachedText | ConvertFrom-Json
                        $cachedJson.PSObject.Properties.Remove('_fingerprint')
                        $sourceText = $sourceJson | ConvertTo-Json -Depth 32 -Compress
                        $cachedText = $cachedJson | ConvertTo-Json -Depth 32 -Compress
                    }
                    if ($sourceText -cne $cachedText) { throw "설치된 후보 파일 내용이 고정 소스와 다릅니다: $($fingerprint.Path)" }
                }
            }
        }
        $results += $summary
    }
}

$report = [PSCustomObject]@{ Source=$Source; Version=$Version; Repository=$sourceUrl; GitRef=$gitRef; Tag=($(if ($Source -eq 'Remote') { $Tag } else { $null })); Results=$results }
[IO.File]::WriteAllText("$runRoot/release-validation.json", ($report | ConvertTo-Json -Depth 6), $utf8)
Write-Host "Persistence 릴리스 검증 완료: $runRoot/release-validation.json"
