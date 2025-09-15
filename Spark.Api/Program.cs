using System;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Spark.Domain.Api;

namespace Spark.Api // garanta que é o Program do Spark.Api (o que vira Spark.Api.dll)
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // Log pra confirmar em runtime
            var addrs = host.Services.GetService(typeof(IServerAddressesFeature)) as IServerAddressesFeature;
            Console.WriteLine($">>> PORT={Environment.GetEnvironmentVariable("PORT")}");
            Console.WriteLine(">>> ADDRESSES: " + string.Join(", ", addrs?.Addresses ?? Array.Empty<string>()));

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, cfg) =>
                {
                    var env = ctx.HostingEnvironment;
                    cfg.Sources.Clear();
                    cfg.SetBasePath(env.ContentRootPath)
                       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                       .AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(k =>
                    {
                        var port = int.TryParse(Environment.GetEnvironmentVariable("PORT"), out var p) ? p : 8080;
                        k.Listen(IPAddress.Any, port); // <= 0.0.0.0:<PORT>
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
