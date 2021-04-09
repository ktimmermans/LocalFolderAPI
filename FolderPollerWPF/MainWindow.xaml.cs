using MahApps.Metro.Controls;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Services.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace FolderPollerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private IHost _host;

        private List<FolderConfig> Folders { get; set; }
        public MainWindow()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: SystemConsoleTheme.Colored)
                .WriteTo.File(
                @$"{Directory.GetCurrentDirectory()}/log/FolderPoller.log",
                fileSizeLimitBytes: 100_000,
                rollOnFileSizeLimit: true,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1))
                .CreateLogger();

            this.Folders = new List<FolderConfig>();
            for (int i = 1; i < 8; i++)
            {
                this.Folders.Add(new FolderConfig
                {
                    FolderName = $"test {i}",
                    PollingType = i % 2 == 0 ? "Polling" : ""
                });
            }

            InitializeComponent();
            this.FolderScroller.ItemsSource = this.Folders;

            this._host = CreateHostBuilder().Build();

            Task.Run(() =>
            {
                try
                {
                    Log.Information($"Started loggin to: {Directory.GetCurrentDirectory()}/log/FolderPoller.log");

                    this._host.Run();
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Host terminated unexpectedly");
                }
                finally
                {
                    Log.CloseAndFlush();
                }
            });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _host.Dispose();
            base.OnClosing(e);
        }

        private void LaunchGitHubSite(object sender, RoutedEventArgs e)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "https://github.com/ktimmermans/LocalFolderAPI",
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://+:5000")
                    .UseStartup<WebHostStartUp>();
                });
    }
}
