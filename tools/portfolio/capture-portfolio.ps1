param(
    [Parameter(Mandatory=$true)][string]$ProjectPath,
    [Parameter(Mandatory=$true)][string]$OutputPath,
    [string]$UnityVersion = '6000.3.20f1',
    [switch]$SkipBuild,
    [switch]$CaptureOnly
)

$ErrorActionPreference = 'Stop'
$project = [IO.Path]::GetFullPath($ProjectPath)
$output = [IO.Path]::GetFullPath($OutputPath)
if (Test-Path -LiteralPath $output) { throw '측정 출력은 새 폴더를 지정하세요.' }
if (!(Test-Path -LiteralPath "$project/Packages/manifest.json")) { throw 'Git 설치 검증을 마친 임시 ui-lab 프로젝트가 필요합니다.' }
if ((Split-Path -Leaf $project) -ne 'ui-lab' -or (Split-Path -Leaf (Split-Path -Parent $project)) -notlike 'UnityTools-Compatibility-*') { throw '호환성 스크립트가 만든 ui-lab 프로젝트만 사용하세요.' }
$lock = Get-Content -LiteralPath "$project/Packages/packages-lock.json" -Raw | ConvertFrom-Json
if ($lock.dependencies.'com.aassder95.unitytools.benchmark'.source -ne 'git') { throw '공개 Git 패키지 설치 프로젝트가 필요합니다.' }
if (!$SkipBuild)
{
    Copy-Item -LiteralPath "$PSScriptRoot/PortfolioCapture.cs" -Destination "$project/Assets/PortfolioCapture.cs"
    Copy-Item -LiteralPath "$PSScriptRoot/PortfolioBuild.cs" -Destination "$project/Assets/Editor/PortfolioBuild.cs"
    $manifest = Get-Content -LiteralPath "$project/Packages/manifest.json" -Raw | ConvertFrom-Json
    $manifest.dependencies | Add-Member -NotePropertyName 'com.unity.modules.screencapture' -NotePropertyValue '1.0.0' -Force
    $manifest.dependencies | Add-Member -NotePropertyName 'com.unity.modules.imageconversion' -NotePropertyValue '1.0.0' -Force
    [IO.File]::WriteAllText("$project/Packages/manifest.json", ($manifest | ConvertTo-Json -Depth 10), [Text.UTF8Encoding]::new($false))
    $unity = "C:/Program Files/Unity/Hub/Editor/$UnityVersion/Editor/Unity.exe"
    $process = Start-Process -FilePath $unity -ArgumentList @('-batchmode','-nographics','-quit','-projectPath',('"' + $project + '"'),'-executeMethod','PortfolioBuild.Build','-logFile',('"' + "$project/portfolio-build.log" + '"')) -PassThru -WindowStyle Hidden
    $process.WaitForExit()
    $process.Refresh()
    if ($process.ExitCode -ne 0 -or (Get-Content "$project/portfolio-build-result.txt" -Raw).Trim() -ne 'Succeeded') { throw '시연 빌드 실패: portfolio-build.log 확인' }
}

New-Item -ItemType Directory -Path $output | Out-Null
$prevOutput = $env:UNITYTOOLS_PORTFOLIO_OUTPUT
$prevCaptureOnly = $env:UNITYTOOLS_PORTFOLIO_CAPTURE_ONLY
try
{
    $env:UNITYTOOLS_PORTFOLIO_OUTPUT = $output
    $env:UNITYTOOLS_PORTFOLIO_CAPTURE_ONLY = if ($CaptureOnly) { '1' } else { '0' }
    $player = Start-Process -FilePath "$project/PortfolioBuild/UnityTools-Portfolio.exe" -ArgumentList @('-screen-width','1280','-screen-height','720','-screen-fullscreen','0','-logFile',('"' + "$output/player.log" + '"')) -PassThru -WindowStyle Hidden
    $player.WaitForExit()
    $player.Refresh()
    if ($player.ExitCode -ne 0 -or !(Test-Path -LiteralPath "$output/capture-complete.txt")) { throw '시연 실행 실패: player.log 확인' }
}
finally
{
    $env:UNITYTOOLS_PORTFOLIO_OUTPUT = $prevOutput
    $env:UNITYTOOLS_PORTFOLIO_CAPTURE_ONLY = $prevCaptureOnly
}

$caption = "drawbox=y=ih-38:h=38:color=black@0.9:t=fill,drawtext=fontfile='C\:/Windows/Fonts/arial.ttf':text='DEMO CAPTURE - statistics include capture cost; use the separate 4-pair CSV':fontcolor=white:fontsize=17:x=12:y=h-27"
& ffmpeg -hide_banner -loglevel error -framerate 2 -i "$output/frames/%04d.png" -vf $caption -c:v libx264 -pix_fmt yuv420p -movflags +faststart "$output/ui-performance-demo.mp4"
if ($LASTEXITCODE -ne 0) { throw '시연 영상 인코딩 실패' }
Write-Host "측정 및 시연 자료: $output"
