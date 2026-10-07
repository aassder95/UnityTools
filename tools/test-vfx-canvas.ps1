param(
    [Parameter(Mandatory = $true)]
    [string]$Project,
    [ValidateSet('BuiltIn', 'Urp')]
    [string]$Pipeline = 'BuiltIn'
)

$ErrorActionPreference = 'Stop'
$utf8 = [Text.UTF8Encoding]::new($false)
$Project = (Resolve-Path -LiteralPath $Project).ProviderPath
$summary = Get-Content -LiteralPath "$Project/summary.json" -Raw | ConvertFrom-Json
if ($summary.Source -ne 'Git' -or $summary.Passed -ne $summary.Total -or $summary.Total -le 0 -or $summary.Skipped -ne 0) { throw '먼저 VFX Git 설치 검증을 완료하세요.' }
& (Join-Path $PSScriptRoot 'confirm-upm-source.ps1') -Project $Project -PackageName 'com.aassder95.unitytools.vfx' -SourceRef $summary.SourceRef
if (!(Test-Path -LiteralPath "$Project/Assets/ProjectVfx/3.Resources/Materials/UIAdditive.mat")) { throw 'UIAdditive.mat 검증 사본이 없습니다.' }
$runPath = Join-Path $Project "Canvas-$Pipeline-$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $runPath, "$Project/Assets/CanvasValidation" -Force | Out-Null
$manifest = Get-Content -LiteralPath "$Project/Packages/manifest.json" -Raw | ConvertFrom-Json
$uiVersion = if ($summary.Unity.StartsWith('2022.3.')) { '1.0.0' } else { '2.0.0' }
$manifest.dependencies | Add-Member -MemberType NoteProperty -Name 'com.unity.ugui' -Value $uiVersion -Force
$manifest.dependencies | Add-Member -MemberType NoteProperty -Name 'com.unity.modules.ui' -Value '1.0.0' -Force
[IO.File]::WriteAllText("$Project/Packages/manifest.json", ($manifest | ConvertTo-Json -Depth 10), $utf8)
[IO.File]::WriteAllText("$Project/CanvasOutput.txt", $runPath, $utf8)
[IO.File]::WriteAllText("$Project/CanvasPipeline.txt", $Pipeline, $utf8)
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'VfxCanvasTests.cs') -Destination "$Project/Assets/CanvasValidation/VfxCanvasTests.cs"
$assembly = @{ name = 'UnityTools.Vfx.CanvasValidation'; references = @('UnityTools.Vfx.Editor', 'Unity.ugui'); includePlatforms = @('Editor'); optionalUnityReferences = @('TestAssemblies') }
[IO.File]::WriteAllText("$Project/Assets/CanvasValidation/CanvasValidation.asmdef", ($assembly | ConvertTo-Json), $utf8)
Write-Host "Canvas 검증 결과 경로: $runPath"
$unityPath = "C:/Program Files/Unity/Hub/Editor/$($summary.Unity)/Editor/Unity.exe"
$arguments = @('-batchmode', '-projectPath', ('"' + $Project + '"'), '-runTests', '-testPlatform', 'EditMode', '-testFilter', 'VfxCanvasTests', '-testResults', ('"' + $runPath + '/results.xml"'), '-logFile', ('"' + $runPath + '/tests.log"'))
$editor = Start-Process -FilePath $unityPath -ArgumentList $arguments -PassThru -WindowStyle Hidden
$editor.WaitForExit()
$editor.Refresh()
if ($editor.ExitCode -ne 0) { throw "Canvas 테스트 실패: $runPath/tests.log" }
[xml]$tests = Get-Content -LiteralPath "$runPath/results.xml" -Raw
$run = $tests.'test-run'
if ($run.result -ne 'Passed' -or [int]$run.passed -ne 1 -or [int]$run.skipped -ne 0) { throw "Canvas 결과 실패: $runPath/results.xml" }
$report = (Get-Content -LiteralPath "$runPath/canvas-result.txt" -Raw).Trim()
$expectedPipeline = if ($Pipeline -eq 'BuiltIn') { 'BuiltIn' } else { 'UniversalRenderPipelineAsset' }
if (!$report.StartsWith($summary.Unity + ' | ' + $expectedPipeline + ' |')) { throw 'Canvas 실행 버전 또는 pipeline 불일치' }
$lock = Get-Content -LiteralPath "$Project/Packages/packages-lock.json" -Raw | ConvertFrom-Json
if ($lock.dependencies.'com.unity.ugui'.version -ne $uiVersion) { throw 'uGUI lock 버전 불일치' }
$source = Get-Content -LiteralPath "$Project/project-vfx-source.json" -Raw | ConvertFrom-Json
foreach ($entry in $source.sha256.PSObject.Properties) {
    if ((Get-FileHash -LiteralPath (Join-Path $source.source_assets $entry.Name) -Algorithm SHA256).Hash -ne $entry.Value -or (Get-FileHash -LiteralPath (Join-Path "$Project/Assets/ProjectVfx" $entry.Name) -Algorithm SHA256).Hash -ne $entry.Value) { throw "VFX 원본 또는 사본 변경: $($entry.Name)" }
}
$result = [ordered]@{ Unity = $summary.Unity; Pipeline = $Pipeline; Ui = $uiVersion; SourceRef = $summary.SourceRef; Tests = '1/1'; Result = $report; SourceHashesPreserved = $true; Output = $runPath }
[IO.File]::WriteAllText("$runPath/summary.json", ($result | ConvertTo-Json), $utf8)
$result | ConvertTo-Json
