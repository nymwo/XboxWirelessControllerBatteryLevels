using System.Windows.Forms;

namespace XboxWirelessControllerBatteryLevels
{
    public class ApplicationForm : Form
    {
        private readonly NotifyIcon notifyIcon;
        private readonly System.ComponentModel.IContainer components;

        public ApplicationForm()
        {
            components = new System.ComponentModel.Container();

            notifyIcon = new NotifyIcon(components);
            UpdateStatus();
            notifyIcon.Visible = true;

            var contextMenu = new ContextMenuStrip(components);
            contextMenu.Items.AddRange(new[] {
            new ToolStripMenuItem(
                "Exit",
                System.Drawing.SystemIcons.Exclamation.ToBitmap(),
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
            notifyIcon.Icon = icon;
            notifyIcon.Text = batteryLevels.Count > 0
                ? string.Join("%, ", batteryLevels) + "%"
                : "No controllers connected";
            System.GC.Collect();
        }

        protected override void OnLoad(System.EventArgs e)
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
}