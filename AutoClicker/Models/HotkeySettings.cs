using AutoClicker.Utils;

namespace AutoClicker.Models
{
    public class HotkeySettings
    {
        public static readonly KeyMapping defaultStartKeyMapping = KeyMappingUtils.GetKeyMappingByCode(Constants.DEFAULT_START_HOTKEY);

        public static readonly KeyMapping defaultStopKeyMapping = KeyMappingUtils.GetKeyMappingByCode(Constants.DEFAULT_STOP_HOTKEY);

        public KeyMapping StartHotkey { get; set; } = defaultStartKeyMapping;

        public KeyMapping StopHotkey { get; set; } = defaultStopKeyMapping;
    }
}