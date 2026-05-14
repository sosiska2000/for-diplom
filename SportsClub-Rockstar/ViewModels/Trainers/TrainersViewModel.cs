using Rockstar.Admin.WPF.Models;
using Rockstar.Admin.WPF.Services.Interfaces;
using Rockstar.Admin.WPF.ViewModels.Base;
using Rockstar.Admin.WPF.ViewModels.Commands;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Rockstar.Admin.WPF.ViewModels.Trainers
{
    public class TrainersViewModel : ViewModelBase
    {
        private readonly ITrainerService _trainerService;
        private readonly Action<Page> _navigate;

        private ObservableCollection<Trainer> _trainers = new();
        private ObservableCollection<Trainer> _allTrainers = new();
        private string _searchText = string.Empty;
        private bool _isLoading = true;

        // 👇 ФИЛЬТРЫ
        private bool _filterYoga;
        private bool _filterFitness;
        private bool _filterClimbing;

        public TrainersViewModel(ITrainerService trainerService, Action<Page> navigate)
        {
            _trainerService = trainerService;
            _navigate = navigate;

            AddTrainerCommand = new RelayCommand(ExecuteAddTrainer);
            EditTrainerCommand = new RelayCommand<Trainer>(ExecuteEditTrainer);
            DeleteTrainerCommand = new AsyncRelayCommand<Trainer>(ExecuteDeleteTrainer);
            RefreshCommand = new RelayCommand(() => _ = LoadTrainersAsync());
            BackCommand = new RelayCommand(ExecuteBack);
            ClearFiltersCommand = new RelayCommand(ClearFilters);

            _ = LoadTrainersAsync();
        }

        public ObservableCollection<Trainer> Trainers
        {
            get => _trainers;
            private set => SetField(ref _trainers, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetField(ref _searchText, value))
                {
                    ApplyFilter();
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

        // 👇 ФИЛЬТРЫ
        public bool FilterYoga
        {
            get => _filterYoga;
            set
            {
                if (SetField(ref _filterYoga, value))
                {
                    ApplyFilter();
                }
            }
        }

        public bool FilterFitness
        {
            get => _filterFitness;
            set
            {
                if (SetField(ref _filterFitness, value))
                {
                    ApplyFilter();
                }
            }
        }

        public bool FilterClimbing
        {
            get => _filterClimbing;
            set
            {
                if (SetField(ref _filterClimbing, value))
                {
                    ApplyFilter();
                }
            }
        }

        public Visibility LoadingVisibility => IsLoading ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ContentVisibility => IsLoading ? Visibility.Collapsed : Visibility.Visible;
        public int AllTrainersCount => _allTrainers?.Count ?? 0;

        public ICommand AddTrainerCommand { get; }
        public ICommand EditTrainerCommand { get; }
        public ICommand DeleteTrainerCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        private async Task LoadTrainersAsync()
        {
            try
            {
                Debug.WriteLine("=== LoadTrainersAsync started ===");
                IsLoading = true;

                var trainers = await _trainerService.GetAllAsync();
                Debug.WriteLine($"Loaded {trainers.Count} trainers");

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _allTrainers = new ObservableCollection<Trainer>(trainers);
                    ApplyFilter();
                    IsLoading = false;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadTrainersAsync error: {ex.Message}");
                IsLoading = false;
            }
        }

        private void ApplyFilter()
        {
            if (_allTrainers == null || _allTrainers.Count == 0)
            {
                Trainers = new ObservableCollection<Trainer>();
                return;
            }

            Debug.WriteLine($"ApplyFilter: Yoga={_filterYoga}, Fitness={_filterFitness}, Climbing={_filterClimbing}, Search={_searchText}");

            var filtered = _allTrainers.AsEnumerable();

            // 👇 ФИЛЬТР ПО НАПРАВЛЕНИЯМ
            var selectedDirections = new System.Collections.Generic.List<string>();
            if (_filterYoga) selectedDirections.Add("yoga");
            if (_filterFitness) selectedDirections.Add("fitness");
            if (_filterClimbing) selectedDirections.Add("climbing");

            if (selectedDirections.Any())
            {
                filtered = filtered.Where(t =>
                    t.SelectedDirections != null &&
                    t.SelectedDirections.Any(d => selectedDirections.Contains(d.NameKey)));
            }

            // ПОИСК ПО ИМЕНИ ИЛИ ОПИСАНИЮ
            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                var searchLower = _searchText.ToLower();
                filtered = filtered.Where(t =>
                    t.FullName.ToLower().Contains(searchLower) ||
                    (t.Description?.ToLower().Contains(searchLower) ?? false));
            }

            Trainers = new ObservableCollection<Trainer>(filtered);
            Debug.WriteLine($"Filtered to {Trainers.Count} trainers");
        }

        private void ClearFilters()
        {
            FilterYoga = false;
            FilterFitness = false;
            FilterClimbing = false;
            SearchText = string.Empty;
        }

        private void ExecuteAddTrainer()
        {
            _navigate(new Views.Trainers.AddTrainerView(_navigate, null));
        }

        private void ExecuteEditTrainer(Trainer? trainer)
        {
            if (trainer == null) return;
            _navigate(new Views.Trainers.AddTrainerView(_navigate, trainer));
        }

        private async Task ExecuteDeleteTrainer(Trainer? trainer)
        {
            if (trainer == null) return;

            var result = MessageBox.Show(
                $"Удалить тренера {trainer.FullName}?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var success = await _trainerService.DeleteAsync(trainer.Id);
                if (success)
                {
                    _allTrainers.Remove(trainer);
                    ApplyFilter();
                    MessageBox.Show("Тренер удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void ExecuteBack()
        {
            _navigate(new Views.Main.MainPage(_navigate));
        }
    }
}