if (-not (Test-Path "$PSScriptRoot\__used_files.txt")) {
    Write-Output "Copying to $PSScriptRoot\__used_files.txt..."
    Copy-Item "$PSScriptRoot\_used_files.txt" "$PSScriptRoot\__used_files.txt"
}

$used_files = Get-Content "$PSScriptRoot\__used_files.txt" | Where-Object { $_ -ne "" }
$new_used_files = $used_files.Clone()

Write-Output "Probing $($used_files.Length) files..."

for ($i = 0; $i -lt $new_used_files.Length; $i++) {
    Write-Output "Probing $i of $($new_used_files.Length)..."

    $new_used_files[$i] = ""
    Set-Content "$PSScriptRoot\_used_files.txt" $new_used_files

    try {
        # delete Release folder
        if (Test-Path ./bin/Release) {
            Remove-Item ./bin/Release -Recurse -Force -ErrorAction Stop
        }

        # run _publish.ps1
        dotnet publish -c Release

        if (-not (Test-Path ./bin/Release/net*/win*/publish/XboxWirelessControllerBatteryLevels.exe)) {
            Write-Output "Publish failed. Restoring $i..."
            $new_used_files[$i] = $used_files[$i]
            continue
        }

        # start publish/XboxWirelessControllerBatteryLevels.exe and wait for it to exit or for 3 seconds
        $process = Start-Process ./bin/Release/net*/win*/publish/XboxWirelessControllerBatteryLevels.exe -PassThru
        $process.WaitForExit(3000)

        # if the process is still running, kill it
        if ($process.HasExited) {
            Write-Output "Process exited with code $($process.ExitCode), restoring $i..."
            $new_used_files[$i] = $used_files[$i]
        } else {
            Write-Output "Process timed out. Killing process..."
            $process = Get-Process -Name XboxWirelessControllerBatteryLevels -ErrorAction SilentlyContinue
            if ($process) {
                Stop-Process -Id $process.Id -Force -ErrorAction Stop

                Start-Sleep 2
            
                $process.WaitForExit()
            }
        }
    } catch {
        Write-Output "Caught exception: $_"
        Write-Output "Restoring $i..."
        $new_used_files[$i] = $used_files[$i]
    }

    Start-Sleep 1
}

Write-Output "Done probing."
Set-Content "$PSScriptRoot\_used_files.txt" $new_used_files
