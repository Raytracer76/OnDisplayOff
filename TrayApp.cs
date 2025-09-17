using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static OnDisplayOff.PowerInterop;

namespace OnDisplayOff
{
    /// <summary>
    /// Main application form that runs as a system tray application.
    /// Handles display power notifications and executes configured power actions after a grace period.
    /// The form itself is hidden - all interaction occurs through the system tray icon.
    /// </summary>
    public sealed class TrayApp : Form
    {
        /// <summary>System tray icon and context menu</summary>
        private readonly NotifyIcon _tray = new();
        
        /// <summary>Handles for registered power setting notifications</summary>
        private IntPtr _hNotifyConsole = IntPtr.Zero, _hNotifyMonitor = IntPtr.Zero;
        
        /// <summary>Timer for grace period countdown before executing power action</summary>
        private readonly System.Windows.Forms.Timer _graceTimer = new() { Interval = 60_000 };
        
        /// <summary>Current application settings</summary>
        private AppSettings _cfg = null!;

        /// <summary>
        /// Initializes the TrayApp form.
        /// Sets up the form as hidden and configures event handlers.
        /// </summary>
        public TrayApp()
        {
            // Hide the form completely - we only use the tray icon
            ShowInTaskbar = false;
            Opacity = 0;
            
            // Wire up event handlers
            Load += OnLoad;
            FormClosed += OnClosed;
            _graceTimer.Tick += (_, __) => OnGraceElapsed();
        }

        /// <summary>
        /// Handles the form load event. Initializes settings, tray icon, and power notifications.
        /// </summary>
        private void OnLoad(object? s, EventArgs e)
        {
            // Load application settings from disk
            _cfg = AppSettings.Load();
            
            // Configure Windows startup task based on settings
            ApplySchedulerState(_cfg.StartAtLogon);
            
            // Set grace timer interval from settings (convert seconds to milliseconds)
            // Timer interval must be > 0, so use 1ms minimum (won't be used when grace is 0)
            _graceTimer.Interval = Math.Max(1, _cfg.GetTotalSeconds()) * 1000;

            // Set up system tray icon and context menu
            _tray.Icon = SystemIcons.Application;
            _tray.Visible = true;
            _tray.Text = "OnDisplayOff";
            
            // Create context menu with current pause state
            var menu = new ContextMenuStrip();
            var pauseItem = new ToolStripMenuItem(_cfg.Paused ? "Resume" : "Pause", null, (_,__) => TogglePause());
            menu.Items.Add(pauseItem);
            menu.Items.Add(new ToolStripMenuItem("Settings…", null, (_,__) => OpenSettings(pauseItem)));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem("Exit", null, (_,__) => Close()));
            _tray.ContextMenuStrip = menu;
            
            // Double-click tray icon opens settings
            _tray.DoubleClick += (_,__) => OpenSettings(pauseItem);

            // Register to receive Windows power setting notifications
            var consoleGuid = GUID_CONSOLE_DISPLAY_STATE;
            var monitorGuid = GUID_MONITOR_POWER_ON;
            _hNotifyConsole = RegisterPowerSettingNotification(Handle, ref consoleGuid, DEVICE_NOTIFY_WINDOW_HANDLE);
            _hNotifyMonitor = RegisterPowerSettingNotification(Handle, ref monitorGuid, DEVICE_NOTIFY_WINDOW_HANDLE);
        }

        /// <summary>
        /// Toggles the paused state of the application.
        /// When paused, no power actions will be performed regardless of display state.
        /// </summary>
        private void TogglePause()
        {
            // Toggle pause state and save to disk
            _cfg.Paused = !_cfg.Paused;
            _cfg.Save();
            
            // Update the context menu text
            if (_tray.ContextMenuStrip?.Items[0] is ToolStripMenuItem pauseMenuItem)
                pauseMenuItem.Text = _cfg.Paused ? "Resume" : "Pause";
            
            // Stop any active grace timer when pausing
            if (_cfg.Paused) _graceTimer.Stop();
            
            // Show balloon notification to user
            _tray.BalloonTipTitle = "OnDisplayOff";
            _tray.BalloonTipText = _cfg.Paused ? "Paused" : "Resumed";
            _tray.ShowBalloonTip(2000);
        }

