using System;
using System.IO;
using System.Text.Json;

namespace OnDisplayOff
{
    /// <summary>
    /// Defines the possible power management actions that can be performed when the display turns off.
    /// </summary>
    public enum SleepAction
    {
        /// <summary>Put the computer to sleep (suspend to RAM)</summary>
        Sleep,
        /// <summary>Hibernate the computer (suspend to disk)</summary>
        Hibernate,
        /// <summary>Shutdown the computer completely</summary>
        Shutdown,
        /// <summary>Restart the computer</summary>
        Restart
    }

    /// <summary>
    /// Defines the available time units for the grace period setting.
    /// </summary>
    public enum TimeUnit
    {
        /// <summary>Milliseconds (1/1000 of a second)</summary>
        Milliseconds,
        /// <summary>Seconds</summary>
        Seconds,
        /// <summary>Minutes</summary>
        Minutes,
        /// <summary>Hours</summary>
        Hours,
        /// <summary>Days</summary>
        Days
    }

    /// <summary>
    /// Manages application configuration settings including power actions, timing, and startup behavior.
    /// Settings are automatically persisted to JSON in the user's AppData folder.
    /// </summary>
    public sealed class AppSettings
    {
        /// <summary>
        /// Grace period value to wait after display turns off before executing the power action.
        /// Set to 0 for immediate action. Use with GraceTimeUnit to define the time scale.
        /// </summary>
        public int GraceValue { get; set; } = 60;

        /// <summary>
        /// Time unit for the grace period value. Defaults to seconds for backward compatibility.
        /// </summary>
        public TimeUnit GraceTimeUnit { get; set; } = TimeUnit.Seconds;

        /// <summary>
        /// Legacy property for backward compatibility. Gets/sets grace period in seconds.
        /// When setting, automatically converts to GraceValue and GraceTimeUnit.
        /// </summary>
        [Obsolete("Use GraceValue and GraceTimeUnit instead")]
        public int GraceSeconds
        {
            get => GetTotalSeconds();
            set
            {
                GraceValue = value;
                GraceTimeUnit = TimeUnit.Seconds;
            }
        }
        
        /// <summary>
        /// The power management action to perform when the display turns off and grace period expires.
        /// </summary>
        public SleepAction Action { get; set; } = SleepAction.Sleep;
        
        /// <summary>
        /// Whether to automatically start the application when the user logs on to Windows.
        /// </summary>
        public bool StartAtLogon { get; set; } = true;
        
        /// <summary>
        /// Whether the application is currently paused and should not perform any power actions.
        /// </summary>
        public bool Paused { get; set; } = false;

        /// <summary>
        /// Gets the directory path where application settings are stored.
        /// Located in the user's AppData\Roaming folder.
        /// </summary>
        public static string Dir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "OnDisplayOff");
            
        /// <summary>
        /// Gets the full path to the settings JSON file.
        /// </summary>
        public static string PathJson => Path.Combine(Dir, "settings.json");

        /// <summary>
        /// Converts the current grace period value and unit to total seconds.
        /// </summary>
        /// <returns>Total seconds for the grace period</returns>
        public int GetTotalSeconds()
        {
            return GraceTimeUnit switch
            {
                TimeUnit.Milliseconds => GraceValue / 1000,
                TimeUnit.Seconds => GraceValue,
                TimeUnit.Minutes => GraceValue * 60,
                TimeUnit.Hours => GraceValue * 3600,
                TimeUnit.Days => GraceValue * 86400,
                _ => GraceValue
            };
        }

        /// <summary>
        /// Sets the grace period from a total number of seconds, automatically choosing the best unit.
        /// </summary>
        /// <param name="totalSeconds">Total seconds for the grace period</param>
        public void SetGracePeriod(int totalSeconds)
        {
            if (totalSeconds == 0)
            {
                GraceValue = 0;
                GraceTimeUnit = TimeUnit.Seconds;
            }
            else if (totalSeconds % 86400 == 0)
            {
                GraceValue = totalSeconds / 86400;
                GraceTimeUnit = TimeUnit.Days;
            }
            else if (totalSeconds % 3600 == 0)
            {
                GraceValue = totalSeconds / 3600;
                GraceTimeUnit = TimeUnit.Hours;
            }
            else if (totalSeconds % 60 == 0)
            {
                GraceValue = totalSeconds / 60;
                GraceTimeUnit = TimeUnit.Minutes;
            }
            else
            {
                GraceValue = totalSeconds;
                GraceTimeUnit = TimeUnit.Seconds;
            }
        }

        /// <summary>
        /// Loads application settings from the JSON configuration file.
        /// If the file doesn't exist or cannot be read, returns default settings.
        /// </summary>
        /// <returns>AppSettings instance with loaded or default values</returns>
        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(PathJson))
                {
                    var json = File.ReadAllText(PathJson);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch 
            { 
                // If deserialization fails (corrupted file, version mismatch, etc.),
                // fall back to default settings rather than crashing
            }
            return new AppSettings();
        }

        /// <summary>
        /// Saves the current settings to the JSON configuration file.
        /// Creates the settings directory if it doesn't exist.
        /// </summary>
        public void Save()
        {
            // Ensure the settings directory exists
            Directory.CreateDirectory(Dir);
            
            // Serialize settings to formatted JSON for readability
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions{ WriteIndented = true });
            
            // Write to the settings file
            File.WriteAllText(PathJson, json);
        }
    }
}
