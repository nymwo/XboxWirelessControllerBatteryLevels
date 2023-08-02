using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace XboxWirelessControllerBatteryLevels;

static partial class BatteryLevelHelper
{
    internal static IEnumerable<int> GetBatteryLevels()
    {
        #if DEBUG
        return new[] { 70, 35, 20, 10 };
        #endif

        var script = """
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

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
            {
                FileName = "pwsh.exe",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

        process.Start();
        process.StandardInput.WriteLine(script);
        process.StandardInput.Flush();
        process.StandardInput.Close();
        process.WaitForExit();

        var errors = process.StandardError.ReadToEnd();
        if (!string.IsNullOrWhiteSpace(errors))
        {
            throw new Exception(errors);
        }

        var result = process.StandardOutput.ReadToEnd();
        var results =
            BatteryLevelRegex().Matches(result)
            .Select(x => x.Groups[1].Value)
            .Select(x => int.Parse(x))
            .OrderByDescending(x => x);

        return results;
    }
    internal static Icon GetIcon(IEnumerable<int> batteryLevels)
    {
        int size = 16;

        var image = new Bitmap(size, size);
        using var graphics = Graphics.FromImage(image);
        graphics.FillRectangle(
            Brushes.Black,
            new RectangleF(0, 0, size, size)
        );
        foreach (var (batteryLevel, index) in batteryLevels.Select((x, i) => (x, i)))
        {
            var color = batteryLevel switch
            {
                < 15 => Color.Red,
                < 25 => Color.Orange,
                < 40 => Color.Yellow,
                _ => Color.Green
            };
            int w = size / batteryLevels.Count();
            int h = size * batteryLevel / 100;

            // One pixel gap between each battery level.
            int w2 = w - 1;

            // The last battery level never fills the entire icon, adjust the width so it always fits.
            if (
                index == batteryLevels.Count() - 1
            )
            {
                w2 = size - index * w;
            }

            graphics.FillRectangle(
                new SolidBrush(color),
                new RectangleF(index * w, size - h, w2, h)
            );
        }

        return Icon.FromHandle(image.GetHicon());
    }

    [GeneratedRegex("BATTERY_LEVEL=(\\d+)=END")]
    private static partial Regex BatteryLevelRegex();
}