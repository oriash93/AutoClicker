using System;
using System.IO;
using System.Text.Json;
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
            string jsonString = JsonSerializer.Serialize(CurrentSettings);
            using (StreamWriter streamWriter = File.CreateText(settingsFilePath))
            {
                streamWriter.Write(jsonString);
                Log.Information("Settings saved!");
            }
        }

        public static void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsFilePath))
                {
                    Log.Debug("Read file {FilePath}", settingsFilePath);
                    string jsonString = File.ReadAllText(settingsFilePath);
                    ApplicationSettings settings = JsonSerializer.Deserialize<ApplicationSettings>(jsonString);
                    CurrentSettings = settings;
                }
                else
                {
                    Log.Error("File {FilePath} is missing", settingsFilePath);
                    throw new FileNotFoundException(settingsFilePath);
                }
            }
            catch (JsonException)
            {
                Log.Warning("Failed parsing ApplicationSettings");
            }
            Log.Debug("Read file {FilePath} succesfully", settingsFilePath);
        }
    }
}
