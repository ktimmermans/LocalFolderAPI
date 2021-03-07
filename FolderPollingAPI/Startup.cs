using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services;
using Services.Interfaces;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Http.Features;
using BackgroundWorker;
using Services.Models;
using Microsoft.EntityFrameworkCore;
using Services.FolderConfiguration;

namespace PSFolderPlugin
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            this.ConfigureDatabase(services);

            services.AddTransient<IFolderConfigurationProvider, IniConfigurationProvider>();
            services.AddScoped<IFolderConfigService, FolderConfigService>();
            services.AddScoped<IFolderService, FolderService>();
            services.AddScoped<IFolderManagerService, FolderManagerService>();
            services.AddTransient<IFolderPollingManagerService, FolderPollingManagerService>();
            services.AddScoped<IWebhookService, WebhookService>();

            services.AddHttpClient();

            // Add methods to process timedtask
            services.AddTaskManager();
            services.AddHostedService<TimedFolderPollingService>();
            services.AddBackgroundQueue<PollingTaskDescriptor>();


            services.AddControllersWithViews();
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "PSFolderPlugin", Version = "v3" });
            });

            // Set maximum request size
            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue; // if don't set default value is: 128 MB
                x.MultipartHeadersLengthLimit = int.MaxValue;
            });

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = int.MaxValue; // if don't set default value is: 30 MB
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            // Use swagger UI so that everyone can see
            if (!env.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(x =>
                {
                    x.SwaggerEndpoint("/swagger/v1/swagger.json", "PSFolderPlugin API V1");
                });
            }

            app.UseSpa(spa =>
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

        public virtual void ConfigureDatabase(IServiceCollection services)
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
