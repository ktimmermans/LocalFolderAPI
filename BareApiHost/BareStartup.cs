using BackgroundWorker;
using BareApiHost.ConfigurationExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Services;
using Services.FolderConfiguration;
using Services.Interfaces;
using System.Reflection;

namespace BareApiHost
{
    public class BareStartup
    {
        public BareStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // ------------------------------------------------------------------------
            // Try to configure the database (method should be overridden by the startup class in the UI project)
            // ------------------------------------------------------------------------
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

            // ------------------------------------------------------------------------
            // Add the default FolderPoller controllers from the BareApiHost project to be used in every UI project that extends the BareApiHostProject
            // ------------------------------------------------------------------------
            services.AddControllers()
                .AddApplicationPart(Assembly.GetAssembly(typeof(Controllers.FolderController)))
                .AddApplicationPart(Assembly.GetAssembly(typeof(Controllers.QueueController)))
                .AddControllersAsServices();

            services.AddCors();

            // ------------------------------------------------------------------------
            // Try to add custom UI services (method should be overridden by the startup class in the UI project)
            // ------------------------------------------------------------------------
            this.AddUI(services);

            // ------------------------------------------------------------------------
            // SET SWAGGER CONFIGURATION
            // ------------------------------------------------------------------------
            services.AddSwagger();

            // ------------------------------------------------------------------------
            // SET REQUEST CONFIGURATION
            // ------------------------------------------------------------------------
            services.ConfigureRequests();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            app.UseStaticFiles();

            app.UseCors(builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseRouting();

            // ------------------------------------------------------------------------
            // Add swagger
            // ------------------------------------------------------------------------
            app.UseSwagger(provider);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // ------------------------------------------------------------------------
            // Try to add custom UI (method should be overridden by the startup class in the UI project)
            // ------------------------------------------------------------------------
            this.UseUI(app, env);
        }

        /// <summary>
        /// Overridable method to configure the database to use
        /// </summary>
        /// <param name="services"></param>
        public virtual void ConfigureDatabase(IServiceCollection services)
        {

        }

        /// <summary>
        /// Overridable method to add required middleware for UI purposes
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="env"></param>
        public virtual void UseUI(IApplicationBuilder builder, IWebHostEnvironment env)
        {

        }

        /// <summary>
        /// Overridable method to add services for UI purposes
        /// </summary>
        /// <param name="services"></param>
        public virtual void AddUI(IServiceCollection services)
        {

        }
    }
}
