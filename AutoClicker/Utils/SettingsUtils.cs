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
            CurrentSettings.StartHotkey = key;
            NotifyChanges(CurrentSettings.StartHotkey, Operation.Start);
        }

        public static void SetStopHotKey(KeyMapping key)
        {
            CurrentSettings.StopHotkey = key;
            NotifyChanges(CurrentSettings.StopHotkey, Operation.Stop);
        }

        public static void Reset()
        {
            Log.Information("Reset settings to default");
            SetStartHotKey(ApplicationSettings.defaultStartKeyMapping);
            SetStopHotKey(ApplicationSettings.defaultStopKeyMapping);
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
            CurrentSettings = JsonUtils.ReadJson<ApplicationSettings>(settingsFilePath);
        }

        public static void SetApplicationSettings(AutoClickerSettings settings)
        {
            CurrentSettings.AutoClickerSettings.Milliseconds = settings.Milliseconds;
            CurrentSettings.AutoClickerSettings.Seconds = settings.Seconds;
            CurrentSettings.AutoClickerSettings.Minutes = settings.Minutes;
            CurrentSettings.AutoClickerSettings.Hours = settings.Hours;

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
