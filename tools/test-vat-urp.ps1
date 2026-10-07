param([Parameter(Mandatory = $true)][string]$Project)
$ErrorActionPreference = 'Stop'
$Project = (Resolve-Path -LiteralPath $Project).ProviderPath
$summary = Get-Content -LiteralPath "$Project/summary.json" -Raw | ConvertFrom-Json
if ($summary.PlayerProbe -ne 'Passed' -or $summary.EditMode.Failed -ne 0) { throw '먼저 이식 기능의 Built-in 검증을 완료하세요.' }
$version = $summary.Unity
$urpVersion = if ($version.StartsWith('2022.3')) { '14.0.12' } elseif ($version.StartsWith('6000.3')) { '17.3.0' } else { throw '지원 Unity 버전이 아닙니다.' }
$manifest = Get-Content -LiteralPath "$Project/Packages/manifest.json" -Raw | ConvertFrom-Json
$manifest.dependencies | Add-Member -MemberType NoteProperty -Name 'com.unity.render-pipelines.universal' -Value $urpVersion -Force
[IO.File]::WriteAllText("$Project/Packages/manifest.json", ($manifest | ConvertTo-Json -Depth 10), [Text.UTF8Encoding]::new($false))
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'VatUrpValidation.cs') -Destination "$Project/Assets/Editor/VatUrpValidation.cs"
$unityPath = "C:/Program Files/Unity/Hub/Editor/$version/Editor/Unity.exe"
foreach ($step in @('Catalog','Configure','Tests'))
{
    $extra = if ($step -eq 'Catalog') { @('-runTests','-testPlatform','EditMode','-testFilter','UnityTools.Ui.Editor.Tests.UiAssetCatalogSerializationTests','-testResults',('"' + $Project + '/catalog-serialization.xml"')) } elseif ($step -eq 'Configure') { @('-executeMethod','VatUrpValidation.Configure','-quit') } else { @('-runTests','-testPlatform','EditMode','-testFilter','UnityTools.Vat.Editor.Tests','-testResults',('"' + $Project + '/vat-urp.xml"')) }
    $arguments = @('-batchmode','-projectPath',('"' + $Project + '"'),'-logFile',('"' + $Project + '/vat-urp-' + $step + '.log"')) + $extra
    $process = Start-Process -FilePath $unityPath -ArgumentList $arguments -PassThru -WindowStyle Hidden
    $process.WaitForExit()
    $process.Refresh()
    if ($process.ExitCode -ne 0) { throw "VAT URP $step 실패: $Project/vat-urp-$step.log" }
}
[xml]$catalogResult = Get-Content -LiteralPath "$Project/catalog-serialization.xml" -Raw
if ($catalogResult.'test-run'.result -ne 'Passed' -or [int]$catalogResult.'test-run'.passed -le 0) { throw '카탈로그 직렬화 테스트 실패' }
[xml]$result = Get-Content -LiteralPath "$Project/vat-urp.xml" -Raw
$run = $result.'test-run'
if ($run.result -ne 'Passed' -or [int]$run.passed -le 0 -or [int]$run.skipped -ne 0) { throw 'VAT URP 테스트 실패 또는 skip' }
$lock = Get-Content -LiteralPath "$Project/Packages/packages-lock.json" -Raw | ConvertFrom-Json
if ($lock.dependencies.'com.unity.render-pipelines.universal'.version -ne $urpVersion) { throw 'URP 버전 불일치' }
$report = @{ Unity = $version; Urp = $urpVersion; Passed = [int]$run.passed; Failed = [int]$run.failed; Skipped = [int]$run.skipped; Source = 'Local'; CatalogSerializationPassed = [int]$catalogResult.'test-run'.passed }
$report | ConvertTo-Json | Set-Content -LiteralPath "$Project/vat-urp-summary.json" -Encoding utf8
Get-Content -LiteralPath "$Project/vat-urp-summary.json"
