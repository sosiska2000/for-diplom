using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Rockstar.Admin.WPF.Models;

namespace Rockstar.Admin.WPF.Views.Directions
{
    public partial class EditServiceDialog : Window
    {
        private readonly Service? _editingService;

        public string ServiceName { get; private set; } = string.Empty;
        public decimal Price { get; private set; }
        public int SessionsCount { get; private set; }
        public int? DurationMinutes { get; private set; }
        public string Description { get; private set; } = string.Empty;

        public EditServiceDialog(Service? service)
        {
            InitializeComponent();

            _editingService = service;

            if (service != null)
            {
                Title = "Редактирование услуги";
                tbTitle.Text = "Редактирование услуги";
                tbName.Text = service.Name;
                tbPrice.Text = service.Price.ToString("F0");
                tbSessionsCount.Text = service.SessionsCount.ToString();
                tbDuration.Text = service.DurationMinutes?.ToString() ?? "";
                tbDescription.Text = service.Description ?? "";
            }
            else
            {
                Title = "Новая услуга";
                tbTitle.Text = "Новая услуга";
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                MessageBox.Show("Введите название услуги", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(tbPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(tbSessionsCount.Text, out int sessionsCount) || sessionsCount <= 0)
            {
                MessageBox.Show("Введите корректное количество занятий", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? duration = null;
            if (!string.IsNullOrWhiteSpace(tbDuration.Text))
            {
                if (!int.TryParse(tbDuration.Text, out int dur) || dur <= 0)
                {
                    MessageBox.Show("Введите корректную длительность", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                duration = dur;
            }

            ServiceName = tbName.Text.Trim();
            Price = price;
            SessionsCount = sessionsCount;
            DurationMinutes = duration;
            Description = tbDescription.Text.Trim();

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}