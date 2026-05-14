using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Rockstar.Admin.WPF.Models;

namespace Rockstar.Admin.WPF.Views.Directions
{
    public partial class EditServiceTypeDialog : Window
    {
        private readonly ServiceType? _editingServiceType;

        public string ServiceTypeName { get; private set; } = string.Empty;
        public int DefaultDuration { get; private set; } = 60;
        public string Description { get; private set; } = string.Empty;

        public EditServiceTypeDialog(ServiceType? serviceType)
        {
            InitializeComponent();

            _editingServiceType = serviceType;

            if (serviceType != null)
            {
                Title = "Редактирование вида занятий";
                tbTitle.Text = "Редактирование вида занятий";
                tbName.Text = serviceType.Name;
                tbDefaultDuration.Text = serviceType.DefaultDuration.ToString();
                tbDescription.Text = serviceType.Description ?? "";
            }
            else
            {
                Title = "Новый вид занятий";
                tbTitle.Text = "Новый вид занятий";
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                MessageBox.Show("Введите название вида занятий", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(tbDefaultDuration.Text, out int duration) || duration <= 0)
            {
                MessageBox.Show("Введите корректную длительность", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ServiceTypeName = tbName.Text.Trim();
            DefaultDuration = duration;
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