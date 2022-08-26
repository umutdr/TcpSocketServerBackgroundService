using System.Reflection;

namespace TcpSocketServerBackgroundService
{
    public static class AssemblyHelper
    {
        public static string AssemblyName { get; set; } = Assembly.GetEntryAssembly()?.GetName().Name;

        public static string AssemblyVersion { get; set; } = Assembly.GetEntryAssembly()?.GetName().Version.ToString();
    }
}
