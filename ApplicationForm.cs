using System;
using System.Linq;
using System.Windows.Forms;
using XboxWirelessControllerBatteryLevels;

public class ApplicationForm : System.Windows.Forms.Form
{
    private System.Windows.Forms.NotifyIcon notifyIcon;
    private System.ComponentModel.IContainer components;

    public ApplicationForm()
    {
        components = new System.ComponentModel.Container();

        notifyIcon = new System.Windows.Forms.NotifyIcon(components);
        UpdateStatus();
        notifyIcon.Visible = true;

        var contextMenu = new System.Windows.Forms.ContextMenuStrip(components);
        contextMenu.Items.AddRange(new[] {
            new System.Windows.Forms.ToolStripMenuItem(
                "Exit",
                System.Drawing.SystemIcons.Exclamation.ToBitmap(),
                (sender, e) => Application.Exit()
            )
        });
        notifyIcon.ContextMenuStrip = contextMenu;

        var timer = new System.Windows.Forms.Timer(components);
        timer.Interval = 5000;
        timer.Tick += (sender, e) => UpdateStatus();
        timer.Start();
    }

    private void UpdateStatus()
    {
        // notifyIcon.Icon = System.Drawing.SystemIcons.Exclamation;
        var batteryLevels = BatteryLevelHelper.GetBatteryLevels();
        var icon = BatteryLevelHelper.GetIcon(batteryLevels);
        notifyIcon.Icon = icon;
        notifyIcon.Text = batteryLevels.Any()
            ? batteryLevels.Select(x => $"{x}%").Aggregate((x, y) => $"{x}, {y}")
            : "No controllers connected";
    }

    protected override void OnLoad(EventArgs e)
    {
        Visible = false;
        ShowInTaskbar = false;

        base.OnLoad(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }
}