using Rockstar.Admin.WPF.Models;
using Rockstar.Admin.WPF.Services.Interfaces;
using Rockstar.Admin.WPF.ViewModels.Base;
using Rockstar.Admin.WPF.ViewModels.Commands;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Rockstar.Admin.WPF.ViewModels.Directions
{
    public class DirectionsViewModel : ViewModelBase
    {
        private readonly IDirectionService _directionService;
        private readonly Action<Page> _navigate;
        private ObservableCollection<Direction> _directions = new();
        private bool _isLoading = true;

        public DirectionsViewModel(IDirectionService directionService, Action<Page> navigate)
        {
            _directionService = directionService;
            _navigate = navigate;

            // Создаем команды
            // ✅ ИСПРАВЛЕНО: используем латинские ключи (yoga, fitness, climbing)
            NavigateToYogaCommand = new RelayCommand(() => ExecuteNavigateToDirection("yoga"));
            NavigateToFitnessCommand = new RelayCommand(() => ExecuteNavigateToDirection("fitness"));
            NavigateToClimbingCommand = new RelayCommand(() => ExecuteNavigateToDirection("climbing"));
            BackCommand = new RelayCommand(ExecuteBack);

            // Запускаем асинхронную загрузку
            _ = LoadDataAsync();
        }

        public ObservableCollection<Direction> Directions
        {
            get => _directions;
            private set => SetField(ref _directions, value);
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

        public Visibility LoadingVisibility => IsLoading ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ContentVisibility => IsLoading ? Visibility.Collapsed : Visibility.Visible;

        // Команды
        public ICommand NavigateToYogaCommand { get; }
        public ICommand NavigateToFitnessCommand { get; }
        public ICommand NavigateToClimbingCommand { get; }
        public ICommand BackCommand { get; }

        private async Task LoadDataAsync()
        {
            try
            {
                Debug.WriteLine("=== LoadDataAsync started ===");
                var directions = await _directionService.GetAllAsync();
                Debug.WriteLine($"Loaded {directions.Count} directions");

                // Обновляем UI в правильном потоке
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Directions = new ObservableCollection<Direction>(directions);
                    IsLoading = false;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadDataAsync error: {ex.Message}");
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки направлений: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    IsLoading = false;
                });
            }
        }

        private void ExecuteNavigateToDirection(string directionKey)
        {
            Debug.WriteLine($"Navigating to direction: {directionKey}");
            _navigate(new Views.Directions.DirectionDetailView(_navigate, directionKey));
        }

        private void ExecuteBack()
        {
            _navigate(new Views.Main.MainPage(_navigate));
        }
    }
}