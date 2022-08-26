using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;

namespace TcpSocketServerBackgroundService
{
    public static class SerilogHelper
    {
        private static readonly string loggerOutputTemplate = "[App: {App} Version: {Version}] [{Level: u3}] [{Timestamp:dd-MM-yyyy HH:mm:ss.fff zzz}] Msg: {Message:lj}{NewLine}{Exception}";
        private static readonly string serilogOutputFileDirectory = "./_Logs/";
        private static readonly string assemblyName = AssemblyHelper.AssemblyName;
        private static readonly string assemblyVersion = AssemblyHelper.AssemblyVersion;
        private static readonly string serilogFileNameDatetimeExtension = $"{DateTime.Now:yyyyMMddHHmmss}";
        private static readonly string serilogFilePath = string.Format("{0}{1}_{2}.txt", serilogOutputFileDirectory, assemblyName, serilogFileNameDatetimeExtension);

        public static LoggerConfiguration SetupSerilog(IConfiguration cfg)
        {
            // Enrichments: https://github.com/serilog/serilog/wiki/Enrichment
            // Writing Log Events: https://github.com/serilog/serilog/wiki/Writing-Log-Events
            var loggerCfg
                = new LoggerConfiguration()
                    .Enrich.WithProperty("App", assemblyName)
                    .Enrich.WithProperty("Version", assemblyVersion)
                    .WriteTo
                        .File(
                            path: serilogFilePath,
                            outputTemplate: loggerOutputTemplate,
                            restrictedToMinimumLevel: LogEventLevel.Verbose,
                            rollOnFileSizeLimit: true,
                            fileSizeLimitBytes: 1000 * 1024)
                    .WriteTo
                        .Console(
                            outputTemplate: loggerOutputTemplate,
                            theme: AnsiConsoleTheme.Literate)
                    // Serilog arguments in the appsettings.json can override the above settings
                    .ReadFrom.Configuration(cfg, "Serilog");

            return loggerCfg;
        }

    }
}