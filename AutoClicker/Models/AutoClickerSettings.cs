using AutoClicker.Enums;

namespace AutoClicker.Models
{
    public class AutoClickerSettings
    {
        public int Hours { get; set; }

        public int Minutes { get; set; }

        public int Seconds { get; set; }

        public int Milliseconds { get; set; }

        public int MaximumHours { get; set; }

        public int MaximumMinutes { get; set; }

        public int MaximumSeconds { get; set; }

        public int MaximumMilliseconds { get; set; }

        public int MinimumHours { get; set; }

        public int MinimumMinutes { get; set; }

        public int MinimumSeconds { get; set; }

        public int MinimumMilliseconds { get; set; }

        public bool IsRandomizedIntervalEnabled { get; set; }

        public MouseButton SelectedMouseButton { get; set; }

        public MouseAction SelectedMouseAction { get; set; }

        public RepeatMode SelectedRepeatMode { get; set; }

        public LocationMode SelectedLocationMode { get; set; }

        public int PickedXValue { get; set; }

        public int PickedYValue { get; set; }

        public int SelectedTimesToRepeat { get; set; }
    }
}
