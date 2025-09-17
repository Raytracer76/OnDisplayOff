using System;
using System.Diagnostics;
using System.IO;

namespace SleepOnDisplayOff
{
    /// <summary>
    /// Manages Windows Task Scheduler integration for automatic startup at user logon.
    /// Creates and manages a scheduled task that runs the application with elevated privileges.
    /// </summary>
    internal static class StartupTask
    {
        /// <summary>Name of the scheduled task in Windows Task Scheduler</summary>
        private const string TaskName = "SleepOnDisplayOff";
        /// <summary>
        /// Creates or updates the Windows scheduled task for automatic startup.
        /// The task is configured to run at user logon with highest privileges.
        /// </summary>
        public static void CreateOrUpdate()
        {
            // Get the full path to the current executable
            var exe = Process.GetCurrentProcess().MainModule!.FileName!;
            var quoted = "\"" + exe + "\"";
            
            // Build schtasks command arguments:
            // /Create - Create a new task
            // /TN - Task name
            // /TR - Task to run (executable path)
            // /SC ONLOGON - Trigger on user logon
            // /RL HIGHEST - Run with highest privileges (for shutdown/restart)
            // /F - Force create (overwrite existing)
            var args = $"/Create /TN \"{TaskName}\" /TR {quoted} /SC ONLOGON /RL HIGHEST /F";
            RunSchTasks(args);
        }
        /// <summary>
        /// Deletes the Windows scheduled task, disabling automatic startup.
        /// </summary>
        public static void Delete() => RunSchTasks($"/Delete /TN \"{TaskName}\" /F");

        /// <summary>
        /// Executes the schtasks.exe command with the specified arguments.
        /// Runs hidden without showing a console window.
        /// </summary>
        /// <param name="args">Command line arguments for schtasks.exe</param>
        private static void RunSchTasks(string args)
        {
            var psi = new ProcessStartInfo("schtasks.exe", args)
            {
                UseShellExecute = false,  // Don't use shell execute for better control
                CreateNoWindow = true     // Hide the command window
            };
            
            using var p = Process.Start(psi);
            
            // Wait up to 4 seconds for the command to complete
            // Most schtasks operations complete quickly
            p?.WaitForExit(4000);
        }
    }
}
