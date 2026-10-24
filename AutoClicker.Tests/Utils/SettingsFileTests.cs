using System;
using System.IO;
using AutoClicker.Enums;
using AutoClicker.Models;
using AutoClicker.Utils;
using Xunit;

namespace AutoClicker.Tests.Utils
{
    public class SettingsFileTests : IDisposable
    {
        // Settings file as written by v3.4.0, before the random offset settings existed
        private const string LegacySettingsJson = """
            {
              "HotkeySettings": {
                "StartHotkey": { "display_name": "F6", "virtual_key_name": "VK_F6", "virtual_key_hex_code": "0x75", "virtual_key_code": 117 },
                "StopHotkey": { "display_name": "F7", "virtual_key_name": "VK_F7", "virtual_key_hex_code": "0x76", "virtual_key_code": 118 },
                "ToggleHotkey": { "display_name": "F8", "virtual_key_name": "VK_F8", "virtual_key_hex_code": "0x77", "virtual_key_code": 119 },
                "IncludeModifiers": true
              },
              "AutoClickerSettings": {
                "Hours": 0,
                "Minutes": 0,
                "Seconds": 3,
                "Milliseconds": 250,
                "SelectedMouseButton": 1,
                "SelectedMouseAction": 0,
                "SelectedRepeatMode": 1,
                "SelectedLocationMode": 1,
                "PickedXValue": -771,
                "PickedYValue": 182,
                "SelectedTimesToRepeat": 3
              }
            }
            """;

        private readonly string filePath = Path.Combine(Path.GetTempPath(), $"AutoClicker_Tests_{Guid.NewGuid()}.json");

        public void Dispose()
        {
            if (File.Exists(filePath))
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
            }
            File.Delete(filePath + ".tmp");
        }

        [Fact]
        public void WriteJson_ReplacesExistingFileWithoutLeavingTempFile()
        {
            File.WriteAllText(filePath, LegacySettingsJson);
            ApplicationSettings settings = new ApplicationSettings();
            settings.AutoClickerSettings.Seconds = 7;

            JsonUtils.WriteJson(filePath, settings);

            Assert.Equal(7, JsonUtils.ReadJson<ApplicationSettings>(filePath).AutoClickerSettings.Seconds);
            Assert.False(File.Exists(filePath + ".tmp"));
        }

        [Fact]
        public void WriteJson_WhenFileCannotBeReplaced_KeepsOriginalFile()
        {
            File.WriteAllText(filePath, LegacySettingsJson);
            File.SetAttributes(filePath, FileAttributes.ReadOnly);

            Assert.ThrowsAny<Exception>(() => JsonUtils.WriteJson(filePath, new ApplicationSettings()));

            Assert.Equal(LegacySettingsJson, File.ReadAllText(filePath));
            Assert.False(File.Exists(filePath + ".tmp"));
        }

        [Fact]
        public void ReadJson_NullSections_AreFilledWithDefaults()
        {
            File.WriteAllText(filePath, """{ "HotkeySettings": { "StartHotkey": null, "StopHotkey": null }, "AutoClickerSettings": null }""");

            ApplicationSettings settings = JsonUtils.ReadJson<ApplicationSettings>(filePath);
            settings.FillMissingValues();

            Assert.NotNull(settings.AutoClickerSettings);
            Assert.Equal(Constants.DEFAULT_START_HOTKEY, settings.HotkeySettings.StartHotkey.VirtualKeyCode);
            Assert.Equal(Constants.DEFAULT_STOP_HOTKEY, settings.HotkeySettings.StopHotkey.VirtualKeyCode);
            Assert.Equal(Constants.DEFAULT_TOGGLE_HOTKEY, settings.HotkeySettings.ToggleHotkey.VirtualKeyCode);
        }

        [Fact]
        public void FillMissingValues_KeepsExistingValues()
        {
            File.WriteAllText(filePath, LegacySettingsJson);
            ApplicationSettings settings = JsonUtils.ReadJson<ApplicationSettings>(filePath);
            settings.HotkeySettings.StartHotkey = KeyMappingUtils.GetKeyMappingByCode(Constants.VK_OEM_8);

            settings.FillMissingValues();

            Assert.Equal(Constants.VK_OEM_8, settings.HotkeySettings.StartHotkey.VirtualKeyCode);
            Assert.Equal(3, settings.AutoClickerSettings.Seconds);
        }

        [Fact]
        public void ReadJson_InvalidFile_ThrowsJsonException()
        {
            File.WriteAllText(filePath, """{ "HotkeySettings": { "StartHotkey": { "display_name": "F6",""");

            Assert.ThrowsAny<System.Text.Json.JsonException>(() => JsonUtils.ReadJson<ApplicationSettings>(filePath));
        }

        [Fact]
        public void ReadJson_LegacySettings_KeepsValuesAndDisablesRandomOffset()
        {
            File.WriteAllText(filePath, LegacySettingsJson);

            ApplicationSettings settings = JsonUtils.ReadJson<ApplicationSettings>(filePath);

            Assert.Equal(3, settings.AutoClickerSettings.Seconds);
            Assert.Equal(250, settings.AutoClickerSettings.Milliseconds);
            Assert.Equal(MouseButton.Right, settings.AutoClickerSettings.SelectedMouseButton);
            Assert.Equal(-771, settings.AutoClickerSettings.PickedXValue);
            Assert.Equal(3, settings.AutoClickerSettings.SelectedTimesToRepeat);
            Assert.Equal(0x75, settings.HotkeySettings.StartHotkey.VirtualKeyCode);
            Assert.True(settings.HotkeySettings.IncludeModifiers);
            Assert.False(settings.AutoClickerSettings.IsRandomOffsetEnabled);
            Assert.Equal(0, settings.AutoClickerSettings.RandomOffsetMilliseconds);
        }

        [Fact]
        public void WriteJson_ThenReadJson_KeepsAllValues()
        {
            ApplicationSettings original = new ApplicationSettings();
            original.AutoClickerSettings.Seconds = 2;
            original.AutoClickerSettings.IsRandomOffsetEnabled = true;
            original.AutoClickerSettings.RandomOffsetMilliseconds = 500;
            original.AutoClickerSettings.SelectedLocationMode = LocationMode.PickedLocation;
            original.AutoClickerSettings.PickedXValue = 343;
            original.HotkeySettings.ToggleHotkey = KeyMappingUtils.GetKeyMappingByCode(Constants.VK_OEM_8);

            JsonUtils.WriteJson(filePath, original);
            ApplicationSettings loaded = JsonUtils.ReadJson<ApplicationSettings>(filePath);

            Assert.Equal(2, loaded.AutoClickerSettings.Seconds);
            Assert.True(loaded.AutoClickerSettings.IsRandomOffsetEnabled);
            Assert.Equal(500, loaded.AutoClickerSettings.RandomOffsetMilliseconds);
            Assert.Equal(LocationMode.PickedLocation, loaded.AutoClickerSettings.SelectedLocationMode);
            Assert.Equal(343, loaded.AutoClickerSettings.PickedXValue);
            Assert.Equal(Constants.VK_OEM_8, loaded.HotkeySettings.ToggleHotkey.VirtualKeyCode);
        }

        [Fact]
        public void ReadJson_MissingFile_ReturnsNull()
        {
            Assert.Null(JsonUtils.ReadJson<ApplicationSettings>(filePath));
        }
    }
}
