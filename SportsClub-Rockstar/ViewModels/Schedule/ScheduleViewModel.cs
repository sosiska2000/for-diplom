using Rockstar.Admin.WPF.Models;
using Rockstar.Admin.WPF.Services.Interfaces;
using Rockstar.Admin.WPF.ViewModels.Base;
using Rockstar.Admin.WPF.ViewModels.Commands;
using Rockstar.Admin.WPF.Views.Schedule;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using ScheduleModel = Rockstar.Admin.WPF.Models.Schedule;

namespace Rockstar.Admin.WPF.ViewModels.Schedule
{
    public class ScheduleViewModel : ViewModelBase
    {
        private readonly IScheduleService _scheduleService;
        private readonly Action<Page> _navigate;

        // 👇 ИСПОЛЬЗУЕМ ПОЛНОЕ ИМЯ: Models.Schedule
        private ObservableCollection<Models.Schedule> _schedules = new();
        private ObservableCollection<Models.Schedule> _allSchedules = new();

        private string _searchText = string.Empty;
        private bool _isLoading = true;

        public ScheduleViewModel(IScheduleService scheduleService, Action<Page> navigate)
        {
            _scheduleService = scheduleService;
            _navigate = navigate;

            LoadSchedulesCommand = new AsyncRelayCommand(async () => await LoadSchedules());
            AddScheduleCommand = new RelayCommand(() => AddSchedule());
            EditScheduleCommand = new AsyncRelayCommand<int>(async (id) => await EditSchedule(id));
            DeleteScheduleCommand = new AsyncRelayCommand<int>(async (id) => await DeleteSchedule(id));
            OpenScheduleDetailsCommand = new RelayCommand<int>((id) => OpenScheduleDetails(id));
            OpenHistoryCommand = new RelayCommand(ExecuteOpenHistory);
            BackCommand = new RelayCommand(ExecuteBack);
            RefreshCommand = new RelayCommand(() => _ = LoadSchedules());

            _ = LoadSchedules();
        }

        // 👇 ИСПОЛЬЗУЕМ ПОЛНОЕ ИМЯ: Models.Schedule
        public ObservableCollection<Models.Schedule> Schedules
        {
            get => _schedules;
            private set
            {
                if (SetField(ref _schedules, value))
                {
                    OnPropertyChanged(nameof(FilteredSchedules));
                }
            }
        }

        // 👇 ИСПОЛЬЗУЕМ ПОЛНОЕ ИМЯ: Models.Schedule
        public ObservableCollection<Models.Schedule> FilteredSchedules
        {
            get
            {
                if (Schedules == null || Schedules.Count == 0)
                    return new ObservableCollection<Models.Schedule>();

                if (string.IsNullOrWhiteSpace(SearchText))
                    return Schedules;

                var filtered = Schedules.Where(s =>
                    s.DirectionName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    s.ServiceName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    s.TrainerName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                ).ToList();

                return new ObservableCollection<Models.Schedule>(filtered);
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetField(ref _searchText, value))
                {
                    OnPropertyChanged(nameof(FilteredSchedules));
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

        public Visibility LoadingVisibility => IsLoading ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ContentVisibility => IsLoading ? Visibility.Collapsed : Visibility.Visible;

        public int AllSchedulesCount => _allSchedules?.Count ?? 0;
        public int FilteredSchedulesCount => _schedules?.Count ?? 0;

        public ICommand LoadSchedulesCommand { get; }
        public ICommand AddScheduleCommand { get; }
        public ICommand EditScheduleCommand { get; }
        public ICommand DeleteScheduleCommand { get; }
        public ICommand OpenScheduleDetailsCommand { get; }
        public ICommand OpenHistoryCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand RefreshCommand { get; }

        private async Task LoadSchedules()
        {
            try
            {
                Debug.WriteLine("Loading schedules...");
                IsLoading = true;

                var schedules = await _scheduleService.GetGroupSchedulesAsync();
                Debug.WriteLine($"Loaded {schedules.Count} schedules");

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _allSchedules = new ObservableCollection<Models.Schedule>(schedules);

                    // 👇 ИСПОЛЬЗУЕМ ПОЛНОЕ ИМЯ: Models.Schedule
                    Schedules = new ObservableCollection<Models.Schedule>(
                        schedules.OrderBy(s => s.DateTime));

                    foreach (var s in schedules)
                    {
                        Debug.WriteLine($"Schedule {s.Id}: {s.DirectionName} - {s.DateTime}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadSchedules error: {ex.Message}");
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки расписания: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddSchedule()
        {
            _navigate(new EditScheduleView(_navigate, null));
        }

        private async Task EditSchedule(int id)
        {
            try
            {
                var schedule = await _scheduleService.GetScheduleByIdAsync(id);
                if (schedule != null)
                {
                    _navigate(new EditScheduleView(_navigate, schedule));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditSchedule error: {ex.Message}");
                MessageBox.Show($"Ошибка при редактировании: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteSchedule(int id)
        {
            try
            {
                var result = MessageBox.Show(
                    "Вы уверены, что хотите удалить это занятие?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var success = await _scheduleService.DeleteScheduleAsync(id);
                    if (success)
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            var schedule = Schedules.FirstOrDefault(s => s.Id == id);
                            if (schedule != null)
                            {
                                Schedules.Remove(schedule);
                                _allSchedules.Remove(schedule);
                                OnPropertyChanged(nameof(FilteredSchedules));
                            }
                            MessageBox.Show("Занятие успешно удалено!", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        });
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при удалении занятия!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteSchedule error: {ex.Message}");
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenScheduleDetails(int id)
        {
            _navigate(new ScheduleDetailsView(_navigate, id));
        }

        private void ExecuteOpenHistory()
        {
            _navigate(new ScheduleHistoryView(_navigate));
        }

        private void ExecuteBack()
        {
            _navigate(new Views.Main.MainPage(_navigate));
        }
    }
}