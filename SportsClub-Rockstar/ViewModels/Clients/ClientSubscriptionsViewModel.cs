using Rockstar.Admin.WPF.Models;
using Rockstar.Admin.WPF.Services.Interfaces;
using Rockstar.Admin.WPF.ViewModels.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Rockstar.Admin.WPF.ViewModels.Clients
{
    public class ClientSubscriptionsViewModel : INotifyPropertyChanged
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly Client _client;
        private readonly Action _onClose;

        private ObservableCollection<SubscriptionPurchaseDto> _subscriptions = new();
        private ObservableCollection<Subscription> _availableSubscriptions = new();
        private Subscription? _selectedSubscription;
        private DateTime? _expiryDate;
        private bool _isLoading;

        public ClientSubscriptionsViewModel(ISubscriptionService subscriptionService, Client client, Action onClose)
        {
            _subscriptionService = subscriptionService;
            _client = client;
            _onClose = onClose;

            _ = LoadDataAsync();
        }

        public ObservableCollection<SubscriptionPurchaseDto> Subscriptions
        {
            get => _subscriptions;
            set { _subscriptions = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Subscription> AvailableSubscriptions
        {
            get => _availableSubscriptions;
            set { _availableSubscriptions = value; OnPropertyChanged(); }
        }

        public Subscription? SelectedSubscription
        {
            get => _selectedSubscription;
            set { _selectedSubscription = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanPurchase)); }
        }

        public DateTime? ExpiryDate
        {
            get => _expiryDate;
            set { _expiryDate = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); OnPropertyChanged(nameof(LoadingVisibility)); }
        }

        public Visibility LoadingVisibility => IsLoading ? Visibility.Visible : Visibility.Collapsed;
        public bool CanPurchase => SelectedSubscription != null;
        public string ClientName => $"{_client.FirstName} {_client.LastName}";

        public ICommand PurchaseCommand => new AsyncRelayCommand(PurchaseSubscription, () => CanPurchase && !IsLoading);
        public ICommand UseSessionCommand => new AsyncRelayCommand<int>(UseSession);
        public ICommand AddSessionCommand => new AsyncRelayCommand<int>(AddSession);
        public ICommand CloseCommand => new RelayCommand(() => _onClose?.Invoke());

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                var subscriptions = await _subscriptionService.GetUserSubscriptionsAsync(_client.Id);
                Subscriptions = new ObservableCollection<SubscriptionPurchaseDto>(subscriptions);

                var allSubscriptions = await _subscriptionService.GetAllAsync();
                AvailableSubscriptions = new ObservableCollection<Subscription>(allSubscriptions);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadDataAsync error: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task PurchaseSubscription()
        {
            if (SelectedSubscription == null) return;

            IsLoading = true;
            try
            {
                var success = await _subscriptionService.PurchaseSubscriptionAsync(_client.Id, SelectedSubscription.Id, ExpiryDate);

                if (success)
                {
                    MessageBox.Show("Абонемент успешно добавлен клиенту!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDataAsync();
                    SelectedSubscription = null;
                    ExpiryDate = null;
                }
                else
                {
                    MessageBox.Show("Ошибка при добавлении абонемента", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UseSession(int purchaseId)
        {
            var result = MessageBox.Show("Списать одно занятие по этому абонементу?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            IsLoading = true;
            try
            {
                var success = await _subscriptionService.UseSessionAsync(purchaseId);
                if (success)
                {
                    await LoadDataAsync();
                    MessageBox.Show("Занятие списано!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Ошибка при списании занятия", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AddSession(int purchaseId)
        {
            var result = MessageBox.Show("Добавить одно занятие к абонементу?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            IsLoading = true;
            try
            {
                var success = await _subscriptionService.AddSessionAsync(purchaseId);
                if (success)
                {
                    await LoadDataAsync();
                    MessageBox.Show("Занятие добавлено!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Ошибка при добавлении занятия", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}