using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace TcpSocketServerBackgroundService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (IHost host = AppHelper.GetHostBuilderWithDependenciesInjected(args).Build())
            {
                IHostApplicationLifetime hostApplicationLifetime =
                    host.Services.GetRequiredService<IHostApplicationLifetime>();

                await host.RunAsync(token: hostApplicationLifetime.ApplicationStopping);
            }
        }
    }
}