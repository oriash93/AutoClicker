using System.ComponentModel;
using System.Text.Json.Serialization;
using AutoClicker.Enums;
using AutoClicker.Utils;

namespace AutoClicker.Models
{
    public class AutoClickerSettings : IDataErrorInfo
    {
        public int Hours { get; set; }

        public int Minutes { get; set; }

        public int Seconds { get; set; }

        public int Milliseconds { get; set; }

        public bool IsRandomOffsetEnabled { get; set; }

        public int RandomOffsetMilliseconds { get; set; }

        public MouseButton SelectedMouseButton { get; set; }

        public MouseAction SelectedMouseAction { get; set; }

        public RepeatMode SelectedRepeatMode { get; set; }

        public LocationMode SelectedLocationMode { get; set; }

        public int PickedXValue { get; set; }

        public int PickedYValue { get; set; }

        public int SelectedTimesToRepeat { get; set; }

        [JsonIgnore]
        public string Error => null;

        public string this[string propertyName]
        {
            get
            {
                int? value = propertyName switch
                {
                    nameof(Hours) => Hours,
                    nameof(Minutes) => Minutes,
                    nameof(Seconds) => Seconds,
                    nameof(Milliseconds) => Milliseconds,
                    nameof(RandomOffsetMilliseconds) => RandomOffsetMilliseconds,
                    _ => null
                };
                return value < 0 ? Constants.NEGATIVE_VALUE_ERROR : null;
            }
        }
    }
}
