using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace KaiheilaInspector
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();
            _ = host.Services.GetService<BotHost>();
            host.Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<ConfigHost>();
                    services.AddSingleton<BotHost>();
                })
                .ConfigureLogging(builder => builder
                    .AddDebug()
                    .AddSimpleConsole(options =>
                    {
                        options.ColorBehavior = LoggerColorBehavior.Enabled;
                        options.SingleLine = true;
                        options.IncludeScopes = true;
                    })
                    .AddSystemdConsole());
    }
}
