using Microsoft.Extensions.DependencyInjection;
using Rockstar.Admin.WPF.Services.Interfaces;
using Rockstar.Admin.WPF.Views.Auth;
using Rockstar.Admin.WPF.Views.Clients;
using Rockstar.Admin.WPF.Views.Directions;
using Rockstar.Admin.WPF.Views.Schedule;
using Rockstar.Admin.WPF.Views.Subscriptions;
using Rockstar.Admin.WPF.Views.Trainers;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Rockstar.Admin.WPF.Views.Main
{
    public partial class MainPage : Page
    {
        private readonly Action<Page> _navigate;

        public MainPage(Action<Page> navigate)
        {
            try
            {
                Debug.WriteLine("MainPage: Initializing");

                InitializeComponent();
                _navigate = navigate;

                // Получаем IApiService через App.Services
                var apiService = App.Services.GetRequiredService<IApiService>();
                var token = apiService.GetAuthToken();
                Debug.WriteLine($"MainPage: Token in ApiService: {(string.IsNullOrEmpty(token) ? "NULL" : "PRESENT")}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage constructor error: {ex.Message}");
                throw;
            }
        }

        private void ClientsButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Navigating to ClientsView");
            _navigate(new ClientsView(_navigate));
        }

        private void SubscriptionsButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Navigating to SubscriptionsView");
            _navigate(new SubscriptionsView(_navigate));
        }

        private void TrainersButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Navigating to TrainersView");
            _navigate(new TrainersView(_navigate));
        }

        private void DirectionsButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Navigating to DirectionsView");
            _navigate(new DirectionsView(_navigate));
        }

        private void ScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Navigating to ScheduleView");
            _navigate(new ScheduleView(_navigate));
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы действительно хотите выйти из системы?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Debug.WriteLine("Logging out...");

                // Очищаем токен
                var apiService = App.Services.GetRequiredService<IApiService>();
                apiService.SetAuthToken(null);

                var authService = App.Services.GetRequiredService<IAuthService>();
                await authService.LogoutAsync();

                _navigate(new LoginPage(App.Services, _navigate));
            }
        }
    }
}