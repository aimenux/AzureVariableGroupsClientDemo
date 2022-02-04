using App.Extensions;
using Bullseye;
using Lib;
using Lib.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using AppHost = Microsoft.Extensions.Hosting.Host;

namespace App;

public static class Program
{
    public static async Task Main(string[] args)
    {
        using (var host = CreateHostBuilder(args).Build())
        {
            var targets = new Targets();
            var client = host.Services.GetRequiredService<IAzureDevopsClient>();

            targets.Add(TargetTypes.Default, dependsOn: new List<string>
            {
                TargetTypes.GetVariableGroupsBySdk,
                TargetTypes.GetVariableGroupsByRest,
            });

            targets.Add(TargetTypes.GetVariableGroupsBySdk, async () =>
            {
                var results = await client.GetAzureVariableGroupsAsync(AzureDevopsChoice.Sdk);
                var resultsDump = ObjectDumper.Dump(results);
                Console.WriteLine(resultsDump);
            });

            targets.Add(TargetTypes.GetVariableGroupsByRest, async () =>
            {
                var results = await client.GetAzureVariableGroupsAsync(AzureDevopsChoice.Rest);
                var resultsDump = ObjectDumper.Dump(results);
                Console.WriteLine(resultsDump);
            });

            await targets.RunAndExitAsync(args);
        }

        Console.WriteLine("Press any key to exit !");
        Console.ReadKey();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        AppHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddJsonFile();
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            })
            .ConfigureLogging((_, loggingBuilder) =>
            {
                loggingBuilder.AddNonGenericLogger();
            })
            .ConfigureServices((hostingContext, services) =>
            {
                services.AddHttpClient<IAzureDevopsClient, AzureDevopsClient>();
                services.AddTransient<TaskAgentHttpClient>(serviceProvider =>
                {
                    var settings = serviceProvider.GetRequiredService<IOptions<Settings>>().Value;
                    ObjectDumper.Dump(settings);
                    var credentials = new VssBasicCredential(string.Empty, settings.PersonalAccessToken);
                    var connection = new VssConnection(new Uri($"{settings.AzureDevopsUrl}/{settings.OrganizationName}"), credentials);
                    return connection.GetClient<TaskAgentHttpClient>();
                });
                services.Configure<Settings>(hostingContext.Configuration.GetSection(nameof(Settings)));
            })
            .UseConsoleLifetime();

    private static void AddNonGenericLogger(this ILoggingBuilder loggingBuilder)
    {
        var categoryName = typeof(Program).Namespace!;
        var services = loggingBuilder.Services;
        services.AddSingleton(serviceProvider =>
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            return loggerFactory.CreateLogger(categoryName);
        });
    }

    private static class TargetTypes
    {
        public const string Default = "Default";
        public const string GetVariableGroupsBySdk = "GetVariableGroupsBySdk";
        public const string GetVariableGroupsByRest = "GetVariableGroupsByRest";
    }
}