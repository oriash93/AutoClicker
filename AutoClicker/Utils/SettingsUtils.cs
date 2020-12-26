using System;
using System.IO;
using System.Text.Json;
using System.Windows.Input;
using AutoClicker.Enums;

namespace AutoClicker.Utils
{
    public static class SettingsUtils
    {
        private static readonly string settingsFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.SETTINGS_FILE_NAME);

        private static ApplicationSettings CurrentSettings { get; set; }

        static SettingsUtils()
        {
            CurrentSettings = new ApplicationSettings();
        }

        public static Hotkey GetStartHotKey()
        {
            return CurrentSettings.StartHotkey;
        }

        public static Hotkey GetStopHotKey()
        {
            return CurrentSettings.StopHotkey;
        }

        public static void SetStartHotKey(Key key)
        {
            CurrentSettings.StartHotkey = new Hotkey(key);
            NotifyChanges(CurrentSettings.StartHotkey, Operation.Start);
        }

        public static void SetStopHotKey(Key key)
        {
            CurrentSettings.StopHotkey = new Hotkey(key);
            NotifyChanges(CurrentSettings.StopHotkey, Operation.Stop);
        }

        private static void NotifyChanges(Hotkey hotkey, Operation operation)
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
            string jsonString = JsonSerializer.Serialize(CurrentSettings);
            using (StreamWriter streamWriter = File.CreateText(settingsFilePath))
            {
                streamWriter.Write(jsonString);
            }
        }

        public static void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsFilePath))
                {
                    string jsonString = File.ReadAllText(settingsFilePath);
                    ApplicationSettings settings = JsonSerializer.Deserialize<ApplicationSettings>(jsonString);
                    CurrentSettings = settings;
                }
                else
                {
                    // TODO: log this exception or show error message
                }
            }
            catch (JsonException)
            {
                // TODO: log this exception or show error message
            }
        }
    }
}
