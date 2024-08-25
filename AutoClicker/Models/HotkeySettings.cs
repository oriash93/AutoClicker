using AutoClicker.Utils;

namespace AutoClicker.Models
{
    public class HotkeySettings
    {
        public static readonly KeyMapping defaultStartKeyMapping = KeyMappingUtils.GetKeyMappingByCode(Constants.DEFAULT_START_HOTKEY);

        public static readonly KeyMapping defaultStopKeyMapping = KeyMappingUtils.GetKeyMappingByCode(Constants.DEFAULT_STOP_HOTKEY);

        public static readonly KeyMapping defaultToggleKeyMapping = KeyMappingUtils.GetKeyMappingByCode(Constants.DEFAULT_TOGGLE_HOTKEY);

        public static readonly bool defaultIncludeModifiers = false;

        public KeyMapping StartHotkey { get; set; } = defaultStartKeyMapping;

        public KeyMapping StopHotkey { get; set; } = defaultStopKeyMapping;

        public KeyMapping ToggleHotkey { get; set; } = defaultToggleKeyMapping;

        public bool IncludeModifiers { get; set; } = defaultIncludeModifiers;
    }
}