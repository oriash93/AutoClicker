using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Input;

namespace AutoClicker.Utils
{
    public static class AssemblyUtils
    {
        private static readonly Assembly assembly = Assembly.GetExecutingAssembly();

        public static AssemblyName GetAssemblyInfo()
            => assembly.GetName();

        public static Icon GetApplicationIcon()
            => Icon.ExtractAssociatedIcon(assembly.Location);

        public static Uri GetProjectUri()
            => new(assembly.GetCustomAttribute<AssemblyMetadataAttribute>().Value);

        public static RoutedUICommand CreateCommand(Type windowType, string commandName, KeyGesture keyGesture = null)
            => keyGesture == null
                ? new RoutedUICommand(commandName, commandName, windowType)
                : new RoutedUICommand(commandName, commandName, windowType, [keyGesture]);
    }
}
