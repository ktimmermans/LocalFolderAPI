using BareApiHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.Models;
using System;

namespace FolderPollerWPF
{
    public class WebHostStartUp : BareStartup
    {
        public WebHostStartUp(IConfiguration configuration) : base(configuration)
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
    }
}
