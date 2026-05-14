using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rockstar.Admin.WPF.Models;
using Rockstar.Admin.WPF.Services;
using Rockstar.Admin.WPF.Services.Interfaces;
using Rockstar.Admin.WPF.ViewModels.Auth;
using Rockstar.Admin.WPF.ViewModels.Clients;
using Rockstar.Admin.WPF.ViewModels.Directions;
using Rockstar.Admin.WPF.ViewModels.Main;
using Rockstar.Admin.WPF.ViewModels.Schedule;
using Rockstar.Admin.WPF.ViewModels.Subscriptions;
using Rockstar.Admin.WPF.ViewModels.Trainers;
using Rockstar.Admin.WPF.Views.Auth;
using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace Rockstar.Admin.WPF
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;
        public static IConfiguration Configuration { get; private set; } = null!;

        private NavigationService _navigationService = new();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                Configuration = builder.Build();

                var services = new ServiceCollection();
                ConfigureServices(services);
                Services = services.BuildServiceProvider();

                _navigationService = Services.GetRequiredService<NavigationService>();

                var mainWindow = new MainWindow();
                mainWindow.Show();

                var loginPage = new LoginPage(Services, page => _navigationService.NavigateTo(page));
                _navigationService.NavigateTo(loginPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\n\n{ex.StackTrace}", "Критическая ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<NavigationService>();
            services.AddLogging(b => { b.AddConsole(); b.AddDebug(); b.SetMinimumLevel(LogLevel.Debug); });
            services.AddHttpClient();

            services.AddSingleton<IApiService>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var baseUrl = config["ApiSettings:BaseUrl"] ?? "http://localhost:5143/api/";
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
                httpClient.BaseAddress = new Uri(baseUrl);
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                return new ApiService(httpClient);
            });

            services.AddSingleton<IAuthService, ApiAuthService>();
            services.AddSingleton<IClientService, ApiClientService>();
            services.AddSingleton<ITrainerService, ApiTrainerService>();
            services.AddSingleton<IScheduleService, ApiScheduleService>();
            services.AddSingleton<ISubscriptionService, ApiSubscriptionService>();
            services.AddSingleton<IDirectionService, ApiDirectionService>();
            services.AddSingleton<IServiceService, ApiServiceService>();
            services.AddSingleton<IDirectionService, ApiDirectionService>();
            services.AddSingleton<IServiceTypeService, ApiServiceTypeService>();

            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<ClientsViewModel>();
            services.AddTransient<TrainersViewModel>();
            services.AddTransient<ScheduleViewModel>();
            services.AddTransient<SubscriptionsViewModel>();
            services.AddTransient<DirectionsViewModel>();
        }
    }

    public class NavigationService
    {
        public event EventHandler<Page>? NavigateRequested;
        public void NavigateTo(Page page) => NavigateRequested?.Invoke(this, page);
    }
}