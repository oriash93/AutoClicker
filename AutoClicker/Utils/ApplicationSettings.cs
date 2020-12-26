namespace AutoClicker.Utils
{
    public class ApplicationSettings
    {
        public Hotkey StartHotkey { get; set; } = new Hotkey(Constants.DEFAULT_START_HOTKEY);

        public Hotkey StopHotkey { get; set; } = new Hotkey(Constants.DEFAULT_STOP_HOTKEY);
    }
}
