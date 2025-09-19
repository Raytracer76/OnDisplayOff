using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace OnDisplayOff
{
    /// <summary>
    /// Manages Windows startup integration using both Task Scheduler and Registry methods.
    /// Prefers scheduled task for elevated privileges, falls back to registry for user-level startup.
    /// </summary>
    internal static class StartupTask
    {
        /// <summary>Name of the scheduled task in Windows Task Scheduler</summary>
        private const string TaskName = "OnDisplayOff";

        /// <summary>Registry path for Windows startup programs</summary>
        private const string RegistryStartupPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        /// <summary>Registry value name for this application</summary>
        private const string RegistryValueName = "OnDisplayOff";
        /// <summary>
        /// Creates startup entry for automatic startup at user logon.
        /// Tries scheduled task first (for elevated privileges), falls back to registry.
        /// </summary>
        public static void CreateOrUpdate()
        {
            var exe = Process.GetCurrentProcess().MainModule!.FileName!;

            // First try to create a scheduled task for elevated privileges
            try
            {
                var quoted = "\"" + exe + "\"";
                var args = $"/Create /TN \"{TaskName}\" /TR {quoted} /SC ONLOGON /RL HIGHEST /F";

                var psi = new ProcessStartInfo("schtasks.exe", args)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var p = Process.Start(psi);
                if (p != null)
                {
                    p.WaitForExit(4000);

                    if (p.ExitCode == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("[STARTUP] Scheduled task created successfully");
                        return; // Success - we're done
                    }
                    else
                    {
                        var error = p.StandardError.ReadToEnd();
                        System.Diagnostics.Debug.WriteLine($"[STARTUP] Scheduled task failed (code {p.ExitCode}): {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[STARTUP] Scheduled task exception: {ex.Message}");
            }

            // Fallback to registry method (user-level startup)
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryStartupPath, true);
                if (key != null)
                {
                    key.SetValue(RegistryValueName, $"\"{exe}\"", RegistryValueKind.String);
                    System.Diagnostics.Debug.WriteLine("[STARTUP] Registry startup entry created");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[STARTUP] Registry fallback failed: {ex.Message}");
                throw; // Re-throw to let caller handle
            }
        }
        /// <summary>
        /// Removes startup entries to disable automatic startup.
        /// Removes both scheduled task and registry entry.
        /// </summary>
        public static void Delete()
        {
            // Try to delete scheduled task
            try
            {
                var args = $"/Delete /TN \"{TaskName}\" /F";
                var psi = new ProcessStartInfo("schtasks.exe", args)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var p = Process.Start(psi);
                if (p != null)
                {
                    p.WaitForExit(4000);
                    if (p.ExitCode == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("[STARTUP] Scheduled task deleted");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[STARTUP] Scheduled task delete failed (code {p.ExitCode})");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[STARTUP] Exception deleting scheduled task: {ex.Message}");
            }

            // Remove registry entry as well
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryStartupPath, true);
                if (key != null)
                {
                    key.DeleteValue(RegistryValueName, false); // Don't throw if doesn't exist
                    System.Diagnostics.Debug.WriteLine("[STARTUP] Registry startup entry deleted");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[STARTUP] Exception deleting registry entry: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if startup is configured via either scheduled task or registry.
        /// </summary>
        /// <returns>True if startup is configured, false otherwise</returns>
        public static bool Exists()
        {
            // Check for scheduled task first
            try
            {
                var psi = new ProcessStartInfo("schtasks.exe", $"/Query /TN \"{TaskName}\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var p = Process.Start(psi);
                if (p != null)
                {
                    p.WaitForExit(4000);
                    if (p.ExitCode == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("[STARTUP] Scheduled task exists");
                        return true; // Scheduled task exists
                    }
                }
            }
            catch
            {
                // Ignore errors when checking scheduled task
            }

            // Check registry entry as fallback
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryStartupPath, false);
                if (key != null)
                {
                    var value = key.GetValue(RegistryValueName);
                    if (value != null)
                    {
                        System.Diagnostics.Debug.WriteLine("[STARTUP] Registry startup entry exists");
                        return true; // Registry entry exists
                    }
                }
            }
            catch
            {
                // Ignore errors when checking registry
            }

            System.Diagnostics.Debug.WriteLine("[STARTUP] No startup entries found");
            return false;
        }

    }
}
