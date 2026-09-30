using System;
using System.Windows.Forms;
using FellowOakDicom;
using Microsoft.Extensions.DependencyInjection;

namespace Worlist_SCP
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            new DicomSetupBuilder()
                .RegisterServices(services => services.AddFellowOakDicom())
                .Build();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
