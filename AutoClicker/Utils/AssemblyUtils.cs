using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace AutoClicker.Utils
{
    public static class AssemblyUtils
    {
        private static readonly Assembly assembly = Assembly.GetExecutingAssembly();

        public static AssemblyName GetAssemblyInfo()
            => assembly.GetName();

        public static string GetDisplayVersion()
            => assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        public static Icon GetApplicationIcon()
            => Icon.ExtractAssociatedIcon(Environment.ProcessPath);

        public static Uri GetProjectUri()
            => new(assembly.GetCustomAttribute<AssemblyMetadataAttribute>().Value);

        public static Stream GetResourceStream(string resourceName)
            => assembly.GetManifestResourceStream(resourceName);
    }
}
