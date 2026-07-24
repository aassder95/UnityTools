param(
    [switch]$Release
)

$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()
$OutputEncoding = [Console]::OutputEncoding
$repoRoot = Split-Path -Parent $PSScriptRoot
$uiPackagePath = 'UnityTools/Packages/com.aassder95.unitytools.ui'
$timerPackagePath = 'UnityTools/Packages/com.aassder95.unitytools.timer'

function Get-DuplicatePackageGuids([string]$packagePath)
{
    $guidOwners = @{}
    foreach ($metaPath in (git ls-files "$packagePath/**/*.meta"))
    {
        $guidLine = Get-Content -LiteralPath $metaPath -TotalCount 3 | Select-String '^guid: ([0-9a-f]{32})$'
        if (!$guidLine)
        {
            continue
        }

        $guid = $guidLine.Matches[0].Groups[1].Value
        if (!$guidOwners.ContainsKey($guid))
        {
            $guidOwners[$guid] = [System.Collections.Generic.List[string]]::new()
        }

        $guidOwners[$guid].Add($metaPath)
    }

    return @($guidOwners.GetEnumerator() | Where-Object { $_.Value.Count -gt 1 })
}

Push-Location $repoRoot
try
{
    $failures = [System.Collections.Generic.List[string]]::new()

    $originUrl = git remote get-url origin 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($originUrl))
    {
        $failures.Add('origin URL을 확인할 수 없습니다.')
    }
    else
    {
        $originUri = $null
        if (![Uri]::TryCreate($originUrl, [UriKind]::Absolute, [ref]$originUri) -or $originUri.Scheme -ne 'https')
        {
            $failures.Add('origin URL은 credential 없는 일반 HTTPS 주소여야 합니다.')
        }
        elseif (-not [string]::IsNullOrEmpty($originUri.UserInfo))
        {
            $failures.Add('origin URL에 credential이 포함되어 있습니다.')
        }
    }

    $historyPaths = git rev-list --objects --all
    $forbiddenHistory = @($historyPaths | Select-String -Pattern '(?i)(Com\.ForbiddenByte|(^|/)[^/]*OSA[^/]*(/|$)|(^|/)DOTweenPro([^/]*)(/|$))')
    if ($forbiddenHistory.Count -gt 0)
    {
        $failures.Add("금지된 에셋 경로가 Git 이력에 남아 있습니다. 개수=$($forbiddenHistory.Count)")
    }

    git grep -I -q -E 'gh[pousr]_[A-Za-z0-9]{20,}'
    if ($LASTEXITCODE -eq 0)
    {
        $failures.Add('추적 파일에서 GitHub credential 패턴이 발견되었습니다.')
    }

    git grep -I -q -E 'OSA\.Core|Com\.ForbiddenByte|DOTweenPro' -- 'UnityTools/**'
    if ($LASTEXITCODE -eq 0)
    {
        $failures.Add('추적 파일에서 금지된 에셋 식별자가 발견되었습니다.')
    }

    $packageFiles = @(git ls-files "$uiPackagePath/**" "$timerPackagePath/**")
    $forbiddenPackageFiles = @($packageFiles | Select-String -Pattern '(?i)(DOTween|Com\.ForbiddenByte|(^|/)[^/]*OSA[^/]*(/|$))')
    if ($forbiddenPackageFiles.Count -gt 0)
    {
        $failures.Add("UPM 패키지에 배포 금지 vendor 파일이 포함되어 있습니다. 개수=$($forbiddenPackageFiles.Count)")
    }

    git grep -I -q -E 'UnityTools\.Util|OSA\.Core|Com\.ForbiddenByte|DOTweenPro' -- "$uiPackagePath/Runtime/**" "$uiPackagePath/Editor/**" "$uiPackagePath/Samples~/**" "$uiPackagePath/Tests/**" "$timerPackagePath/Runtime/**" "$timerPackagePath/Samples~/**" "$timerPackagePath/Tests/**"
    if ($LASTEXITCODE -eq 0)
    {
        $failures.Add('패키지 코드 또는 샘플에 이전 namespace나 금지 식별자가 남아 있습니다.')
    }

    $uiManifest = Get-Content -LiteralPath "$uiPackagePath/package.json" -Raw | ConvertFrom-Json
    $timerManifest = Get-Content -LiteralPath "$timerPackagePath/package.json" -Raw | ConvertFrom-Json
    if ($uiManifest.name -ne 'com.aassder95.unitytools.ui' -or $timerManifest.name -ne 'com.aassder95.unitytools.timer')
    {
        $failures.Add('UPM package name이 릴리스 계약과 일치하지 않습니다.')
    }

    if ($uiManifest.dependencies.PSObject.Properties.Name -contains 'com.aassder95.unitytools.timer' -or $uiManifest.dependencies.PSObject.Properties.Name -contains 'com.unity.inputsystem')
    {
        $failures.Add('UI 기본 패키지는 Timer 또는 Input System을 필수 dependency로 가질 수 없습니다.')
    }

    if (@($timerManifest.dependencies.PSObject.Properties).Count -ne 0)
    {
        $failures.Add('Timer 패키지에 불필요한 외부 dependency가 있습니다.')
    }

    $uiAsmdef = Get-Content -LiteralPath "$uiPackagePath/Runtime/UnityTools.Ui.asmdef" -Raw | ConvertFrom-Json
    $timerAsmdef = Get-Content -LiteralPath "$timerPackagePath/Runtime/UnityTools.Timer.asmdef" -Raw | ConvertFrom-Json
    $inputAsmdef = Get-Content -LiteralPath "$uiPackagePath/Runtime/InputSystem/UnityTools.Ui.InputSystem.asmdef" -Raw | ConvertFrom-Json
    if ($uiAsmdef.name -ne 'UnityTools.Ui' -or $timerAsmdef.name -ne 'UnityTools.Timer')
    {
        $failures.Add('런타임 assembly name이 릴리스 계약과 일치하지 않습니다.')
    }

    if ($inputAsmdef.references -notcontains 'Unity.InputSystem' -or $inputAsmdef.defineConstraints -notcontains 'UNITYTOOLS_INPUT_SYSTEM')
    {
        $failures.Add('선택적 Input System assembly의 reference 또는 define constraint가 올바르지 않습니다.')
    }

    foreach ($packagePath in @($uiPackagePath, $timerPackagePath))
    {
        $duplicateGuids = @(Get-DuplicatePackageGuids $packagePath)
        if ($duplicateGuids.Count -gt 0)
        {
            $failures.Add("$packagePath 안에 중복 meta GUID가 있습니다. 개수=$($duplicateGuids.Count)")
        }
    }

    if ($Release)
    {
        if ($uiManifest.version -ne '2.0.0' -or $timerManifest.version -ne '1.0.0')
        {
            $failures.Add('UI 또는 Timer package version이 릴리스 버전과 일치하지 않습니다.')
        }

        if (!(Test-Path -LiteralPath 'LICENSE'))
        {
            $failures.Add('저장소 루트 MIT LICENSE가 없습니다.')
        }

        git grep -I -q '#develop' -- 'README.md' 'UnityTools/Packages/**'
        if ($LASTEXITCODE -eq 0)
        {
            $failures.Add('패키지 문서에 이동하는 develop 설치 주소가 남아 있습니다.')
        }

        $rootReadme = Get-Content -LiteralPath 'README.md' -Raw
        if ($rootReadme -notmatch 'unitytools-ui/v2\.0\.0' -or $rootReadme -notmatch 'unitytools-timer/v1\.0\.0')
        {
            $failures.Add('README에 package-scoped 고정 tag 설치 주소가 없습니다.')
        }
    }

    if ($failures.Count -gt 0)
    {
        $failures | ForEach-Object { Write-Error $_ }
        exit 1
    }

    Write-Host 'Release security checks passed.'
}
finally
{
    Pop-Location
}
