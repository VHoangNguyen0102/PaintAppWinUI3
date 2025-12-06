using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PaintApp.Data;
using PaintApp.Services;

namespace PaintApp
{
    public partial class App : Application
    {
        private Window? _window;

        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        public App()
        {
            InitializeComponent();
            
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
            
            InitializeDatabase();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "paintapp.db");
            
            // Database Context
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite($"Data Source={dbPath}");
            });
            
            // Services
            services.AddScoped<IProfileService, ProfileService>();
            
            // ViewModels
            services.AddTransient<ViewModels.HomePageViewModel>();
            services.AddTransient<ViewModels.ManagePageViewModel>();
            services.AddTransient<ViewModels.DrawPageViewModel>();
            
            // Views/Windows
            services.AddTransient<MainWindow>();
        }

        private void InitializeDatabase()
        {
            try
            {
                using var scope = ServiceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                dbContext.Database.EnsureCreated();
            }
            catch (Exception)
            {
            }
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = ServiceProvider.GetRequiredService<MainWindow>();
            _window.Activate();
        }
    }
}
