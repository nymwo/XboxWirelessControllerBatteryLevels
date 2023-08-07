using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;

namespace XboxWirelessControllerBatteryLevels;

static partial class BatteryLevelHelper
{
    [GeneratedRegex("BATTERY_LEVEL=(\\d+)=END")]
    private static partial Regex BatteryLevelRegex();

    private const string Script = """
        $values = Get-CimInstance -Query 'Select * From Win32_PnPEntity Where Name = "Xbox Wireless Controller"' |`
            Invoke-CimMethod -MethodName GetDeviceProperties -Arguments @{devicePropertyKeys = '{104EA319-6EE2-4701-BD47-8DDBF425BBE5} 2', '{83DA6326-97A6-4088-9453-A1923F573B29} 15'} |`
            Select-Object -ExpandProperty DeviceProperties |`
            Select-Object -ExpandProperty Data

        for ($i = 0; $i -lt $values.Count; $i += 2) {
            $batteryLevel = $values[$i]
            $connected = $values[$i + 1]
            if ($connected) {
                Write-Output "BATTERY_LEVEL=$batteryLevel=END"
            }
        }

    """;
    internal static List<int> GetBatteryLevels()
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

        process.Start();
        process.StandardInput.WriteLine(Script);
        process.StandardInput.Flush();
        process.StandardInput.Close();
        process.WaitForExit();

        var errors = process.StandardError.ReadToEnd();
        if (!string.IsNullOrWhiteSpace(errors))
        {
            throw new Exception(errors);
        }

        var result = process.StandardOutput.ReadToEnd();

        if (BatteryLevelRegex().Matches(result) is not MatchCollection matches)
        {
            return new List<int>();
        }
        
        var results = new List<int>();
        foreach (var m in matches)
        {
            if (m is not Match match)
            {
                continue;
            }
            var batteryLevel = int.Parse(match.Groups[1].Value);
            results.Add(batteryLevel);
        }

        results.Sort();

        return results;
    }
    internal static Icon GetIcon(List<int> batteryLevels)
    {
        int size = 16;

        var image = new Bitmap(size, size);
        using var graphics = Graphics.FromImage(image);
        graphics.FillRectangle(
            Brushes.Black,
            new RectangleF(0, 0, size, size)
        );
        int index = 0;
        foreach (int batteryLevel in batteryLevels)
        {
            var color = batteryLevel switch
            {
                < 15 => Color.Red,
                < 25 => Color.Orange,
                < 40 => Color.Yellow,
                _ => Color.Green
            };
            int w = size / batteryLevels.Count;
            int h = size * batteryLevel / 100;

            // One pixel gap between each battery level.
            int w2 = w - 1;

            // The last battery level never fills the entire icon, adjust the width so it always fits.
            if (
                index == batteryLevels.Count - 1
            )
            {
                w2 = size - index * w;
            }

            graphics.FillRectangle(
                new SolidBrush(color),
                new RectangleF(index * w, size - h, w2, h)
            );

            index++;
        }

        return Icon.FromHandle(image.GetHicon());
    }
}