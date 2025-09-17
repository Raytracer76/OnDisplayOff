# OnDisplayOff

**Automatically perform power actions when your display turns off**

OnDisplayOff is a lightweight Windows utility that monitors display power state and automatically performs configured actions (sleep, hibernate, shutdown, or restart) when the screen turns off, with an optional grace period.

## Problem It Solves

Many modern applications and browsers prevent Windows from sleeping even when the display turns off, leaving your computer running indefinitely. OnDisplayOff solves this by monitoring actual display power events rather than system idle timers, ensuring your PC performs the desired power action regardless of what applications are running.

## Features

- **Multiple Power Actions**: Sleep, Hibernate, Shutdown, or Restart
- **Configurable Grace Period**: 0-600 seconds delay before action (0 = immediate)
- **System Tray Interface**: Runs quietly in the background with easy access
- **Startup Integration**: Optional automatic startup with Windows
- **Pause/Resume**: Temporarily disable without closing the application
- **Event-Driven**: Monitors actual display power state, not idle timers

## How It Works

The application registers for Windows power broadcast notifications and monitors two key events:
- **Console Display State** (GUID_CONSOLE_DISPLAY_STATE): Primary display power changes
- **Monitor Power State** (GUID_MONITOR_POWER_ON): Secondary monitor power events

When either event indicates the display has turned off, OnDisplayOff starts a configurable grace period. If the display turns back on during this period, the action is cancelled. Otherwise, the configured power action is executed.

## Usage

### Installation & Setup
1. Build the project or download a release
2. Run `OnDisplayOff.exe`
3. Double-click the system tray icon to open settings
4. Configure your preferred action and grace period

### Settings
- **Grace Period**: Seconds to wait after display turns off (0-600)
- **Action**: Sleep, Hibernate, Shutdown, or Restart
- **Start at Logon**: Automatically start with Windows
- **Paused**: Temporarily disable power actions

### System Tray Menu
- **Pause/Resume**: Toggle power action functionality
- **Settings**: Open configuration dialog
- **Exit**: Close the application

## Requirements

- **Platform**: Windows (any version supporting .NET 8)
- **Framework**: .NET 8 Windows Desktop Runtime
- **Architecture**: Any CPU, x64, or ARM64
- **Privileges**: Standard user (Shutdown/Restart require elevation via scheduled task)

## Build Instructions

```bash
# Clone the repository
git clone https://github.com/yourusername/OnDisplayOff.git
cd OnDisplayOff

# Build the project
dotnet build -c Release

# Run the application
dotnet run
```

## Special Considerations

### Hibernate Support
To enable hibernate functionality, run this command as administrator:
```cmd
powercfg /hibernate on
```

### Modern Standby (S0) Devices
On devices with Modern Standby, "Sleep" may behave differently than traditional S3 sleep. Hibernate remains the most reliable option for truly powering down the system.

### Elevated Privileges
Shutdown and Restart actions require elevated privileges. The application handles this automatically by:
1. Creating a Windows scheduled task with highest privileges
2. Requesting appropriate shutdown privileges at runtime
3. Using the `EWX_FORCEIFHUNG` flag to handle unresponsive applications

## Troubleshooting

**Application doesn't start automatically**: Check if the Windows scheduled task was created successfully. Group policies may prevent task creation.

**Shutdown/Restart not working**: Ensure the application has appropriate privileges. Try running as administrator once to establish the scheduled task.

**Grace period not working**: Verify the grace period is set correctly in settings. Check Windows Event Viewer for power-related messages.

## Technical Details

- **Language**: C# with .NET 8
- **UI Framework**: Windows Forms
- **Power Management**: Native Windows APIs (User32, Powrprof, Advapi32)
- **Settings Storage**: JSON in `%AppData%\SleepOnDisplayOff\settings.json`
- **Single Instance**: Uses named mutex to prevent multiple instances

## License

MIT License - see [LICENSE](LICENSE) for details.

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.