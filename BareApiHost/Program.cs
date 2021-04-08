using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BareApiHost
{
    public class BareApiHost
    {
        public static void Main(string[] args)
        {
            var webhostBuilder = CreateWebHostBuilder(args).Build();

            webhostBuilder.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .ConfigureLogging((context, logging) =>
            {
                var env = context.HostingEnvironment;
                var config = context.Configuration.GetSection("Logging");
                logging.AddConsole();
                logging.AddConfiguration(config);
            })
            .UseUrls("http://+:5000")
                .UseStartup<BareStartup>();
    }
}
