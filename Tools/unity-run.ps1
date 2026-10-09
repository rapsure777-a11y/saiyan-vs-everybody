# Runs Unity in batch mode for this project and waits. Usage:
#   powershell -NoProfile -ExecutionPolicy Bypass -File Tools\unity-run.ps1 -Method Saiyan.Editor.ProjectSetup.Run
#   powershell -NoProfile -ExecutionPolicy Bypass -File Tools\unity-run.ps1 -Tests EditMode          (or PlayMode; -Filter "TestName;OtherTest")
param([string]$Method = "", [string]$Tests = "", [string]$Version = "6000.3.9f1", [string]$Project = "", [string]$Filter = "", [int]$TimeoutMin = 25)
$root = if ($Project) { $Project } else { Split-Path $PSScriptRoot -Parent }
$unity = "C:\Program Files\Unity\Hub\Editor\$Version\Editor\Unity.exe"
New-Item -ItemType Directory -Force (Join-Path $root "Logs") | Out-Null
$tag = if ($Method) { "method" } else { $Tests.ToLower() }
$log = Join-Path $root "Logs\unity-$tag-$Version.log"
$args = @('-batchmode', '-projectPath', "`"$root`"", '-logFile', "`"$log`"")
if ($Method) { $args += @('-quit', '-executeMethod', $Method) }
if ($Tests) {
    $results = Join-Path $root ("Logs\results-" + $Tests.ToLower() + "-" + $Version + ".xml")
    if (Test-Path $results) { Remove-Item $results }
    $args += @('-runTests', '-testPlatform', $Tests, '-testResults', "`"$results`"")
    if ($Filter) { $args += @('-testFilter', "`"$Filter`"") }
}
$p = Start-Process $unity -ArgumentList $args -PassThru
if (-not $p.WaitForExit($TimeoutMin * 60000)) { $p.Kill(); "TIMEOUT (killed Unity)"; exit 1 }
Select-String -Path $log -Pattern "error CS|Scripts have compiler errors|\[Saiyan\]" | Select-Object -Last 25 | ForEach-Object { $_.Line }
if ($Tests -and (Test-Path $results)) {
    [xml]$x = Get-Content $results
    $r = $x.'test-run'
    "TESTS ($Version $Tests): total=$($r.total) passed=$($r.passed) failed=$($r.failed) skipped=$($r.skipped)"
    $x.SelectNodes("//test-case") | ForEach-Object { "  " + $_.result + "  " + $_.name + "  (" + $_.duration + "s)" }
    $x.SelectNodes("//test-case[@result='Failed']") | ForEach-Object { "FAILED: " + $_.name; ($_.failure.message.InnerText -split "`n")[0..3] -join " | " }
}
"exit $($p.ExitCode)"
