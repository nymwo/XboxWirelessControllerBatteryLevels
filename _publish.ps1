$process = Get-Process -Name XboxWirelessControllerBatteryLevels -ErrorAction SilentlyContinue
if ($process) {
    Stop-Process -Id $process.Id -Force -ErrorAction Stop

    $process.WaitForExit()
}

if (Test-Path ./publish) {
    Remove-Item ./publish -Recurse -Force -ErrorAction Stop
}

dotnet publish -c Release

Copy-Item ./bin/Release/net*/win*/publish ./publish -Recurse -Force -ErrorAction Stop

# Remove unused files from Release folder for clarity

$used_files = Get-Content ./_used_files.txt -ErrorAction Stop
$all_files = Get-ChildItem ./bin/Release/net*/win*/* -File -ErrorAction Stop | Select-Object -ExpandProperty Name

foreach ($file in $all_files) {
    if ($used_files -notcontains $file) {
        Remove-Item ./bin/Release/net*/win*/$file -Force -ErrorAction Stop
    }
}