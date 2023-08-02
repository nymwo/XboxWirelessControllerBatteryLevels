$process = Get-Process -Name XboxWirelessControllerBatteryLevels -ErrorAction SilentlyContinue
if ($process) {
    Stop-Process -Id $process.Id -Force -ErrorAction Stop

    $process.WaitForExit()
}

if (Test-Path ./publish) {
    Remove-Item ./publish -Recurse -Force -ErrorAction Stop
}

dotnet publish -c Release -o ./publish
