using System;
using System.IO;
using System.Text.Json;

namespace SleepOnDisplayOff
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
    /// Manages application configuration settings including power actions, timing, and startup behavior.
    /// Settings are automatically persisted to JSON in the user's AppData folder.
    /// </summary>
    public sealed class AppSettings
    {
        /// <summary>
        /// Grace period in seconds to wait after display turns off before executing the power action.
        /// Set to 0 for immediate action. Maximum value is 600 seconds (10 minutes).
        /// </summary>
        public int GraceSeconds { get; set; } = 60;
        
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
            "SleepOnDisplayOff");
            
        /// <summary>
        /// Gets the full path to the settings JSON file.
        /// </summary>
        public static string PathJson => Path.Combine(Dir, "settings.json");

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
