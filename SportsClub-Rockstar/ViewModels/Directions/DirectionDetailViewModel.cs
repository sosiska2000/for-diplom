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
    public class DirectionDetailViewModel : ViewModelBase
    {
        private readonly IDirectionService _directionService;
        private readonly IServiceService _serviceService;
        private readonly IServiceTypeService _serviceTypeService;
        private readonly Action<Page> _navigate;
        private readonly string _directionKey;

        private Direction? _direction;
        private ObservableCollection<Service> _services = new();
        private ObservableCollection<ServiceType> _serviceTypes = new();

        public DirectionDetailViewModel(
            IDirectionService directionService,
            IServiceService serviceService,
            IServiceTypeService serviceTypeService,
            Action<Page> navigate,
            string directionKey)
        {
            _directionService = directionService;
            _serviceService = serviceService;
            _serviceTypeService = serviceTypeService;
            _navigate = navigate;
            _directionKey = directionKey;

            // Создаем команды
            AddServiceCommand = new RelayCommand(ExecuteAddService);
            AddServiceTypeCommand = new RelayCommand(ExecuteAddServiceType);
            BackCommand = new RelayCommand(ExecuteBack);

            // 👇 НОВЫЕ КОМАНДЫ для услуг
            EditServiceCommand = new RelayCommand<Service>(ExecuteEditService);
            DeleteServiceCommand = new AsyncRelayCommand<Service>(ExecuteDeleteService);

            // Команды для типов услуг
            EditServiceTypeCommand = new RelayCommand<ServiceType>(ExecuteEditServiceType);
            DeleteServiceTypeCommand = new AsyncRelayCommand<ServiceType>(ExecuteDeleteServiceType);

            // Запускаем загрузку
            _ = LoadDataAsync();
        }

        public Direction? Direction
        {
            get => _direction;
            private set => SetField(ref _direction, value);
        }

        public ObservableCollection<Service> Services
        {
            get => _services;
            private set => SetField(ref _services, value);
        }

        public ObservableCollection<ServiceType> ServiceTypes
        {
            get => _serviceTypes;
            private set => SetField(ref _serviceTypes, value);
        }

        public string PageTitle => Direction?.Name ?? "Направление";

        // Команды
        public ICommand AddServiceCommand { get; }
        public ICommand EditServiceCommand { get; }
        public ICommand DeleteServiceCommand { get; }
        public ICommand AddServiceTypeCommand { get; }
        public ICommand EditServiceTypeCommand { get; }
        public ICommand DeleteServiceTypeCommand { get; }
        public ICommand BackCommand { get; }

        private async Task LoadDataAsync()
        {
            try
            {
                Debug.WriteLine($"=== LoadDataAsync for {_directionKey} ===");

                Direction = await _directionService.GetByKeyAsync(_directionKey);

                if (Direction == null)
                {
                    Debug.WriteLine($"Direction not found for key: {_directionKey}");
                    return;
                }

                var services = await _serviceService.GetByDirectionIdAsync(Direction.Id);
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Services = new ObservableCollection<Service>(services);
                });

                var serviceTypes = await _serviceTypeService.GetByDirectionIdAsync(Direction.Id);
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ServiceTypes = new ObservableCollection<ServiceType>(serviceTypes);
                });

                OnPropertyChanged(nameof(PageTitle));
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
        }

        // ==================== УСЛУГИ ====================

        private async void ExecuteAddService()
        {
            try
            {
                if (Direction == null) return;

                // Открываем диалог создания услуги
                var dialog = new Views.Directions.EditServiceDialog(null);
                dialog.Owner = Application.Current.MainWindow;

                if (dialog.ShowDialog() == true)
                {
                    var newService = new Service
                    {
                        DirectionId = Direction.Id,
                        Name = dialog.ServiceName,
                        Price = dialog.Price,
                        SessionsCount = dialog.SessionsCount,
                        DurationMinutes = dialog.DurationMinutes,
                        Description = dialog.Description,
                        IsActive = true
                    };

                    var result = await _serviceService.CreateAsync(newService);
                    if (result)
                    {
                        await LoadDataAsync();
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            MessageBox.Show("Услуга успешно добавлена", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        });
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при добавлении услуги", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AddService error: {ex.Message}");
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        // 👇 НОВЫЙ МЕТОД: редактирование услуги
        private void ExecuteEditService(Service? service)
        {
            if (service == null) return;

            var dialog = new Views.Directions.EditServiceDialog(service);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                // Обновляем данные услуги
                service.Name = dialog.ServiceName;
                service.Price = dialog.Price;
                service.SessionsCount = dialog.SessionsCount;
                service.DurationMinutes = dialog.DurationMinutes;
                service.Description = dialog.Description;

                _ = UpdateServiceAsync(service);
            }
        }

        private async Task UpdateServiceAsync(Service service)
        {
            try
            {
                var success = await _serviceService.UpdateAsync(service);
                if (success)
                {
                    // Показываем сообщение
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        MessageBox.Show("Услуга успешно обновлена!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    });

                    // 👇 ПЕРЕЗАГРУЖАЕМ ДАННЫЕ (ВНЕ Dispatcher.InvokeAsync)
                    await LoadDataAsync();
                }
                else
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        MessageBox.Show("Ошибка при обновлении услуги", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateService error: {ex.Message}");
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private async Task ExecuteDeleteService(Service? service)
        {
            if (service == null) return;

            var result = MessageBox.Show(
                $"Удалить услугу '{service.Name}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var success = await _serviceService.DeleteAsync(service.Id);
                    if (success)
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            Services.Remove(service);
                            MessageBox.Show("Услуга удалена", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        });
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

        // ==================== ТИПЫ УСЛУГ ====================

        private async void ExecuteAddServiceType()
        {
            try
            {
                if (Direction == null) return;

                var dialog = new Views.Directions.EditServiceTypeDialog(null);
                dialog.Owner = Application.Current.MainWindow;

                if (dialog.ShowDialog() == true)
                {
                    var newServiceType = new ServiceType
                    {
                        DirectionId = Direction.Id,
                        Name = dialog.ServiceTypeName,
                        Description = dialog.Description,
                        DefaultDuration = dialog.DefaultDuration
                    };

                    var result = await _serviceTypeService.CreateAsync(newServiceType);
                    if (result)
                    {
                        await LoadDataAsync();
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            MessageBox.Show("Вид занятий успешно добавлен", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AddServiceType error: {ex.Message}");
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private void ExecuteEditServiceType(ServiceType? serviceType)
        {
            if (serviceType == null) return;

            var dialog = new Views.Directions.EditServiceTypeDialog(serviceType);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                serviceType.Name = dialog.ServiceTypeName;
                serviceType.Description = dialog.Description;
                serviceType.DefaultDuration = dialog.DefaultDuration;

                _ = UpdateServiceTypeAsync(serviceType);
            }
        }

        private async Task UpdateServiceTypeAsync(ServiceType serviceType)
        {
            try
            {
                var success = await _serviceTypeService.UpdateAsync(serviceType);
                if (success)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        MessageBox.Show("Вид занятий успешно обновлен!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        _ = LoadDataAsync();
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateServiceType error: {ex.Message}");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExecuteDeleteServiceType(ServiceType? serviceType)
        {
            if (serviceType == null) return;

            var result = MessageBox.Show(
                $"Удалить вид занятий '{serviceType.Name}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var success = await _serviceTypeService.DeleteAsync(serviceType.Id);
                    if (success)
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            ServiceTypes.Remove(serviceType);
                            MessageBox.Show("Вид занятий удален", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        });
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
            _navigate(new Views.Directions.DirectionsView(_navigate));
        }
    }
}