using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OnDisplayOff
{
    internal static class PowerInterop
    {
        // GUIDs
        public static readonly Guid GUID_CONSOLE_DISPLAY_STATE = new("6FE69556-704A-47A0-8F24-C28D936FDA47"); // DWORD 0 Off, 1 On, 2 Dim
        public static readonly Guid GUID_MONITOR_POWER_ON      = new("02731015-4510-4526-99E6-E5A17EBD1AEA"); // DWORD 0 Off, 1 On

        // Notify
        public const int WM_POWERBROADCAST = 0x0218;
        public const int PBT_POWERSETTINGCHANGE = 0x8013;
        public const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;

        [StructLayout(LayoutKind.Sequential)]
        public struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public uint DataLength;
            public byte Data; // first byte; we read explicitly later
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, int Flags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterPowerSettingNotification(IntPtr handle);

        [DllImport("powrprof.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);
        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);
        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges,
            ref TOKEN_PRIVILEGES NewState, uint Zero, IntPtr Null1, IntPtr Null2);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        [StructLayout(LayoutKind.Sequential)] struct LUID { public uint LowPart; public int HighPart; }
        [StructLayout(LayoutKind.Sequential)] struct LUID_AND_ATTRIBUTES { public LUID Luid; public uint Attributes; }
        [StructLayout(LayoutKind.Sequential)] struct TOKEN_PRIVILEGES { public uint PrivilegeCount; public LUID_AND_ATTRIBUTES Privileges; }

        const uint TOKEN_ADJUST_PRIVILEGES = 0x20, TOKEN_QUERY = 0x8;
        const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        const uint SE_PRIVILEGE_ENABLED = 0x2;
        const uint EWX_POWEROFF = 0x00000008, EWX_REBOOT = 0x00000002, EWX_FORCEIFHUNG = 0x00000010;
        const uint SHTDN_REASON_FLAG_PLANNED = 0x80000000;

        static void EnableShutdownPrivilege()
        {
            if (!OpenProcessToken(Process.GetCurrentProcess().Handle, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out var hTok))
                return;
            if (!LookupPrivilegeValue(null!, SE_SHUTDOWN_NAME, out var luid))
                return;
            var tp = new TOKEN_PRIVILEGES
            {
                PrivilegeCount = 1,
                Privileges = new LUID_AND_ATTRIBUTES { Luid = luid, Attributes = SE_PRIVILEGE_ENABLED }
            };
            AdjustTokenPrivileges(hTok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// Executes the specified power management action.
        /// This method handles sleep, hibernate, shutdown, and restart operations.
        /// </summary>
        /// <param name="action">The power action to perform</param>
        public static void ExecuteAction(SleepAction action)
        {
            try
            {
                switch (action)
                {
                    case SleepAction.Hibernate:
                        // Requires: powercfg /hibernate on
                        SetSuspendState(true, false, false);
                        break;
                    case SleepAction.Sleep:
                        SetSuspendState(false, false, false);
                        break;
                    case SleepAction.Shutdown:
                        EnableShutdownPrivilege();
                        ExitWindowsEx(EWX_POWEROFF | EWX_FORCEIFHUNG, SHTDN_REASON_FLAG_PLANNED);
                        break;
                    case SleepAction.Restart:
                        EnableShutdownPrivilege();
                        ExitWindowsEx(EWX_REBOOT | EWX_FORCEIFHUNG, SHTDN_REASON_FLAG_PLANNED);
                        break;
                }
            }
            catch { /* swallow: we’re best-effort */ }
        }
    }
}
