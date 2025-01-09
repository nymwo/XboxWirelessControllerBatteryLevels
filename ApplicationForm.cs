using System;
using System.Windows.Forms;

namespace XboxWirelessControllerBatteryLevels
{
    public class ApplicationForm : Form
    {
        private readonly System.ComponentModel.IContainer components;
        private readonly NotifyIcon notifyIcon;

        public ApplicationForm()
        {
            components = new System.ComponentModel.Container();
            notifyIcon = new NotifyIcon(components);
            UpdateStatus();

            notifyIcon.Visible = true;

            var contextMenu = new ContextMenuStrip(components);
            contextMenu.Items.AddRange(new[] {
                new ToolStripMenuItem(
                    ""
                ),
                new ToolStripMenuItem(
                    "Exit",
                    null,
                    (sender, e) => Application.Exit()
                )
            });
            notifyIcon.ContextMenuStrip = contextMenu;

            var timer = new Timer(components)
            {
                Interval = 5000
            };
            timer.Tick += (sender, e) => UpdateStatus();
            timer.Start();
        }

        private void UpdateStatus()
        {
            var batteryLevels = BatteryLevelHelper.GetBatteryLevels();
            var icon = BatteryLevelHelper.GetIcon(batteryLevels);
            string text = batteryLevels.Count > 0
                ? string.Join("%, ", batteryLevels) + "%"
                : "No controllers connected";
            notifyIcon.Icon = icon;
            notifyIcon.Text = text;
            if (notifyIcon.ContextMenuStrip?.Items.Count > 0)
            {
                notifyIcon.ContextMenuStrip.Items[0].Image = icon.ToBitmap();
                notifyIcon.ContextMenuStrip.Items[0].Text = text;
            }
        }

        protected override void OnLoad(System.EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;

            base.OnLoad(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (
                m.Msg == 0x0011 || // WM_QUERYENDSESSION
                m.Msg == 0x0016 // WM_ENDSESSION
            )
            {
                Application.Exit();
                return;
            }

            base.WndProc(ref m);
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
}