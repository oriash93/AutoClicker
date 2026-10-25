using System;
using System.Drawing;
using System.IO;
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
            => Icon.ExtractAssociatedIcon(Environment.ProcessPath);

        public static Uri GetProjectUri()
            => new(assembly.GetCustomAttribute<AssemblyMetadataAttribute>().Value);

        public static Stream GetResourceStream(string resourceName)
            => assembly.GetManifestResourceStream(resourceName);

        public static RoutedUICommand CreateCommand(Type windowType, string commandName, KeyGesture keyGesture = null)
            => keyGesture == null
                ? new RoutedUICommand(commandName, commandName, windowType)
                : new RoutedUICommand(commandName, commandName, windowType, [keyGesture]);
    }
}