        /// <summary>
        /// Opens the settings dialog and applies any changes made by the user.
        /// </summary>
        /// <param name="pauseItem">Reference to the pause menu item for updating its text</param>
        private void OpenSettings(ToolStripMenuItem pauseItem)
        {
            using var dlg = new SettingsForm(_cfg);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                // Apply the new settings
                _cfg = dlg.Result ?? new AppSettings();
                _cfg.Save();
                
                // Update grace timer interval
                // Timer interval must be > 0, so use 1ms minimum (won't be used when grace is 0)
                _graceTimer.Interval = Math.Max(1, _cfg.GetTotalSeconds()) * 1000;
                
                // Update Windows startup task
                ApplySchedulerState(_cfg.StartAtLogon);
                
                // Update pause menu item text
                pauseItem.Text = _cfg.Paused ? "Resume" : "Pause";
            }
        }

        /// <summary>
        /// Processes Windows messages, specifically power broadcast notifications.
        /// Monitors display state changes and starts/cancels grace periods accordingly.
        /// </summary>
        /// <param name="m">The Windows message to process</param>
        protected override void WndProc(ref Message m)
        {
            // Check if this is a power broadcast message for power setting changes
            if (m.Msg == WM_POWERBROADCAST && m.WParam.ToInt32() == PBT_POWERSETTINGCHANGE)
            {
                // Skip processing if application is paused
                if (_cfg.Paused) { base.WndProc(ref m); return; }

                // Parse the power broadcast setting structure
                var pbs = Marshal.PtrToStructure<POWERBROADCAST_SETTING>(m.LParam);
                
                // Read the actual DWORD value from the Data field
                var value = (uint)Marshal.ReadInt32(m.LParam, Marshal.OffsetOf<POWERBROADCAST_SETTING>("Data").ToInt32());

                if (pbs.PowerSetting == GUID_CONSOLE_DISPLAY_STATE)
                {
                    // Console display state: 0=Off, 1=On, 2=Dimmed
                    // Start grace period when off, cancel when on or dimmed
                    if (value == 0) 
                        StartGrace("ConsoleDisplay:OFF"); 
                    else 
                        CancelGrace($"ConsoleDisplay:{value}");
                }
                else if (pbs.PowerSetting == GUID_MONITOR_POWER_ON)
                {
                    // Monitor power state: 0=Off, 1=On
                    // Start grace period when off, cancel when on
                    if (value == 0) 
                        StartGrace("MonitorPower:OFF"); 
                    else 
                        CancelGrace("MonitorPower:ON");
                }
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// Starts the grace period countdown before executing the configured power action.
        /// If grace period is 0, executes the action immediately.
        /// </summary>
        /// <param name="reason">Diagnostic string describing why the grace period started</param>
        private void StartGrace(string reason)
        {
            if (_cfg.GetTotalSeconds() == 0)
            {
                // No grace period - execute action immediately
                Debug.WriteLine($"[SOFF] Screen OFF → immediate action ({reason})");
                ExecuteAction(_cfg.Action);
                return;
            }
            
            // Start grace period countdown
            Debug.WriteLine($"[SOFF] Screen OFF → grace {_cfg.GetTotalSeconds()}s ({reason})");
            _graceTimer.Stop();  // Stop any existing timer
            _graceTimer.Start(); // Start fresh countdown
            
            // Update tray tooltip to show countdown
            _tray.Text = $"OnDisplayOff (T-{_cfg.GetTotalSeconds()}s)";
        }

        /// <summary>
        /// Cancels any active grace period countdown (e.g., when display turns back on).
        /// </summary>
        /// <param name="reason">Diagnostic string describing why the grace period was cancelled</param>
        private void CancelGrace(string reason)
        {
            if (_graceTimer.Enabled)
            {
                Debug.WriteLine($"[SOFF] Cancel grace ({reason})");
                _graceTimer.Stop();
                
                // Reset tray tooltip to normal text
                _tray.Text = "OnDisplayOff";
            }
        }

        /// <summary>
        /// Called when the grace period timer expires.
        /// Executes the configured power action.
        /// </summary>
        private void OnGraceElapsed()
        {
            _graceTimer.Stop();
            
            // Reset tray tooltip
            _tray.Text = "OnDisplayOff";
            
            // Execute the configured power action
            ExecuteAction(_cfg.Action);
        }

        /// <summary>
        /// Applies the Windows startup task state based on the StartAtLogon setting.
        /// Creates or deletes the scheduled task as needed.
        /// </summary>
        /// <param name="enable">True to enable startup, false to disable</param>
        private void ApplySchedulerState(bool enable)
        {
            try
            {
                if (enable) 
                    StartupTask.CreateOrUpdate();
                else 
                    StartupTask.Delete();
            } 
            catch 
            { 
                // Ignore errors - startup task management is best-effort
                // May fail due to insufficient privileges or group policy restrictions
            }
        }

        /// <summary>
        /// Handles form closing. Cleans up power notification registrations and disposes resources.
        /// </summary>
        private void OnClosed(object? s, EventArgs e)
        {
            // Unregister power setting notifications
            if (_hNotifyConsole != IntPtr.Zero) 
                UnregisterPowerSettingNotification(_hNotifyConsole);
            if (_hNotifyMonitor != IntPtr.Zero) 
                UnregisterPowerSettingNotification(_hNotifyMonitor);
            
            // Clean up tray icon and timer
            _tray.Visible = false;
            _tray.Dispose();
            _graceTimer.Dispose();
        }
    }
}
