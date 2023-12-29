using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Galytix.WebApi
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://*:9091");
                    webBuilder.UseKestrel();    
                    webBuilder.UseStartup<Startup>();
                });

    }
}
