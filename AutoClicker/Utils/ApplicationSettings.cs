namespace AutoClicker.Utils
{
    public class ApplicationSettings
    {
        public KeyMapping StartHotkey { get; set; } = new KeyMapping(Constants.DEFAULT_START_HOTKEY);

        public KeyMapping StopHotkey { get; set; } = new KeyMapping(Constants.DEFAULT_STOP_HOTKEY);
    }
}
