using System;
using System.IO;
using AutoClicker.Enums;
using Serilog;

namespace AutoClicker.Utils
{
    public static class SettingsUtils
    {
        private static readonly string settingsFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.SETTINGS_FILE_PATH);

        public static ApplicationSettings CurrentSettings { get; set; }

        static SettingsUtils()
        {
            CurrentSettings = new ApplicationSettings();
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

            SaveSettings();
        }

        public static event EventHandler<HotkeyChangedEventArgs> HotKeyChangedEvent;

        private static void SaveSettings()
        {
            JsonUtils.WriteJson(settingsFilePath, CurrentSettings);
        }

        public static void LoadSettings()
        {
            CurrentSettings = JsonUtils.ReadJson<ApplicationSettings>(settingsFilePath);
        }
    }
}
