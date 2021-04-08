using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using PSFolderPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FolderPollerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IHost _host;
        public MainWindow()
        {
            InitializeComponent();

            _host = Host.CreateDefaultBuilder()
                           .ConfigureWebHostDefaults(webHostBuilder =>
                           {
                               webHostBuilder.UseStartup<WebHostStartUp>();
                           })
                           .Build();

            _host.Start();
        }
    }
}
