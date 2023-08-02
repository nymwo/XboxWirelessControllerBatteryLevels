using System;
using System.Windows.Forms;

namespace XboxWirelessControllerBatteryLevels;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new ApplicationForm());
    }
}