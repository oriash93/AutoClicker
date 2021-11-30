using System;
using System.IO;
using AutoClicker.Enums;
using AutoClicker.Models;
using Serilog;

namespace AutoClicker.Utils
{
    public static class SettingsUtils
    {
        private static readonly string settingsFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.SETTINGS_FILE_PATH);
        private static readonly string logFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.LOG_FILE_PATH);

        public static ApplicationSettings CurrentSettings { get; set; }

        static SettingsUtils()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(logFilePath)
                .CreateLogger();
            Log.Debug("==================================================");
            Log.Information("Logger initialized successfully");

            LoadSettingsFromFile();
        }

        public static void SetStartHotKey(KeyMapping key)
        {
            CurrentSettings.HotkeySettings.StartHotkey = key;
            NotifyChanges(CurrentSettings.HotkeySettings.StartHotkey, Operation.Start);
        }

        public static void SetStopHotKey(KeyMapping key)
        {
            CurrentSettings.HotkeySettings.StopHotkey = key;
            NotifyChanges(CurrentSettings.HotkeySettings.StopHotkey, Operation.Stop);
        }

        public static void Reset()
        {
            Log.Information("Reset hotkey settings to default");
            SetStartHotKey(HotkeySettings.defaultStartKeyMapping);
            SetStopHotKey(HotkeySettings.defaultStopKeyMapping);
        }

        private static void NotifyChanges(KeyMapping hotkey, Operation operation)
        {
            HotkeyChangedEventArgs args = new HotkeyChangedEventArgs
            {
                Hotkey = hotkey,
                Operation = operation
            };
            HotKeyChangedEvent.Invoke(null, args);

            SaveSettingsToFile();
        }

        public static event EventHandler<HotkeyChangedEventArgs> HotKeyChangedEvent;

        private static void SaveSettingsToFile()
        {
            JsonUtils.WriteJson(settingsFilePath, CurrentSettings);
        }

        public static void LoadSettingsFromFile()
        {
            ApplicationSettings applicationSettings = JsonUtils.ReadJson<ApplicationSettings>(settingsFilePath);
            if (applicationSettings == null)
            {
                CurrentSettings = new ApplicationSettings();
            }
            else
            {
                CurrentSettings = applicationSettings;
            }
        }

        public static void SetApplicationSettings(AutoClickerSettings settings)
        {
            CurrentSettings.AutoClickerSettings.Milliseconds = settings.Milliseconds;
            CurrentSettings.AutoClickerSettings.Seconds = settings.Seconds;
            CurrentSettings.AutoClickerSettings.Minutes = settings.Minutes;
            CurrentSettings.AutoClickerSettings.Hours = settings.Hours;

            CurrentSettings.AutoClickerSettings.MaximumMilliseconds = settings.MaximumMilliseconds;
            CurrentSettings.AutoClickerSettings.MaximumSeconds = settings.MaximumSeconds;
            CurrentSettings.AutoClickerSettings.MaximumMinutes = settings.MaximumMinutes;
            CurrentSettings.AutoClickerSettings.MaximumHours = settings.MaximumHours;

            CurrentSettings.AutoClickerSettings.MinimumMilliseconds = settings.MinimumMilliseconds;
            CurrentSettings.AutoClickerSettings.MinimumSeconds = settings.MinimumSeconds;
            CurrentSettings.AutoClickerSettings.MinimumMinutes = settings.MinimumMinutes;
            CurrentSettings.AutoClickerSettings.MinimumHours = settings.MinimumHours;

            CurrentSettings.AutoClickerSettings.PickedXValue = settings.PickedXValue;
            CurrentSettings.AutoClickerSettings.PickedYValue = settings.PickedYValue;

            CurrentSettings.AutoClickerSettings.SelectedLocationMode = settings.SelectedLocationMode;
            CurrentSettings.AutoClickerSettings.SelectedMouseAction = settings.SelectedMouseAction;
            CurrentSettings.AutoClickerSettings.SelectedMouseButton = settings.SelectedMouseButton;
            CurrentSettings.AutoClickerSettings.SelectedRepeatMode = settings.SelectedRepeatMode;
            CurrentSettings.AutoClickerSettings.SelectedTimesToRepeat = settings.SelectedTimesToRepeat;

            SaveSettingsToFile();
        }
    }
}
