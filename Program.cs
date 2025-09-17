using System;
using System.Threading;
using System.Windows.Forms;

namespace OnDisplayOff
{
    /// <summary>
    /// Entry point for the OnDisplayOff application.
    /// This class handles application initialization and ensures only one instance runs at a time.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Main entry point for the application.
        /// Sets up single instance protection and initializes the Windows Forms application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Create a named mutex to prevent multiple instances of the application
            // The mutex is automatically released when the application exits
            using var mutex = new Mutex(true, "OnDisplayOff_SingleInstance", out bool isNew);
            
            // If another instance is already running, exit immediately
            if (!isNew) return;

            // Enable Windows visual styles for modern appearance
            Application.EnableVisualStyles();
            
            // Use GDI+ text rendering for better compatibility
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Start the main application as a hidden tray application
            Application.Run(new TrayApp());
        }
    }
}
