using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.IO;

namespace TcpSocketServerBackgroundService
{
    public static class AppHelper
    {
        private static IHostBuilder _hostBuilder;
        private static IConfiguration _configuration;

        public static IHostBuilder GetHostBuilderWithDependenciesInjected(string[] args)
        {
            if (_hostBuilder == null)
            { _hostBuilder = Host.CreateDefaultBuilder(args); }

            Log.Logger =
                SerilogHelper
                    .SetupSerilog(GetConfiguration())
                    .CreateLogger();

            _hostBuilder
                .UseSerilog()
                .AddRequiredDependencies();

            return _hostBuilder;
        }

        private static IConfiguration GetConfiguration()
        {
            if (_configuration == null)
            {
                _configuration
                    = new ConfigurationBuilder()
                        .AddEnvironmentVariables()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .Build();
            }

            return _configuration;
        }

        private static IHostBuilder AddRequiredDependencies(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                    .ConfigureServices(
                        (serviceCollection) =>
                            serviceCollection
                                .AddOptionsDependencies()
                                .AddHostedService<TcpSocketServerBackgroundService>()
                                //.AddSingleton<ITheService,TheService>()
                                );
        }

        private static IServiceCollection AddOptionsDependencies(this IServiceCollection serviceCollection)
        {
            MeAppSettingsModel meMqAppSettingsModel = GetConfiguration().GetSection("Me").Get<MeAppSettingsModel>();

            serviceCollection
                .AddOptions()
                .AddSingleton(meMqAppSettingsModel);

            return serviceCollection;
        }
    }
}