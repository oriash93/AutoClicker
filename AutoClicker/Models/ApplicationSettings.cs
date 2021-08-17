namespace AutoClicker.Models
{
    public class ApplicationSettings
    {
        public HotkeySettings HotkeySettings { get; set; } = new HotkeySettings();

        public AutoClickerSettings AutoClickerSettings { get; set; } = new AutoClickerSettings();
    }
}
