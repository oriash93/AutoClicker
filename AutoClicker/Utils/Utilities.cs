using System;
using System.Deployment.Application;
using System.Reflection;

namespace AutoClicker.Utils
{
    public static class Utilities
    {
        public static AssemblyName GetAssemblyInfo()
        {
            return Assembly.GetExecutingAssembly().GetName();
        }
    }
}
