using System.Globalization;
using System.Windows.Data;
using AutoClicker.Enums;
using Xunit;

namespace AutoClicker.Tests.Enums
{
    public class EnumToBooleanConverterTests
    {
        private readonly EnumToBooleanConverter converter = new EnumToBooleanConverter();

        [Fact]
        public void Convert_MatchingValue_ReturnsTrue()
        {
            Assert.Equal(true, converter.Convert(RepeatMode.Count, typeof(bool), RepeatMode.Count, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void Convert_OtherValue_ReturnsFalse()
        {
            Assert.Equal(false, converter.Convert(RepeatMode.Infinite, typeof(bool), RepeatMode.Count, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void ConvertBack_Checked_ReturnsParameter()
        {
            Assert.Equal(LocationMode.PickedLocation, converter.ConvertBack(true, typeof(LocationMode), LocationMode.PickedLocation, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void ConvertBack_Unchecked_DoesNothing()
        {
            Assert.Equal(Binding.DoNothing, converter.ConvertBack(false, typeof(LocationMode), LocationMode.PickedLocation, CultureInfo.InvariantCulture));
        }
    }
}
