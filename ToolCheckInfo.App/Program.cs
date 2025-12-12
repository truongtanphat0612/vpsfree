using System;
using System.Windows.Forms;
using ToolCheckInfo.App.UI; // Đã thêm using

namespace ToolCheckInfo;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }
}