using Microsoft.Extensions.DependencyInjection;
using Rockstar.Admin.WPF.Services.Interfaces;
using Rockstar.Admin.WPF.ViewModels.Clients;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Rockstar.Admin.WPF.Views.Clients
{
    public partial class ClientsView : Page
    {
        private readonly Action<Page> _navigate;
        private readonly ClientsViewModel _viewModel;

        public ClientsView(Action<Page> navigate)
        {
            try
            {
                Debug.WriteLine("=== ClientsView Constructor START ===");

                InitializeComponent();
                _navigate = navigate;

                Debug.WriteLine("ClientsView: Getting services from App.Services");

                var clientService = App.Services.GetRequiredService<IClientService>();
                var subscriptionService = App.Services.GetRequiredService<ISubscriptionService>();  // 👈 ДОБАВИТЬ
                var apiService = App.Services.GetRequiredService<IApiService>();

                // Проверяем токен
                var token = apiService.GetAuthToken();
                Debug.WriteLine($"ClientsView: Token in ApiService: {(string.IsNullOrEmpty(token) ? "NULL" : "PRESENT")}");

                Debug.WriteLine("ClientsView: Creating ViewModel");
                // 👇 ИСПРАВЛЕНО: передаём subscriptionService и navigate
                _viewModel = new ClientsViewModel(clientService, subscriptionService, navigate);

                Debug.WriteLine("ClientsView: Setting DataContext");
                DataContext = _viewModel;

                Debug.WriteLine("=== ClientsView Constructor END ===");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ClientsView constructor error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Ошибка инициализации страницы: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            _navigate(new Views.Main.MainPage(_navigate));
        }
    }
}