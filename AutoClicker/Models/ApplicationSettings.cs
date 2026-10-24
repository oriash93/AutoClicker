namespace AutoClicker.Models
{
    public class ApplicationSettings
    {
        public HotkeySettings HotkeySettings { get; set; } = new HotkeySettings();

        public AutoClickerSettings AutoClickerSettings { get; set; } = new AutoClickerSettings();

        public void FillMissingValues()
        {
            HotkeySettings ??= new HotkeySettings();
            AutoClickerSettings ??= new AutoClickerSettings();

            HotkeySettings.StartHotkey ??= HotkeySettings.defaultStartKeyMapping;
            HotkeySettings.StopHotkey ??= HotkeySettings.defaultStopKeyMapping;
            HotkeySettings.ToggleHotkey ??= HotkeySettings.defaultToggleKeyMapping;
        }
    }
}
