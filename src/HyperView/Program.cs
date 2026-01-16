using HyperView.Class;

namespace HyperView
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Write startup banner to log
            FileLogger.WriteStartupBanner();

            // Register application exit event to write shutdown banner
            Application.ApplicationExit += OnApplicationExit;

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            
            try
            {
                Application.Run(new Forms.LoginForm());
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Unhandled application exception: {ex.Message}", 
                    FileLogger.EventType.Error, 9999);
                FileLogger.Message($"Stack trace: {ex.StackTrace}", 
                    FileLogger.EventType.Error, 9999);
            }
        }

        private static void OnApplicationExit(object sender, EventArgs e)
        {
            // Write shutdown banner to log
            FileLogger.WriteShutdownBanner();
        }
    }
}