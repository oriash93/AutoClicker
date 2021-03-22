using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using Serilog;

namespace AutoClicker.Utils
{
    public static class Utilities
    {
        private static readonly Assembly assembly = Assembly.GetExecutingAssembly();
        private static readonly string logFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.LOG_FILE_PATH);

        static Utilities()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(logFilePath)
                .CreateLogger();
        }

        public static AssemblyName GetAssemblyInfo()
            => assembly.GetName();

        public static Icon GetApplicationIcon()
            => Icon.ExtractAssociatedIcon(assembly.Location);

        public static string GetProjectURL()
            => assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;

        public static Uri GetProjectUri()
            => new Uri(GetProjectURL());

        public static RoutedUICommand CreateCommand(Type windowType, string commandName, KeyGesture keyGesture = null)
            => keyGesture == null
                ? new RoutedUICommand(commandName, commandName, windowType)
                : new RoutedUICommand(commandName, commandName, windowType, new InputGestureCollection() { keyGesture });
    }
}
