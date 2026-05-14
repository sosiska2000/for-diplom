using Rockstar.Admin.WPF.Models;
using Rockstar.Admin.WPF.Services.Interfaces;
using Rockstar.Admin.WPF.ViewModels.Commands;
using Rockstar.Admin.WPF.Views.Schedule;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Rockstar.Admin.WPF.ViewModels.Schedule
{
    public class EditScheduleViewModel : INotifyPropertyChanged
    {
        private readonly IScheduleService _scheduleService;
        private readonly IApiService _apiService;
        private readonly Action<Page> _navigate;
        private readonly Models.Schedule? _editingSchedule;

        private string _pageTitle = string.Empty;
        public string PageTitle { get => _pageTitle; set { _pageTitle = value; OnPropertyChanged(); } }

        private string _saveButtonText = string.Empty;
        public string SaveButtonText { get => _saveButtonText; set { _saveButtonText = value; OnPropertyChanged(); } }

        private ObservableCollection<Direction> _directions = new();
        public ObservableCollection<Direction> Directions { get => _directions; set { _directions = value; OnPropertyChanged(); } }

        private ObservableCollection<Service> _services = new();
        public ObservableCollection<Service> Services { get => _services; set { _services = value; OnPropertyChanged(); } }

        private ObservableCollection<Trainer> _trainers = new();
        public ObservableCollection<Trainer> Trainers { get => _trainers; set { _trainers = value; OnPropertyChanged(); } }

        private Direction? _selectedDirection;
        public Direction? SelectedDirection
        {
            get => _selectedDirection;
            set
            {
                _selectedDirection = value;
                OnPropertyChanged();
                if (value != null) _ = LoadServicesAsync(value.Id);
                else Services.Clear();
                ValidateForm();
            }
        }

        private Service? _selectedService;
        public Service? SelectedService
        {
            get => _selectedService;
            set
            {
                _selectedService = value;
                OnPropertyChanged();
                if (value != null) Price = value.Price;
                ValidateForm();
            }
        }

        private Trainer? _selectedTrainer;
        public Trainer? SelectedTrainer
        {
            get => _selectedTrainer;
            set { _selectedTrainer = value; OnPropertyChanged(); ValidateForm(); }
        }

        private DateTime _date = DateTime.Today;
        public DateTime Date { get => _date; set { _date = value; OnPropertyChanged(); ValidateForm(); } }

        private string _time = "10:00";
        public string Time { get => _time; set { _time = value; OnPropertyChanged(); ValidateForm(); } }

        private int _durationMinutes = 60;
        public int DurationMinutes { get => _durationMinutes; set { _durationMinutes = value; OnPropertyChanged(); ValidateForm(); } }

        private int _maxParticipants = 20;
        public int MaxParticipants { get => _maxParticipants; set { _maxParticipants = value; OnPropertyChanged(); ValidateForm(); } }

        private decimal _price = 0;
        public decimal Price { get => _price; set { _price = value; OnPropertyChanged(); ValidateForm(); } }

        private bool _canSave;
        public bool CanSave { get => _canSave; set { _canSave = value; OnPropertyChanged(); } }

        // ========== ПОВТОРЯЮЩИЕСЯ ЗАНЯТИЯ ==========
        private bool _isRecurring;
        public bool IsRecurring
        {
            get => _isRecurring;
            set { _isRecurring = value; OnPropertyChanged(); }
        }

        private string _recurrencePattern = "weekly";
        public string RecurrencePattern
        {
            get => _recurrencePattern;
            set { _recurrencePattern = value; OnPropertyChanged(); }
        }

        private int _recurrenceInterval = 1;
        public int RecurrenceInterval
        {
            get => _recurrenceInterval;
            set { _recurrenceInterval = value; OnPropertyChanged(); }
        }

        private string _weekDays = string.Empty;
        public string WeekDays
        {
            get => _weekDays;
            set { _weekDays = value; OnPropertyChanged(); }
        }

        private DateTime? _recurrenceEndDate;
        public DateTime? RecurrenceEndDate
        {
            get => _recurrenceEndDate;
            set { _recurrenceEndDate = value; OnPropertyChanged(); }
        }

        private int _maxOccurrences = 10;
        public int MaxOccurrences
        {
            get => _maxOccurrences;
            set { _maxOccurrences = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public EditScheduleViewModel(IScheduleService scheduleService, IApiService apiService, Action<Page> navigate, Models.Schedule? schedule)
        {
            _scheduleService = scheduleService;
            _apiService = apiService;
            _navigate = navigate;
            _editingSchedule = schedule;

            SaveCommand = new AsyncRelayCommand(async () => await SaveAsync(), () => CanSave);
            CancelCommand = new RelayCommand(() => Cancel());

            _ = InitializeAsync();

            if (schedule != null)
            {
                PageTitle = "Редактирование занятия";
                SaveButtonText = "Сохранить изменения";
                LoadScheduleData(schedule);
            }
            else
            {
                PageTitle = "Новое занятие";
                SaveButtonText = "Создать занятие";
                RecurrenceEndDate = DateTime.Today.AddMonths(1);
            }
        }

        private async Task InitializeAsync()
        {
            await LoadDirections();
            await LoadTrainers();
        }

        private async Task LoadDirections()
        {
            try
            {
                var directions = await _scheduleService.GetDirectionsAsync();
                Directions = new ObservableCollection<Direction>(directions);
                Debug.WriteLine($"Loaded {directions.Count} directions");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading directions: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки направлений: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadServicesAsync(int directionId)
        {
            try
            {
                var services = await _scheduleService.GetServicesByDirectionAsync(directionId);
                Services = new ObservableCollection<Service>(services);
                Debug.WriteLine($"Loaded {services.Count} services for direction {directionId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading services: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки услуг: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadTrainers()
        {
            try
            {
                var trainers = await _scheduleService.GetTrainersAsync();
                Trainers = new ObservableCollection<Trainer>(trainers);
                Debug.WriteLine($"Loaded {trainers.Count} trainers");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading trainers: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки тренеров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadScheduleData(Models.Schedule schedule)
        {
            SelectedDirection = Directions.FirstOrDefault(d => d.Id == schedule.DirectionId);
            if (SelectedDirection != null)
            {
                Task.Run(async () => await LoadServicesAsync(SelectedDirection.Id))
                    .ContinueWith(t => Application.Current.Dispatcher.Invoke(() =>
                        SelectedService = Services.FirstOrDefault(s => s.Id == schedule.ServiceId)));
            }
            SelectedTrainer = Trainers.FirstOrDefault(t => t.Id == schedule.TrainerId);
            Date = schedule.DateTime.Date;
            Time = schedule.DateTime.ToString("HH:mm");
            DurationMinutes = schedule.DurationMinutes;
            MaxParticipants = schedule.MaxParticipants;
            Price = schedule.Price;
        }

        private void ValidateForm()
        {
            CanSave = SelectedDirection != null && SelectedService != null && SelectedTrainer != null &&
                      !string.IsNullOrWhiteSpace(Time) && DurationMinutes > 0 && MaxParticipants > 0 && Price > 0;
        }

        private DateTime CombineDateAndTime() =>
            TimeSpan.TryParse(Time, out var ts) ? Date.Date.Add(ts) : Date;

        private async Task SaveAsync()
        {
            if (!CanSave)
            {
                MessageBox.Show("Заполните все обязательные поля", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool success;
                string errorMessage = null;

                if (IsRecurring)
                {
                    success = await SaveRecurringAsync();
                    if (!success) errorMessage = "Ошибка при создании повторяющихся занятий";
                }
                else if (_editingSchedule != null)
                {
                    success = await UpdateSingleScheduleAsync();
                    if (!success) errorMessage = "Ошибка при обновлении занятия";
                }
                else
                {
                    success = await CreateSingleScheduleAsync();
                    if (!success) errorMessage = "Ошибка при создании занятия";
                }

                if (success)
                {
                    MessageBox.Show(_editingSchedule != null ? "Занятие обновлено!" : "Занятие создано!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    _navigate(new ScheduleView(_navigate));
                }
                else
                {
                    MessageBox.Show(errorMessage ?? "Ошибка при сохранении! Проверьте, не занят ли тренер в это время.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveAsync error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // 👇 ПОДРОБНАЯ ОШИБКА
                string userMessage = "Ошибка при сохранении: " + ex.Message;
                if (ex.InnerException != null)
                {
                    userMessage += "\n\n" + ex.InnerException.Message;
                }

                MessageBox.Show(userMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<bool> CreateSingleScheduleAsync()
        {
            try
            {
                var schedule = new Models.Schedule
                {
                    Id = 0,
                    DirectionId = SelectedDirection?.Id ?? 0,
                    DirectionName = SelectedDirection?.Name ?? "",
                    ServiceId = SelectedService?.Id,
                    ServiceName = SelectedService?.Name ?? "",
                    TrainerId = SelectedTrainer?.Id,
                    TrainerName = SelectedTrainer?.FullName ?? "",
                    DateTime = CombineDateAndTime(),
                    DurationMinutes = DurationMinutes,
                    MaxParticipants = MaxParticipants,
                    Price = Price,
                    IsGroup = true,
                    IsActive = true
                };

                Debug.WriteLine($"Creating schedule: TrainerId={schedule.TrainerId}, DirectionId={schedule.DirectionId}, ServiceId={schedule.ServiceId}, DateTime={schedule.DateTime}");

                return await _scheduleService.CreateScheduleAsync(schedule);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CreateSingleScheduleAsync error: {ex.Message}");
                throw; // Пробрасываем исключение дальше для детальной обработки
            }
        }

        private async Task<bool> UpdateSingleScheduleAsync()
        {
            try
            {
                var schedule = new Models.Schedule
                {
                    Id = _editingSchedule?.Id ?? 0,
                    DirectionId = SelectedDirection?.Id ?? 0,
                    DirectionName = SelectedDirection?.Name ?? "",
                    ServiceId = SelectedService?.Id,
                    ServiceName = SelectedService?.Name ?? "",
                    TrainerId = SelectedTrainer?.Id,
                    TrainerName = SelectedTrainer?.FullName ?? "",
                    DateTime = CombineDateAndTime(),
                    DurationMinutes = DurationMinutes,
                    MaxParticipants = MaxParticipants,
                    Price = Price,
                    IsGroup = true,
                    IsActive = true
                };

                Debug.WriteLine($"Updating schedule: Id={schedule.Id}, TrainerId={schedule.TrainerId}, DateTime={schedule.DateTime}");

                return await _scheduleService.UpdateScheduleAsync(schedule);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateSingleScheduleAsync error: {ex.Message}");
                throw;
            }
        }

        private async Task<bool> SaveRecurringAsync()
        {
            try
            {
                var startDateTime = CombineDateAndTime();

                var dto = new
                {
                    TrainerId = SelectedTrainer?.Id,
                    DirectionId = SelectedDirection?.Id,
                    ServiceId = SelectedService?.Id,
                    DurationMinutes = DurationMinutes,
                    MaxParticipants = MaxParticipants,
                    Price = Price,
                    IsGroup = true,
                    Pattern = RecurrencePattern,
                    Interval = RecurrenceInterval,
                    WeekDays = WeekDays,
                    StartDate = startDateTime.Date,
                    EndDate = RecurrenceEndDate,
                    MaxOccurrences = MaxOccurrences,
                    StartTime = new TimeSpan(startDateTime.Hour, startDateTime.Minute, startDateTime.Second)
                };

                Debug.WriteLine($"📤 Creating recurring schedules: {System.Text.Json.JsonSerializer.Serialize(dto)}");

                var result = await _apiService.PostAsync<dynamic>("schedule/recurring", dto);
                return result != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveRecurringAsync error: {ex.Message}");
                throw;
            }
        }

        private void Cancel() => _navigate(new ScheduleView(_navigate));

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}