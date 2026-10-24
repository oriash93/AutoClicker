using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using AutoClicker.Enums;
using AutoClicker.Models;
using Serilog;

namespace AutoClicker.Utils
{
    public static class SettingsUtils
    {
        private static readonly string settingsFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.SETTINGS_FILE_PATH);
        private static readonly string invalidSettingsFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.INVALID_SETTINGS_FILE_PATH);
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

        public static void SetStartHotkey(KeyMapping key)
        {
            CurrentSettings.HotkeySettings.StartHotkey = key;
            NotifyChanges(CurrentSettings.HotkeySettings.StartHotkey, Operation.Start);
        }

        public static void SetStopHotkey(KeyMapping key)
        {
            CurrentSettings.HotkeySettings.StopHotkey = key;
            NotifyChanges(CurrentSettings.HotkeySettings.StopHotkey, Operation.Stop);
        }

        public static void SetToggleHotkey(KeyMapping key)
        {
            CurrentSettings.HotkeySettings.ToggleHotkey = key;
            NotifyChanges(CurrentSettings.HotkeySettings.ToggleHotkey, Operation.Toggle);
        }

        public static void SetIncludeModifiers(bool includeModifiers)
        {
            CurrentSettings.HotkeySettings.IncludeModifiers = includeModifiers;
            NotifyChanges(CurrentSettings.HotkeySettings.StartHotkey, Operation.Start);
            NotifyChanges(CurrentSettings.HotkeySettings.StopHotkey, Operation.Stop);
            NotifyChanges(CurrentSettings.HotkeySettings.ToggleHotkey, Operation.Toggle);
        }

        public static void Reset()
        {
            Log.Information("Reset hotkey settings to default");
            SetStartHotkey(HotkeySettings.defaultStartKeyMapping);
            SetStopHotkey(HotkeySettings.defaultStopKeyMapping);
            SetToggleHotkey(HotkeySettings.defaultToggleKeyMapping);
            SetIncludeModifiers(HotkeySettings.defaultIncludeModifiers);
        }

        private static void NotifyChanges(KeyMapping hotkey, Operation operation)
        {
            HotkeyChangedEventArgs args = new HotkeyChangedEventArgs
            {
                Hotkey = hotkey,
                Operation = operation,
                IncludeModifiers = CurrentSettings.HotkeySettings.IncludeModifiers
            };
            HotkeyChangedEvent.Invoke(null, args);

            SaveSettingsToFile();
        }

        public static event EventHandler<HotkeyChangedEventArgs> HotkeyChangedEvent;

        private static void SaveSettingsToFile()
        {
            try
            {
                JsonUtils.WriteJson(settingsFilePath, CurrentSettings);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                Log.Error(e, "Failed to save settings to {FilePath}", settingsFilePath);
                MessageBox.Show(string.Format(Constants.SAVE_SETTINGS_FAILED_MESSAGE, settingsFilePath, e.Message),
                    Constants.MAIN_WINDOW_TITLE_DEFAULT, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public static void LoadSettingsFromFile()
        {
            ApplicationSettings applicationSettings = null;
            try
            {
                applicationSettings = JsonUtils.ReadJson<ApplicationSettings>(settingsFilePath);
            }
            catch (JsonException)
            {
                Log.Warning("Settings file {FilePath} is invalid, using default settings", settingsFilePath);
                MoveInvalidSettingsFile();
            }

            CurrentSettings = applicationSettings ?? new ApplicationSettings();
            CurrentSettings.FillMissingValues();
        }

        private static void MoveInvalidSettingsFile()
        {
            try
            {
                File.Move(settingsFilePath, invalidSettingsFilePath, true);
                Log.Information("Moved invalid settings file to {FilePath}", invalidSettingsFilePath);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                Log.Error(e, "Failed to move invalid settings file {FilePath}", settingsFilePath);
            }
        }

        public static void SetApplicationSettings(AutoClickerSettings settings)
        {
            CurrentSettings.AutoClickerSettings.Milliseconds = settings.Milliseconds;
            CurrentSettings.AutoClickerSettings.Seconds = settings.Seconds;
            CurrentSettings.AutoClickerSettings.Minutes = settings.Minutes;
            CurrentSettings.AutoClickerSettings.Hours = settings.Hours;

            CurrentSettings.AutoClickerSettings.IsRandomOffsetEnabled = settings.IsRandomOffsetEnabled;
            CurrentSettings.AutoClickerSettings.RandomOffsetMilliseconds = settings.RandomOffsetMilliseconds;

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
