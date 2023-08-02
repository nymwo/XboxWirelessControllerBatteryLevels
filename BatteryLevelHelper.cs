using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace XboxWirelessControllerBatteryLevels;

static class BatteryLevelHelper
{
    internal static IEnumerable<int> GetBatteryLevels()
    {
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
            Regex.Matches(result, @"BATTERY_LEVEL=(\d+)=END")
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
                < 10 => Color.Red,
                < 20 => Color.Orange,
                < 30 => Color.Yellow,
                _ => Color.Green
            };
            int w = size / batteryLevels.Count();
            int h = size * batteryLevel / 100;
            int w2 = w;

            // If the last battery level doesn't fill the entire icon, adjust the width.
            if (
                index == batteryLevels.Count() - 1 &&
                index * w + w2 < size
            )
            {
                w2 = size - index * w;
            }

            graphics.FillRectangle(
                new SolidBrush(color),
                new RectangleF(index * w, size - h, w2, h)
            );
        }
        graphics.DrawString(
            batteryLevels.Count().ToString(),
            new Font("Segoe UI", size * 2 / 5),
            Brushes.White,
            new PointF(size / 2, size / 2),
            new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            }
        );
        return Icon.FromHandle(image.GetHicon());
    }
}