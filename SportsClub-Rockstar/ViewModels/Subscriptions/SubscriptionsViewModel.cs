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

namespace Rockstar.Admin.WPF.ViewModels.Subscriptions
{
    public class SubscriptionsViewModel : ViewModelBase
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IDirectionService _directionService;
        private readonly Action<Page> _navigate;

        private ObservableCollection<Subscription> _subscriptions = new();
        private ObservableCollection<Subscription> _allSubscriptions = new();
        private ObservableCollection<Direction> _directions = new();
        private Direction? _selectedDirectionFilter;
        private string _searchText = string.Empty;
        private bool _isLoading = true;

        public SubscriptionsViewModel(
            ISubscriptionService subscriptionService,
            IDirectionService directionService,
            Action<Page> navigate)
        {
            _subscriptionService = subscriptionService;
            _directionService = directionService;
            _navigate = navigate;

            // Команды
            AddSubscriptionCommand = new RelayCommand(ExecuteAddSubscription);
            EditSubscriptionCommand = new RelayCommand<Subscription>(ExecuteEditSubscription);
            DeleteSubscriptionCommand = new AsyncRelayCommand<Subscription>(async s => await ExecuteDeleteSubscription(s));
            ClearFiltersCommand = new RelayCommand(ExecuteClearFilters);
            RefreshCommand = new RelayCommand(() => _ = LoadDataAsync());
            BackCommand = new RelayCommand(ExecuteBack);

            _ = LoadDataAsync();
        }

        public ObservableCollection<Subscription> Subscriptions
        {
            get => _subscriptions;
            private set => SetField(ref _subscriptions, value);
        }

        public ObservableCollection<Direction> Directions
        {
            get => _directions;
            private set => SetField(ref _directions, value);
        }

        public Direction? SelectedDirectionFilter
        {
            get => _selectedDirectionFilter;
            set
            {
                if (SetField(ref _selectedDirectionFilter, value))
                {
                    ApplyFilters();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetField(ref _searchText, value))
                {
                    ApplyFilters();
                }
            }
        }

        // 🔹 Состояние загрузки (как в ClientsViewModel)
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

        public Visibility LoadingVisibility => IsLoading ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ContentVisibility => IsLoading ? Visibility.Collapsed : Visibility.Visible;

        // 🔹 Счётчики (как в ClientsViewModel)
        public int AllSubscriptionsCount => _allSubscriptions?.Count ?? 0;
        public int FilteredSubscriptionsCount => _subscriptions?.Count ?? 0;

        // 🔹 Команды
        public ICommand AddSubscriptionCommand { get; }
        public ICommand EditSubscriptionCommand { get; }
        public ICommand DeleteSubscriptionCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand BackCommand { get; }

        // 🔹 Загрузка данных (с индикатором загрузки)
        private async Task LoadDataAsync()
        {
            try
            {
                Debug.WriteLine("=== LoadDataAsync started ===");
                IsLoading = true;

                var subscriptions = await _subscriptionService.GetAllAsync();
                Debug.WriteLine($"Loaded {subscriptions.Count} subscriptions");

                var directions = await _directionService.GetAllAsync();
                Debug.WriteLine($"Loaded {directions.Count} directions");

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _allSubscriptions = new ObservableCollection<Subscription>(subscriptions);
                    Directions = new ObservableCollection<Direction>(directions);
                    ApplyFilters();
                    OnPropertyChanged(nameof(AllSubscriptionsCount));
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadDataAsync error: {ex.Message}");
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
                Debug.WriteLine("=== LoadDataAsync finished ===");
            }
        }

        private void ApplyFilters()
        {
            try
            {
                var filtered = _allSubscriptions.AsEnumerable();

                // Фильтр по направлению
                if (_selectedDirectionFilter != null)
                {
                    filtered = filtered.Where(s => s.DirectionId == _selectedDirectionFilter.Id);
                }

                // Поиск по названию
                if (!string.IsNullOrWhiteSpace(_searchText))
                {
                    var searchLower = _searchText.ToLower();
                    filtered = filtered.Where(s =>
                        s.Name.ToLower().Contains(searchLower) ||
                        (s.Description?.ToLower().Contains(searchLower) ?? false));
                }

                Subscriptions = new ObservableCollection<Subscription>(filtered);
                Debug.WriteLine($"Filtered to {Subscriptions.Count} subscriptions");
                OnPropertyChanged(nameof(FilteredSubscriptionsCount));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ApplyFilters error: {ex.Message}");
            }
        }

        private void ExecuteAddSubscription()
        {
            _navigate(new Views.Subscriptions.AddSubscriptionView(_navigate, null));
        }

        private void ExecuteEditSubscription(Subscription subscription)
        {
            if (subscription != null)
            {
                Debug.WriteLine($"Editing subscription ID: {subscription.Id}");
                _navigate(new Views.Subscriptions.AddSubscriptionView(_navigate, subscription));
            }
        }

        private async Task ExecuteDeleteSubscription(Subscription subscription)
        {
            if (subscription == null) return;

            var result = MessageBox.Show(
                $"Удалить абонемент '{subscription.Name}'?\n\nЭто действие нельзя отменить.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Debug.WriteLine($"Deleting subscription ID: {subscription.Id}");
                    var success = await _subscriptionService.DeleteAsync(subscription.Id);

                    if (success)
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            Debug.WriteLine("Subscription deleted successfully");
                            _allSubscriptions.Remove(subscription);
                            ApplyFilters();
                            OnPropertyChanged(nameof(AllSubscriptionsCount));

                            MessageBox.Show("Абонемент успешно удален!", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        });
                    }
                    else
                    {
                        Debug.WriteLine("Failed to delete subscription");
                        MessageBox.Show("Не удалось удалить абонемент.", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Delete error: {ex.Message}");
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteClearFilters()
        {
            SelectedDirectionFilter = null;
            SearchText = string.Empty;
        }

        private void ExecuteBack()
        {
            _navigate(new Views.Main.MainPage(_navigate));
        }
    }
}