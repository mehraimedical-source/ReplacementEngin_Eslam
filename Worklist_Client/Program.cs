using System;
using System.Windows.Forms;
using FellowOakDicom;
using Microsoft.Extensions.DependencyInjection;

namespace Worklist_Client
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
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
