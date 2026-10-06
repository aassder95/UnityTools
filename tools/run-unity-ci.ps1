param(
    [ValidateSet('2022.3.62f3', '6000.3.20f1')]
    [string]$UnityVersion = '2022.3.62f3',
    [ValidateSet('core', 'samples', 'timer')]
    [string]$Suite = 'core',
    [string]$OutputBase = (Join-Path $env:TEMP "unity-ci-$([Guid]::NewGuid().ToString('N').Substring(0, 8))")
)

$ErrorActionPreference = 'Stop'
$utf8 = [Text.UTF8Encoding]::new($false)
New-Item -ItemType Directory -Path $OutputBase -Force | Out-Null
if (@(Get-ChildItem -LiteralPath $OutputBase -Force).Count -gt 0) { throw 'CI 출력 폴더는 비어 있어야 합니다. 새 경로를 지정하세요.' }
$scenarios = switch ($Suite)
{
    'core' { @('ui', 'timer', 'benchmark', 'persistence') }
    'samples' { @('ui-input', 'ui-lab', 'save-lab', 'timer-lab') }
    'timer' { @('timer') }
}
$isPassed = $false
try
{
    & (Join-Path $PSScriptRoot 'test-upm-compatibility.ps1') -Source Local -UnityVersion $UnityVersion -Scenarios $scenarios -OutputBase $OutputBase
    $isPassed = $true
}
finally
{
    $status = if ($isPassed) { 'Passed' } else { 'Failed' }
    $lines = @("## Unity $UnityVersion / $Suite", '', "Result: $status", '', '| Scenario | Play Mode | Windows Mono build |', '| --- | --- | --- |')
    $completed = @()
    foreach ($file in Get-ChildItem -LiteralPath $OutputBase -Recurse -Filter summary.json -File)
    {
        $results = Get-Content -LiteralPath $file.FullName -Raw | ConvertFrom-Json
        foreach ($result in $results)
        {
            $completed += $result.Scenario
            $lines += "| $($result.Scenario) | $($result.Passed)/$($result.Total) | $($result.Build) |"
        }
    }
    foreach ($scenario in $scenarios)
    {
        if ($completed -notcontains $scenario) { $lines += "| $scenario | Incomplete | Incomplete |" }
    }
    $lines += @('', 'A partial suite is not a successful validation. See the Editor logs for the failed step.', 'Device input, mobile platforms, IL2CPP and performance measurements are outside this workflow.')
    [IO.File]::WriteAllText((Join-Path $OutputBase 'ci-summary.md'), ($lines -join "`n") + "`n", $utf8)
}
