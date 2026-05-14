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
    public class AddTrainerViewModel : ViewModelBase
    {
        private readonly ITrainerService _trainerService;
        private readonly IDirectionService _directionService;
        private readonly Action<Page> _navigate;
        private readonly Trainer? _editingTrainer;

        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private string _description = string.Empty;
        private byte[]? _photo;

        // 👇 УДАЛЯЕМ Email и Password
        // private string _email = string.Empty;
        // private string _password = string.Empty;

        private string _selectedDirection = string.Empty;
        private ObservableCollection<DirectionSelection> _allDirections = new();

        // 👇 НОВОЕ: дата начала работы
        private DateTime? _employmentDate;

        private string _firstNameError = string.Empty;
        private string _lastNameError = string.Empty;
        // private string _emailError = string.Empty;
        // private string _passwordError = string.Empty;

        public class DirectionSelection : ViewModelBase
        {
            public Direction Direction { get; set; }

            private bool _isSelected;
            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    if (SetField(ref _isSelected, value))
                    {
                        SelectionChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }

            public event EventHandler? SelectionChanged;

            public DirectionSelection(Direction direction)
            {
                Direction = direction;
            }
        }

        public AddTrainerViewModel(
            ITrainerService trainerService,
            IDirectionService directionService,
            Action<Page> navigate,
            Trainer? trainer)
        {
            _trainerService = trainerService;
            _directionService = directionService;
            _navigate = navigate;
            _editingTrainer = trainer;

            Debug.WriteLine($"=== AddTrainerViewModel Constructor ===");
            Debug.WriteLine($"Editing mode: {_editingTrainer != null}");

            _ = LoadDirectionsAsync();

            if (_editingTrainer != null)
            {
                LoadTrainerData();
            }
        }

        public string PageTitle => _editingTrainer == null ? "Добавить тренера" : "Редактировать тренера";
        public string SaveButtonText => _editingTrainer == null ? "Добавить" : "Сохранить";

        public string FirstName
        {
            get => _firstName;
            set { if (SetField(ref _firstName, value)) { ValidateFirstName(); OnPropertyChanged(nameof(CanSave)); } }
        }

        public string LastName
        {
            get => _lastName;
            set { if (SetField(ref _lastName, value)) { ValidateLastName(); OnPropertyChanged(nameof(CanSave)); } }
        }

        // 👇 УДАЛЯЕМ Email и Password
        // public string Email { get; set; }
        // public string Password { get; set; }

        public string Description
        {
            get => _description;
            set { if (SetField(ref _description, value)) OnPropertyChanged(nameof(CanSave)); }
        }

        // 👇 НОВОЕ: дата начала работы
        public DateTime? EmploymentDate
        {
            get => _employmentDate;
            set
            {
                if (SetField(ref _employmentDate, value))
                {
                    OnPropertyChanged(nameof(ExperienceDisplay));
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        // Отображение стажа
        public string ExperienceDisplay
        {
            get
            {
                if (!_employmentDate.HasValue)
                    return "Стаж не указан";

                var today = DateTime.Today;
                var years = today.Year - _employmentDate.Value.Year;
                var months = today.Month - _employmentDate.Value.Month;

                if (months < 0)
                {
                    years--;
                    months += 12;
                }

                if (years == 0 && months == 0)
                    return "Менее месяца";

                if (years == 0)
                    return $"{months} мес.";

                if (months == 0)
                    return $"{years} {GetYearWord(years)}";

                return $"{years} {GetYearWord(years)} {months} мес.";
            }
        }

        private string GetYearWord(int years)
        {
            if (years % 10 == 1 && years % 100 != 11)
                return "год";
            if (years % 10 >= 2 && years % 10 <= 4 && (years % 100 < 10 || years % 100 >= 20))
                return "года";
            return "лет";
        }

        public string SelectedDirection
        {
            get => _selectedDirection;
            set { if (SetField(ref _selectedDirection, value)) OnPropertyChanged(nameof(CanSave)); }
        }

        public ObservableCollection<DirectionSelection> AllDirections
        {
            get => _allDirections;
            set => SetField(ref _allDirections, value);
        }

        public byte[]? Photo
        {
            get => _photo;
            set
            {
                if (SetField(ref _photo, value))
                {
                    OnPropertyChanged(nameof(PhotoPreview));
                    OnPropertyChanged(nameof(PhotoVisibility));
                }
            }
        }

        public string FirstNameError { get => _firstNameError; private set => SetField(ref _firstNameError, value); }
        public string LastNameError { get => _lastNameError; private set => SetField(ref _lastNameError, value); }
        // public string EmailError { get; set; }
        // public string PasswordError { get; set; }

        public Visibility PhotoVisibility => _photo == null ? Visibility.Visible : Visibility.Collapsed;
        public string? PhotoPreview => _photo != null ? "preview" : null;

        public string SelectedDirectionsDisplay
        {
            get
            {
                var selected = AllDirections.Where(d => d.IsSelected).Select(d => d.Direction.Name);
                return selected.Any() ? string.Join(", ", selected) : "Не выбрано";
            }
        }

        public bool CanSave
        {
            get
            {
                bool hasSelectedDirection = AllDirections.Any(d => d.IsSelected);

                bool baseValidation = !string.IsNullOrWhiteSpace(FirstName) &&
                                      !string.IsNullOrWhiteSpace(LastName) &&
                                      hasSelectedDirection &&
                                      EmploymentDate.HasValue; // 👈 Дата начала работы обязательна

                return baseValidation;
            }
        }

        public ICommand SaveCommand => new RelayCommand(async () =>
        {
            try
            {
                await ExecuteSave();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveCommand error: {ex.Message}");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }, () => CanSave);

        public ICommand CancelCommand => new RelayCommand(ExecuteCancel);

        private async Task LoadDirectionsAsync()
        {
            try
            {
                var directions = await _directionService.GetAllAsync();
                var directionSelections = directions.Select(d => new DirectionSelection(d)).ToList();

                foreach (var ds in directionSelections)
                {
                    ds.SelectionChanged += (s, e) =>
                    {
                        OnPropertyChanged(nameof(CanSave));
                        OnPropertyChanged(nameof(SelectedDirectionsDisplay));
                    };
                }

                AllDirections = new ObservableCollection<DirectionSelection>(directionSelections);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadDirectionsAsync error: {ex.Message}");
            }
        }

        private void LoadTrainerData()
        {
            if (_editingTrainer == null) return;

            _firstName = _editingTrainer.FirstName;
            _lastName = _editingTrainer.LastName;
            _description = _editingTrainer.Description;
            _selectedDirection = _editingTrainer.DirectionKey;
            _photo = _editingTrainer.Photo;

            // 👇 Загружаем дату начала работы
            _employmentDate = _editingTrainer.EmploymentDate;

            if (_editingTrainer.SelectedDirections != null && AllDirections.Any())
            {
                foreach (var directionSelection in AllDirections)
                {
                    directionSelection.IsSelected = _editingTrainer.SelectedDirections
                        .Any(d => d.Id == directionSelection.Direction.Id);
                }
            }

            OnPropertyChanged(nameof(FirstName));
            OnPropertyChanged(nameof(LastName));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(SelectedDirection));
            OnPropertyChanged(nameof(PhotoVisibility));
            OnPropertyChanged(nameof(EmploymentDate));
            OnPropertyChanged(nameof(ExperienceDisplay));
            OnPropertyChanged(nameof(CanSave));
            OnPropertyChanged(nameof(SelectedDirectionsDisplay));
        }

        private void ValidateFirstName()
        {
            FirstNameError = string.IsNullOrWhiteSpace(FirstName) ? "Обязательное поле" : string.Empty;
        }

        private void ValidateLastName()
        {
            LastNameError = string.IsNullOrWhiteSpace(LastName) ? "Обязательное поле" : string.Empty;
        }

        private async Task ExecuteSave()
        {
            Debug.WriteLine("=== ExecuteSave ===");

            ValidateFirstName();
            ValidateLastName();

            if (!CanSave)
            {
                Debug.WriteLine("Validation failed, cannot save");
                return;
            }

            try
            {
                var trainer = _editingTrainer ?? new Trainer();
                trainer.FirstName = FirstName;
                trainer.LastName = LastName;
                trainer.Description = Description;
                trainer.EmploymentDate = EmploymentDate; // 👈 Сохраняем дату
                trainer.Photo = Photo;

                trainer.SelectedDirections.Clear();
                foreach (var directionSelection in AllDirections.Where(d => d.IsSelected))
                {
                    trainer.SelectedDirections.Add(directionSelection.Direction);
                }

                var firstSelected = AllDirections.FirstOrDefault(d => d.IsSelected);
                if (firstSelected != null)
                {
                    trainer.DirectionId = firstSelected.Direction.Id;
                    trainer.DirectionName = firstSelected.Direction.Name;
                    trainer.DirectionKey = firstSelected.Direction.NameKey;
                }

                Debug.WriteLine($"Trainer data prepared:");
                Debug.WriteLine($"  Id: {trainer.Id}");
                Debug.WriteLine($"  FirstName: {trainer.FirstName}");
                Debug.WriteLine($"  LastName: {trainer.LastName}");
                Debug.WriteLine($"  EmploymentDate: {trainer.EmploymentDate:yyyy-MM-dd}");
                Debug.WriteLine($"  ExperienceYears: {trainer.ExperienceYears}");
                Debug.WriteLine($"  Selected directions count: {trainer.SelectedDirections.Count}");

                bool success;
                string successMessage;

                if (_editingTrainer == null)
                {
                    success = await _trainerService.CreateAsync(trainer);
                    successMessage = "Тренер успешно добавлен!";
                }
                else
                {
                    trainer.Id = _editingTrainer.Id;
                    success = await _trainerService.UpdateAsync(trainer);
                    successMessage = "Тренер успешно обновлен!";
                }

                Debug.WriteLine($"Save result: {success}");

                if (success)
                {
                    Debug.WriteLine("Navigate back to TrainersView");
                    MessageBox.Show(successMessage, "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    _navigate(new Views.Trainers.TrainersView(_navigate));
                }
                else
                {
                    Debug.WriteLine("Save failed, showing message");
                    MessageBox.Show("Ошибка сохранения данных.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ExecuteSave Exception: {ex.Message}");
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancel()
        {
            _navigate(new Views.Trainers.TrainersView(_navigate));
        }
    }
}