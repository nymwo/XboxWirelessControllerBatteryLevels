$process = Get-Process -Name XboxWirelessControllerBatteryLevels -ErrorAction SilentlyContinue
if ($process) {
    Stop-Process -Id $process.Id -Force -ErrorAction Stop

    $process.WaitForExit()

    Start-Sleep 2
}

if (Test-Path ./publish) {
    Remove-Item ./publish/* -Recurse -Force -ErrorAction Stop
}

dotnet publish -c Release

Copy-Item ./bin/Release/net*/win*/publish ./ -Recurse -Force -ErrorAction Stop
