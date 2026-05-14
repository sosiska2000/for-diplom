using Rockstar.Admin.WPF.Services.Interfaces;
using Rockstar.Admin.WPF.ViewModels.Base;
using Rockstar.Admin.WPF.ViewModels.Commands;
using Rockstar.Admin.WPF.Views.Auth;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Rockstar.Admin.WPF.ViewModels.Main
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly IApiService _apiService;
        private readonly Action<Page> _navigate;

        public MainViewModel(IAuthService authService, IApiService apiService, Action<Page> navigate)
        {
            _authService = authService;
            _apiService = apiService;
            _navigate = navigate;

            // 👇 Проверяем токен при создании MainViewModel
            var token = _apiService.GetAuthToken();
            Debug.WriteLine($"MainViewModel created. Token in ApiService: {(string.IsNullOrEmpty(token) ? "NULL" : "PRESENT")}");
        }

        public ICommand OpenClientsCommand => new RelayCommand(() =>
        {
            Debug.WriteLine("Opening ClientsView");
            _navigate(new Views.Clients.ClientsView(_navigate));
        });

        public ICommand OpenTrainersCommand => new RelayCommand(() =>
        {
            Debug.WriteLine("Opening TrainersView");
            _navigate(new Views.Trainers.TrainersView(_navigate));
        });

        public ICommand OpenSubscriptionsCommand => new RelayCommand(() =>
        {
            Debug.WriteLine("Opening SubscriptionsView");
            _navigate(new Views.Subscriptions.SubscriptionsView(_navigate));
        });

        public ICommand OpenDirectionsCommand => new RelayCommand(() =>
        {
            Debug.WriteLine("Opening DirectionsView");
            _navigate(new Views.Directions.DirectionsView(_navigate));
        });

        public ICommand OpenScheduleCommand => new RelayCommand(() =>
        {
            Debug.WriteLine("Opening ScheduleView");
            _navigate(new Views.Schedule.ScheduleView(_navigate));
        });

        public ICommand LogoutCommand => new RelayCommand(async () => await ExecuteLogout());

        private async Task ExecuteLogout()
        {
            Debug.WriteLine("Logging out...");

            // 👇 Очищаем токен при выходе
            _apiService.SetAuthToken(null);
            await _authService.LogoutAsync();

            _navigate(new LoginPage(App.Services, _navigate));
        }
    }
}