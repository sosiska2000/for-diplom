using Microsoft.Extensions.DependencyInjection;
using Rockstar.Admin.WPF.Services.Interfaces;
using Rockstar.Admin.WPF.ViewModels.Clients;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Rockstar.Admin.WPF.Views.Clients
{
    public partial class AddClientView : Page
    {
        private readonly AddClientViewModel _viewModel;
        private bool _isUpdatingText = false;

        public AddClientView(Action<Page> navigate, Models.Client? client)
        {
            InitializeComponent();

            var clientService = App.Services.GetRequiredService<IClientService>();
            _viewModel = new AddClientViewModel(clientService, navigate, client);
            DataContext = _viewModel;

            if (!string.IsNullOrEmpty(_viewModel.Password))
            {
                PasswordBox.Password = _viewModel.Password;
            }

            if (!string.IsNullOrEmpty(_viewModel.Phone))
            {
                _isUpdatingText = true;
                PhoneTextBox.Text = FormatPhoneForDisplay(_viewModel.Phone);
                _isUpdatingText = false;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.Password = PasswordBox.Password;
            }
        }

        // Разрешаем ввод только цифр
        private void PhoneTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d+$");
        }

        private void PhoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingText) return;

            var textBox = sender as TextBox;
            if (textBox == null) return;

            // Сохраняем позицию курсора до изменений
            int cursorPosition = textBox.SelectionStart;
            string oldText = textBox.Text;

            // Извлекаем только цифры из текущего текста
            string digits = Regex.Replace(oldText, @"[^\d]", "");

            // Ограничиваем 11 цифрами
            if (digits.Length > 11)
            {
                digits = digits.Substring(0, 11);
            }

            // Форматируем с + в начале
            string newText = "+" + digits;

            // Если текст не изменился, ничего не делаем
            if (oldText == newText) return;

            // Вычисляем новую позицию курсора
            int newCursorPos = CalculateNewCursorPosition(oldText, newText, cursorPosition);

            // Обновляем текст
            _isUpdatingText = true;
            textBox.Text = newText;
            textBox.SelectionStart = newCursorPos;
            _isUpdatingText = false;

            // Обновляем ViewModel (только цифры, без +)
            string phoneDigits = digits;
            if (_viewModel.Phone != phoneDigits)
            {
                _viewModel.Phone = phoneDigits;
            }
        }

        private int CalculateNewCursorPosition(string oldText, string newText, int oldCursorPos)
        {
            // Если курсор в конце, оставляем в конце
            if (oldCursorPos >= oldText.Length)
                return newText.Length;

            // Получаем символ на позиции курсора в старом тексте
            char oldChar = oldCursorPos < oldText.Length ? oldText[oldCursorPos] : '\0';

            // Если символ - цифра, ищем её позицию в новом тексте
            if (char.IsDigit(oldChar))
            {
                // Считаем, какая по счёту это цифра
                int digitIndex = 0;
                for (int i = 0; i < oldCursorPos; i++)
                {
                    if (char.IsDigit(oldText[i]))
                        digitIndex++;
                }

                // Ищем такую же по счёту цифру в новом тексте
                int foundDigit = 0;
                for (int i = 0; i < newText.Length; i++)
                {
                    if (char.IsDigit(newText[i]))
                    {
                        if (foundDigit == digitIndex)
                            return i;
                        foundDigit++;
                    }
                }
                return newText.Length;
            }

            // Если символ не цифра (например, +), ищем позицию после него
            if (oldChar == '+')
            {
                return 1; // Позиция после +
            }

            return oldCursorPos;
        }

        private string FormatPhoneForDisplay(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return string.Empty;

            string digits = Regex.Replace(phone, @"[^\d]", "");
            if (digits.Length > 11)
                digits = digits.Substring(0, 11);

            return "+" + digits;
        }
    }
}