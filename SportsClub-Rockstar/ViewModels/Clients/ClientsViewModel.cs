using Rockstar.Admin.WPF.Models;
using Rockstar.Admin.WPF.Services.Interfaces;
using Rockstar.Admin.WPF.ViewModels.Base;
using Rockstar.Admin.WPF.ViewModels.Commands;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Rockstar.Admin.WPF.ViewModels.Clients
{
    public class ClientsViewModel : ViewModelBase
    {
        private readonly IClientService _clientService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly Action<Page> _navigate;

        private ObservableCollection<Client> _clients = new();
        private string _searchText = string.Empty;
        private bool _isLoading = true;
        private Client? _selectedClient;

        public ClientsViewModel(IClientService clientService, ISubscriptionService subscriptionService, Action<Page> navigate)
        {
            Debug.WriteLine("=== ClientsViewModel Constructor START ===");

            _clientService = clientService;
            _subscriptionService = subscriptionService;
            _navigate = navigate;

            AddClientCommand = new RelayCommand(ExecuteAddClient);
            EditClientCommand = new RelayCommand<Client>(ExecuteEditClient);
            DeleteClientCommand = new AsyncRelayCommand<Client>(ExecuteDeleteClient);
            OpenSubscriptionsCommand = new RelayCommand<Client>(ExecuteOpenSubscriptions);
            RefreshCommand = new RelayCommand(() => _ = LoadClientsAsync());
            BackCommand = new RelayCommand(ExecuteBack);

            Debug.WriteLine("ClientsViewModel: Commands created");

            _ = LoadClientsAsync();

            Debug.WriteLine("=== ClientsViewModel Constructor END ===");
        }

        public ObservableCollection<Client> Clients
        {
            get => _clients;
            private set
            {
                if (SetField(ref _clients, value))
                {
                    OnPropertyChanged(nameof(FilteredClients));
                }
            }
        }

        public ObservableCollection<Client> FilteredClients
        {
            get
            {
                if (Clients == null || Clients.Count == 0)
                    return new ObservableCollection<Client>();

                if (string.IsNullOrWhiteSpace(SearchText))
                    return Clients;

                // Поиск только по телефону и email (ФИО убран)
                var filtered = Clients.Where(c =>
                    (c.Phone != null && c.Phone.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    c.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                ).ToList();

                return new ObservableCollection<Client>(filtered);
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetField(ref _searchText, value))
                {
                    OnPropertyChanged(nameof(FilteredClients));
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (SetField(ref _isLoading, value))
                {
                    OnPropertyChanged(nameof(LoadingVisibility));
                    OnPropertyChanged(nameof(ContentVisibility));
                }
            }
        }

        public Client? SelectedClient
        {
            get => _selectedClient;
            set => SetField(ref _selectedClient, value);
        }

        public Visibility LoadingVisibility => IsLoading ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ContentVisibility => IsLoading ? Visibility.Collapsed : Visibility.Visible;

        // Команды
        public ICommand AddClientCommand { get; }
        public ICommand EditClientCommand { get; }
        public ICommand DeleteClientCommand { get; }
        public ICommand OpenSubscriptionsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand BackCommand { get; }

        private async Task LoadClientsAsync()
        {
            try
            {
                Debug.WriteLine("=== LoadClientsAsync started ===");
                IsLoading = true;

                var clients = await _clientService.GetAllAsync();

                Debug.WriteLine($"Loaded {clients.Count} clients from service");

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Clients.Clear();
                    foreach (var client in clients.OrderBy(c => c.LastName).ThenBy(c => c.FirstName))
                    {
                        Clients.Add(client);
                    }
                    OnPropertyChanged(nameof(FilteredClients));
                    IsLoading = false;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadClientsAsync error: {ex.Message}");
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки клиентов: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    IsLoading = false;
                });
            }
        }

        private void ExecuteAddClient()
        {
            try
            {
                Debug.WriteLine("ExecuteAddClient: Navigating to AddClientView");
                _navigate(new Views.Clients.AddClientView(_navigate, null));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ExecuteAddClient error: {ex.Message}");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteEditClient(Client? client)
        {
            if (client == null) return;

            try
            {
                Debug.WriteLine($"ExecuteEditClient: Editing client {client.Id} - {client.FullName}");
                _navigate(new Views.Clients.AddClientView(_navigate, client));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ExecuteEditClient error: {ex.Message}");
            }
        }

        private void ExecuteOpenSubscriptions(Client? client)
        {
            if (client == null) return;

            try
            {
                Debug.WriteLine($"ExecuteOpenSubscriptions: Opening subscriptions for client {client.Id} - {client.FullName}");
                var subscriptionsView = new Views.Clients.ClientSubscriptionsView(_subscriptionService, client, () => { });
                _navigate(subscriptionsView);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ExecuteOpenSubscriptions error: {ex.Message}");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExecuteDeleteClient(Client? client)
        {
            if (client == null) return;

            Debug.WriteLine($"ExecuteDeleteClient: Attempting to delete client {client.Id} - {client.FullName}");

            var result = MessageBox.Show(
                $"Удалить клиента '{client.FullName}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var success = await _clientService.DeleteAsync(client.Id);

                    if (success)
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            Clients.Remove(client);
                            OnPropertyChanged(nameof(FilteredClients));
                            MessageBox.Show("Клиент удален", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        });
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить клиента", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Delete error: {ex.Message}");
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
            }
        }

        private void ExecuteBack()
        {
            Debug.WriteLine("ExecuteBack: Navigating to MainPage");
            _navigate(new Views.Main.MainPage(_navigate));
        }
    }
}