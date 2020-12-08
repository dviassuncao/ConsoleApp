using ConsoleApp.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Settings.Configuration;
using System;
using System.Configuration;
using Microsoft.Extensions.Http;
using ConsoleApp.Api;
using System.IO;

namespace ConsoleApp
{
    class Program
    {
        static IConfigurationRoot _configurationRoot;

        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
            _configurationRoot = config.Build();
        })
        .ConfigureServices((hostContext, services) =>
        {
            services.Configure<CustomHostedServiceOptions>(options =>
            {
                _configurationRoot.GetSection("CustomHostedServiceOptions").Bind(options);
            });
            services.AddLogging(logging =>
            {
                Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(_configurationRoot).CreateLogger();
                logging.AddSerilog();
            });
            services.AddHostedService<CustomHostedService>();
            services.AddHttpClient<IApiClient, ApiClient>();
        });
    }
}
