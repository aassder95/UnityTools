param(
    [Parameter(Mandatory = $true)]
    [string]$Project
)

$ErrorActionPreference = 'Stop'
$utf8 = [Text.UTF8Encoding]::new($false)
$Project = (Resolve-Path -LiteralPath $Project).ProviderPath
$summary = Get-Content -LiteralPath "$Project/summary.json" -Raw | ConvertFrom-Json
if ($summary.Source -ne 'Git' -or $summary.Player -ne 'Passed') { throw '먼저 Sheets Git 설치와 Player 검증을 완료하세요.' }
if (Test-Path -LiteralPath "$Project/Assets/GuiValidation") { throw 'GUI fixture가 이미 있습니다. 새 Sheets 검증 프로젝트를 사용하세요.' }
& (Join-Path $PSScriptRoot 'confirm-upm-source.ps1') -Project $Project -PackageName 'com.aassder95.unitytools.sheets' -SourceRef $summary.SourceRef
$manifest = Get-Content -LiteralPath "$Project/Packages/manifest.json" -Raw | ConvertFrom-Json
$manifest.dependencies | Add-Member -Name 'com.aassder95.unitytools.vfx' -MemberType NoteProperty -Value "https://github.com/aassder95/UnityTools.git?path=/UnityTools/Packages/com.aassder95.unitytools.vfx#$($summary.SourceRef)" -Force
[IO.File]::WriteAllText("$Project/Packages/manifest.json", ($manifest | ConvertTo-Json -Depth 10), $utf8)
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'PackageGuiFixture.cs') -Destination "$Project/Assets/Editor/PackageGuiFixture.cs"
$unityPath = "C:/Program Files/Unity/Hub/Editor/$($summary.Unity)/Editor/Unity.exe"
$arguments = @('-batchmode', '-projectPath', ('"' + $Project + '"'), '-executeMethod', 'PackageGuiFixture.Prepare', '-quit', '-logFile', ('"' + $Project + '/gui-prepare.log"'))
$editor = Start-Process -FilePath $unityPath -ArgumentList $arguments -PassThru -WindowStyle Hidden
$editor.WaitForExit()
$editor.Refresh()
if ($editor.ExitCode -ne 0) { throw "GUI fixture 준비 실패: $Project/gui-prepare.log" }
$report = (Get-Content -LiteralPath "$Project/gui-fixture-result.txt" -Raw).Trim()
if ($report -ne ($summary.Unity + ' | CSV and 15 prefabs | Prepared')) { throw 'GUI fixture 버전 또는 결과 불일치' }
if (@(Get-ChildItem -LiteralPath "$Project/Assets/GuiValidation" -Filter '*.prefab').Count -ne 15) { throw 'GUI prefab 개수 불일치' }
& (Join-Path $PSScriptRoot 'confirm-upm-source.ps1') -Project $Project -PackageName 'com.aassder95.unitytools.vfx' -SourceRef $summary.SourceRef
Write-Output $report
Write-Output 'Editor에서 Tools > UnityTools > GUI Validation > Open Windows를 실행하세요. GUI 조작 검증은 아직 수행하지 않았습니다.'
