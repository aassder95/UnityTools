param(
    [Parameter(Mandatory = $true)]
    [string]$Version,
    [switch]$Push
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$packageJsonPath = Join-Path $repoRoot "Packages/com.unitytools.core/package.json"

if(!(Test-Path $packageJsonPath))
{
    throw "패키지 파일을 찾을 수 없습니다: $packageJsonPath"
}

$package = Get-Content $packageJsonPath -Raw | ConvertFrom-Json
if($package.version -ne $Version)
{
    throw "버전 불일치: package.json=$($package.version), 입력=$Version"
}

$tag = "unitytools-core/v$Version"
$existingTag = git rev-parse --verify --quiet "refs/tags/$tag"
if($LASTEXITCODE -eq 0 -and ![string]::IsNullOrWhiteSpace($existingTag))
{
    throw "이미 존재하는 태그입니다: $tag"
}

git tag $tag
if($LASTEXITCODE -ne 0)
{
    throw "태그 생성 실패: $tag"
}

Write-Output "태그 생성 완료: $tag"
Write-Output "설치 URL 예시:"
Write-Output "https://<repo>.git?path=/Packages/com.unitytools.core#$tag"

if($Push)
{
    git push origin $tag
    if($LASTEXITCODE -ne 0)
    {
        throw "태그 푸시 실패: $tag"
    }

    Write-Output "원격 태그 푸시 완료: $tag"
}
else
{
    Write-Output "원격 푸시 생략. 필요 시 실행: git push origin $tag"
}
