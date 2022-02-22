using System.ComponentModel;
using System.Runtime.CompilerServices;
using AutoClicker.Annotations;
using AutoClicker.Enums;

namespace AutoClicker.Models
{
    public class AutoClickerSettings : INotifyPropertyChanged
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

        public MouseButton SelectedMouseButton { get; set; }

        public MouseAction SelectedMouseAction { get; set; }

        public RepeatMode SelectedRepeatMode { get; set; }

        public LocationMode SelectedLocationMode { get; set; }

        public int PickedXValue { get; set; }

        public int PickedYValue { get; set; }

        public int SelectedTimesToRepeat { get; set; }

        private bool _isRandomizedIntervalEnabled = false;
        public bool IsRandomizedIntervalEnabled
        {
            get => _isRandomizedIntervalEnabled;
            set
            {
                _isRandomizedIntervalEnabled = value;
                OnPropertyChanged(nameof(IsRandomizedIntervalEnabled));
            }
        }



        // The functions below are required to explicitly call the UpdateSourceTrigger for IsRandomizedIntervalEnabled
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
