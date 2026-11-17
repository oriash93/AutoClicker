using System.Windows.Input;
using AutoClicker.Commands;
using Xunit;

namespace AutoClicker.Tests.Commands
{
    public class CommandUtilsTests
    {
        [Fact]
        public void CreateCommand_WithoutKeyGesture()
        {
            RoutedUICommand command = CommandUtils.CreateCommand(typeof(CommandUtilsTests), "Start");

            Assert.Equal("Start", command.Name);
            Assert.Equal("Start", command.Text);
            Assert.Equal(typeof(CommandUtilsTests), command.OwnerType);
            Assert.Empty(command.InputGestures);
        }

        [Fact]
        public void CreateCommand_WithKeyGesture()
        {
            KeyGesture keyGesture = new(Key.F4, ModifierKeys.Alt);

            RoutedUICommand command = CommandUtils.CreateCommand(typeof(CommandUtilsTests), "Exit", keyGesture);

            Assert.Equal("Exit", command.Name);
            Assert.Same(keyGesture, Assert.Single(command.InputGestures));
        }
    }
}
