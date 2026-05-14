using Rockstar.Admin.WPF.Models;
using Rockstar.Admin.WPF.Services.Interfaces;
using Rockstar.Admin.WPF.ViewModels.Base;
using Rockstar.Admin.WPF.ViewModels.Commands;
using Rockstar.Admin.WPF.Views.Directions;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Rockstar.Admin.WPF.ViewModels.Directions
{
    public class AddServiceViewModel : ViewModelBase
    {
        private readonly IServiceService _serviceService;
        private readonly Action<Page> _navigate;
        private readonly Service? _editingService;
        private readonly int _directionId;
        private readonly string _directionName;
        private readonly string _directionKey;  // ✅ Ключ направления (yoga/fitness/climbing)

        private string _name = string.Empty;
        private decimal _price;
        private int _sessionsCount = 1;
        private int? _durationMinutes = 60;
        private string _description = string.Empty;

        private string _nameError = string.Empty;
        private string _priceError = string.Empty;

        public AddServiceViewModel(
            IServiceService serviceService,
            Action<Page> navigate,
            Service? service,
            int directionId,
            string directionName,
            string directionKey)  // ✅ Принимаем directionKey
        {
            _serviceService = serviceService;
            _navigate = navigate;
            _editingService = service;
            _directionId = directionId;
            _directionName = directionName;
            _directionKey = directionKey;  // ✅ Сохраняем ключ

            SaveCommand = new RelayCommand(async () => await ExecuteSave(), () => CanSave);
            CancelCommand = new RelayCommand(ExecuteCancel);

            if (_editingService != null)
            {
                LoadServiceData();
            }
        }

        private void LoadServiceData()
        {
            if (_editingService == null) return;
            _name = _editingService.Name;
            _price = _editingService.Price;
            _sessionsCount = _editingService.SessionsCount;
            _durationMinutes = _editingService.DurationMinutes;
            _description = _editingService.Description;

            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Price));
            OnPropertyChanged(nameof(SessionsCount));
            OnPropertyChanged(nameof(DurationMinutes));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(CanSave));
        }

        public string PageTitle => _editingService == null ? "Новая услуга" : "Редактирование услуги";
        public string DirectionTitle => _directionName;

        public string Name
        {
            get => _name;
            set { if (SetField(ref _name, value)) { ValidateName(); OnPropertyChanged(nameof(CanSave)); } }
        }

        public decimal Price
        {
            get => _price;
            set { if (SetField(ref _price, value)) { ValidatePrice(); OnPropertyChanged(nameof(CanSave)); } }
        }

        public int SessionsCount
        {
            get => _sessionsCount;
            set { if (SetField(ref _sessionsCount, value)) OnPropertyChanged(nameof(CanSave)); }
        }

        public int? DurationMinutes
        {
            get => _durationMinutes;
            set { if (SetField(ref _durationMinutes, value)) OnPropertyChanged(nameof(CanSave)); }
        }

        public string Description
        {
            get => _description;
            set { if (SetField(ref _description, value)) OnPropertyChanged(nameof(CanSave)); }
        }

        public string NameError
        {
            get => _nameError;
            private set => SetField(ref _nameError, value);
        }

        public string PriceError
        {
            get => _priceError;
            private set => SetField(ref _priceError, value);
        }

        public bool CanSave =>
            !string.IsNullOrWhiteSpace(Name) &&
            Price > 0 &&
            SessionsCount > 0;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private void ValidateName() =>
            NameError = string.IsNullOrWhiteSpace(Name) ? "Введите название услуги" : string.Empty;

        private void ValidatePrice() =>
            PriceError = Price <= 0 ? "Стоимость должна быть больше 0" : string.Empty;

        private async Task ExecuteSave()
        {
            ValidateName();
            ValidatePrice();

            if (!CanSave) return;

            try
            {
                var service = _editingService ?? new Service { DirectionId = _directionId };

                service.Name = Name.Trim();
                service.Price = Price;
                service.SessionsCount = SessionsCount;
                service.DurationMinutes = DurationMinutes;
                service.Description = Description.Trim();
                service.IsActive = true;

                bool success = _editingService != null
                    ? await _serviceService.UpdateAsync(service)
                    : await _serviceService.CreateAsync(service);

                if (success)
                {
                    MessageBox.Show(
                        _editingService != null ? "Услуга обновлена!" : "Услуга создана!",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // ✅ Возврат с правильным directionKey
                    _navigate(new DirectionDetailView(_navigate, _directionKey));
                }
                else
                {
                    MessageBox.Show("Ошибка сохранения. Возможно, услуга с таким названием уже существует.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancel()
        {
            // ✅ Возврат с правильным directionKey
            _navigate(new DirectionDetailView(_navigate, _directionKey));
        }
    }
}