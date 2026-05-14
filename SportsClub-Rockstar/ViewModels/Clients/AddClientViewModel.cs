using Rockstar.Admin.WPF.Models;
using Rockstar.Admin.WPF.Services.Interfaces;
using Rockstar.Admin.WPF.ViewModels.Commands;
using Rockstar.Admin.WPF.Views.Clients;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Rockstar.Admin.WPF.ViewModels.Clients
{
    public class AddClientViewModel : INotifyPropertyChanged
    {
        private readonly IClientService _clientService;
        private readonly Action<Page> _navigate;
        private readonly Client? _editingClient;

        private string _pageTitle = string.Empty;
        public string PageTitle
        {
            get => _pageTitle;
            set { _pageTitle = value; OnPropertyChanged(); }
        }

        private string _saveButtonText = string.Empty;
        public string SaveButtonText
        {
            get => _saveButtonText;
            set { _saveButtonText = value; OnPropertyChanged(); }
        }

        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(); ValidateForm(); }
        }

        private string _lastName = string.Empty;
        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(); ValidateForm(); }
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); ValidateForm(); }
        }

        // Поле для пароля
        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                ValidatePassword();
                ValidateForm();
            }
        }

        private string _phone = string.Empty;
        public string Phone
        {
            get => _phone;
            set
            {
                if (_phone != value)
                {
                    _phone = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(RawPhone));
                    ValidatePhone();
                    ValidateForm();
                }
            }
        }

        // Чистый номер для отправки в API (только цифры, начинается с 7)
        public string RawPhone
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_phone))
                    return string.Empty;

                // Извлекаем только цифры
                string digits = Regex.Replace(_phone, @"[^\d]", "");

                if (string.IsNullOrEmpty(digits))
                    return string.Empty;

                // Если начинается с 8, заменяем на 7
                if (digits.Length > 0 && digits[0] == '8')
                {
                    digits = "7" + digits.Substring(1);
                }
                // Если начинается с 9 и длина 10, добавляем 7
                else if (digits.Length == 10 && digits[0] == '9')
                {
                    digits = "7" + digits;
                }

                // Ограничиваем 11 цифрами
                if (digits.Length > 11)
                {
                    digits = digits.Substring(0, 11);
                }

                return digits;
            }
        }

        // Дата рождения
        private DateTime? _birthDate;
        public DateTime? BirthDate
        {
            get => _birthDate;
            set
            {
                if (value.HasValue)
                {
                    _birthDate = value.Value.Date;
                }
                else
                {
                    _birthDate = null;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(AgeDisplay));
                ValidateForm();
            }
        }

        public string AgeDisplay
        {
            get
            {
                if (!_birthDate.HasValue) return "Не указан";
                var today = DateTime.Today;
                var age = today.Year - _birthDate.Value.Year;
                if (_birthDate.Value.Date > today.AddYears(-age)) age--;
                return $"{age} {GetAgeWord(age)}";
            }
        }

        private string GetAgeWord(int age)
        {
            if (age % 10 == 1 && age % 100 != 11) return "год";
            if (age % 10 >= 2 && age % 10 <= 4 && (age % 100 < 10 || age % 100 >= 20)) return "года";
            return "лет";
        }

        public int? Age => _birthDate.HasValue ? CalculateAge() : null;

        private int CalculateAge()
        {
            if (!_birthDate.HasValue) return 0;
            var today = DateTime.Today;
            var age = today.Year - _birthDate.Value.Year;
            if (_birthDate.Value.Date > today.AddYears(-age)) age--;
            return age;
        }

        private bool _canSave;
        public bool CanSave
        {
            get => _canSave;
            set { _canSave = value; OnPropertyChanged(); }
        }

        // Видимость поля пароля (только для новых клиентов)
        public Visibility PasswordFieldVisibility => _editingClient == null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility EditModeInfoVisibility => _editingClient != null ? Visibility.Visible : Visibility.Collapsed;

        // Ошибки валидации
        private string _firstNameError = string.Empty;
        public string FirstNameError
        {
            get => _firstNameError;
            set { _firstNameError = value; OnPropertyChanged(); }
        }

        private string _lastNameError = string.Empty;
        public string LastNameError
        {
            get => _lastNameError;
            set { _lastNameError = value; OnPropertyChanged(); }
        }

        private string _emailError = string.Empty;
        public string EmailError
        {
            get => _emailError;
            set { _emailError = value; OnPropertyChanged(); }
        }

        private string _passwordError = string.Empty;
        public string PasswordError
        {
            get => _passwordError;
            set { _passwordError = value; OnPropertyChanged(); }
        }

        private string _phoneError = string.Empty;
        public string PhoneError
        {
            get => _phoneError;
            set { _phoneError = value; OnPropertyChanged(); }
        }

        private string _birthDateError = string.Empty;
        public string BirthDateError
        {
            get => _birthDateError;
            set { _birthDateError = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddClientViewModel(IClientService clientService, Action<Page> navigate, Client? client)
        {
            _clientService = clientService;
            _navigate = navigate;
            _editingClient = client;

            SaveCommand = new AsyncRelayCommand(async () => await SaveAsync(), () => CanSave);
            CancelCommand = new RelayCommand(() => Cancel());

            if (client != null)
            {
                PageTitle = "Редактирование клиента";
                SaveButtonText = "Сохранить изменения";
                FirstName = client.FirstName;
                LastName = client.LastName;
                Email = client.Email;

                if (!string.IsNullOrWhiteSpace(client.Phone))
                {
                    Phone = FormatPhoneForDisplay(client.Phone);
                }
                else
                {
                    Phone = string.Empty;
                }

                // 👇 ИСПРАВЛЕНО: загружаем дату рождения из модели, НЕ вычисляем из возраста
                if (client.BirthDate.HasValue)
                {
                    _birthDate = client.BirthDate.Value.Date;
                    OnPropertyChanged(nameof(BirthDate));
                    OnPropertyChanged(nameof(AgeDisplay));
                }
                else if (client.Age.HasValue && client.Age.Value > 0)
                {
                    // Запасной вариант - только если нет даты рождения
                    _birthDate = DateTime.Today.AddYears(-client.Age.Value);
                    OnPropertyChanged(nameof(BirthDate));
                    OnPropertyChanged(nameof(AgeDisplay));
                }
            }
            else
            {
                PageTitle = "Новый клиент";
                SaveButtonText = "Создать клиента";
            }

            ValidateForm();
        }
        private string FormatPhoneForDisplay(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            string digits = Regex.Replace(phone, @"[^\d]", "");

            if (digits.Length == 11 && (digits[0] == '7' || digits[0] == '8'))
            {
                string number = digits;
                if (number[0] == '8')
                {
                    number = "7" + number.Substring(1);
                }
                return $"+{number[0]} ({number.Substring(1, 3)}) {number.Substring(4, 3)}-{number.Substring(7, 2)}-{number.Substring(9, 2)}";
            }
            
            if (phone.StartsWith("+") && phone.Length >= 10)
                return phone;

            return phone;
        }

        private void ValidatePhone()
        {
            if (string.IsNullOrWhiteSpace(_phone))
            {
                PhoneError = "Номер телефона обязателен";
                return;
            }

            string cleaned = RawPhone;

            if (string.IsNullOrWhiteSpace(cleaned))
            {
                PhoneError = "Введите номер телефона";
                return;
            }

            if (cleaned.Length != 11)
            {
                PhoneError = $"Номер должен содержать 11 цифр (сейчас {cleaned.Length})";
                return;
            }

            if (cleaned[0] != '7')
            {
                PhoneError = "Номер должен начинаться с 7 или 8";
                return;
            }

            if (!long.TryParse(cleaned, out _))
            {
                PhoneError = "Номер должен содержать только цифры";
                return;
            }

            PhoneError = string.Empty;
        }

        private void ValidateEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                EmailError = "Email обязателен";
                return;
            }

            var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(Email, emailPattern))
            {
                EmailError = "Введите корректный email (например: name@domain.com)";
            }
            else
            {
                EmailError = string.Empty;
            }
        }

        private void ValidatePassword()
        {
            if (_editingClient != null)
            {
                PasswordError = string.Empty;
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                PasswordError = "Пароль обязателен";
                return;
            }

            if (Password.Length < 6)
            {
                PasswordError = "Пароль должен содержать минимум 6 символов";
                return;
            }

            PasswordError = string.Empty;
        }

        private void ValidateForm()
        {
            FirstNameError = string.Empty;
            LastNameError = string.Empty;
            EmailError = string.Empty;
            PasswordError = string.Empty;
            BirthDateError = string.Empty;

            bool isValid = true;

            if (string.IsNullOrWhiteSpace(FirstName)) { FirstNameError = "Имя обязательно"; isValid = false; }
            if (string.IsNullOrWhiteSpace(LastName)) { LastNameError = "Фамилия обязательна"; isValid = false; }

            ValidateEmail();
            if (!string.IsNullOrEmpty(EmailError)) isValid = false;

            ValidatePassword();
            if (!string.IsNullOrEmpty(PasswordError)) isValid = false;

            ValidatePhone();
            if (!string.IsNullOrEmpty(PhoneError)) isValid = false;

            if (!_birthDate.HasValue)
            {
                BirthDateError = "Дата рождения обязательна";
                isValid = false;
            }
            else if (_birthDate.Value > DateTime.Today)
            {
                BirthDateError = "Дата рождения не может быть в будущем";
                isValid = false;
            }
            else if (_birthDate.Value < DateTime.Today.AddYears(-120))
            {
                BirthDateError = "Некорректная дата рождения";
                isValid = false;
            }
            else
            {
                var minDate = DateTime.Today.AddYears(-14);
                if (_birthDate.Value > minDate)
                {
                    BirthDateError = "Клиент должен быть старше 14 лет";
                    isValid = false;
                }
            }

            CanSave = isValid;
        }

        private async Task SaveAsync()
        {
            if (!CanSave) return;

            try
            {
                var client = new Client
                {
                    Id = _editingClient?.Id ?? 0,
                    FirstName = FirstName?.Trim() ?? "",
                    LastName = LastName?.Trim() ?? "",
                    Email = Email?.Trim().ToLower() ?? "",
                    Phone = RawPhone,
                    Age = Age,
                    BirthDate = _birthDate,  // 👈 ДОБАВЛЯЕМ ДАТУ РОЖДЕНИЯ
                    IsActive = true
                };

                if (_editingClient == null)
                {
                    client.PlainPassword = Password;
                }

                Debug.WriteLine($"=== SAVE CLIENT DEBUG ===");
                Debug.WriteLine($"Id: {client.Id}");
                Debug.WriteLine($"Email: '{client.Email}'");
                Debug.WriteLine($"Phone (Raw): '{client.Phone}'");
                Debug.WriteLine($"BirthDate: {_birthDate?.ToShortDateString()}, Age: {client.Age}");
                Debug.WriteLine($"IsNew: {_editingClient == null}");
                if (_editingClient == null)
                {
                    Debug.WriteLine($"Password length: {Password?.Length ?? 0}");
                }
                Debug.WriteLine($"=========================");

                bool success;
                if (_editingClient != null)
                {
                    // 👇 ИСПРАВЛЕНО: используем один метод с передачей клиента целиком
                    success = await _clientService.UpdateAsync(client);
                }
                else
                {
                    success = await _clientService.CreateAsync(client);
                }

                if (success)
                {
                    MessageBox.Show(
                        _editingClient != null ? "Клиент успешно обновлен!" : "Клиент успешно создан!",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    _navigate(new ClientsView(_navigate));
                }
                else
                {
                    var error = _editingClient != null
                        ? "Ошибка при обновлении клиента! Возможно, email уже используется."
                        : "Ошибка при создании клиента! Возможно, email уже используется.";
                    MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel() => _navigate(new ClientsView(_navigate));

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}