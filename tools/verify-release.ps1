param(
    [switch]$Release
)

$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()
$OutputEncoding = [Console]::OutputEncoding
$repoRoot = Split-Path -Parent $PSScriptRoot

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
        $originUri = [Uri]$originUrl
        if (-not [string]::IsNullOrEmpty($originUri.UserInfo))
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

    if ($Release)
    {
        git grep -I -q '#develop' -- 'UnityTools/Packages/**'
        if ($LASTEXITCODE -eq 0)
        {
            $failures.Add('패키지 문서에 이동하는 develop 설치 주소가 남아 있습니다.')
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
