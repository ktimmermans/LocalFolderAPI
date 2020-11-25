using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PSFolderPlugin
{
    public class Program
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
                .UseStartup<Startup>();
    }
}
