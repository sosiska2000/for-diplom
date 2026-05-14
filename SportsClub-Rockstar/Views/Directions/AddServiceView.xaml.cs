using Microsoft.Extensions.DependencyInjection;
using Rockstar.Admin.WPF.Services.Interfaces;
using Rockstar.Admin.WPF.ViewModels.Directions;
using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace Rockstar.Admin.WPF.Views.Directions
{
    public partial class AddServiceView : Page
    {
        public AddServiceView(
            Action<Page> navigate,
            Models.Service? service,
            int directionId,
            string directionName,
            string directionKey)  // ✅ ДОБАВЛЕНО
        {
            InitializeComponent();

            var serviceService = App.Services.GetRequiredService<IServiceService>();

            var viewModel = new AddServiceViewModel(
                serviceService,
                navigate,
                service,
                directionId,
                directionName,
                directionKey);  // ✅ ПЕРЕДАЁМ directionKey

            DataContext = viewModel;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}