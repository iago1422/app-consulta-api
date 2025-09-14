using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Spark.Domain.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;

                    // Zera fontes e reconstrói de forma tolerante
                    config.Sources.Clear();

                    config.SetBasePath(env.ContentRootPath)
                          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                          .AddEnvironmentVariables(); // Render usa ENV VARS
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    // Porta já é setada no ENTRYPOINT (--urls http://0.0.0.0:$PORT)
                });
    }
}
