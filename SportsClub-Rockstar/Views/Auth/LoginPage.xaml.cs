using Microsoft.Extensions.DependencyInjection;
using Rockstar.Admin.WPF.Services.Interfaces;
using Rockstar.Admin.WPF.ViewModels.Auth;
using System;
using System.Diagnostics;
using System.Windows.Controls;

namespace Rockstar.Admin.WPF.Views.Auth
{
    public partial class LoginPage : Page
    {
        private readonly LoginViewModel _viewModel;

        public LoginPage(IServiceProvider services, Action<Page> navigate)
        {
            try
            {
                InitializeComponent();
                var authService = services.GetRequiredService<IAuthService>();
                _viewModel = new LoginViewModel(authService, navigate);
                DataContext = _viewModel;
                PasswordBox.Password = _viewModel.Password;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoginPage error: {ex.Message}");
                throw;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
                vm.Password = ((PasswordBox)sender).Password;
        }
    }
}