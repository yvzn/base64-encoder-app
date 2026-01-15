using System.Configuration;
using System.Data;
using System.Windows;
using Base64Utils.Services;

namespace Base64Utils
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Register cleanup on application exit
            this.Exit += OnApplicationExit;
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            // Cleanup all temporary files when the application exits
            FileService.CleanupAllTemporaryFiles();
        }
    }

}
