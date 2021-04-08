using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services.Models;
using Microsoft.EntityFrameworkCore;
using BareApiHost;

namespace PSFolderPlugin
{
    public class Startup : BareStartup
    {
        public Startup(IConfiguration configuration) : base(configuration)
        { }

        public override void ConfigureDatabase(IServiceCollection services)
        {
            var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

            services.AddDbContext<ProjectContext>(
                options =>
                {
                    options.UseInMemoryDatabase("FolderConfig");
                    options.UseInternalServiceProvider(serviceProvider);
                }, ServiceLifetime.Transient);
        }

        public override void UseUI(IApplicationBuilder builder, IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                builder.UseSpaStaticFiles();
            }

            builder.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }

        public override void AddUI(IServiceCollection services)
        {
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }
    }
}
