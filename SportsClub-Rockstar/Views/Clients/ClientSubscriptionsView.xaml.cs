using Rockstar.Admin.WPF.Services.Interfaces;
using Rockstar.Admin.WPF.ViewModels.Clients;
using System.Windows;
using System.Windows.Controls;

namespace Rockstar.Admin.WPF.Views.Clients
{
    public partial class ClientSubscriptionsView : Page
    {
        private readonly ClientSubscriptionsViewModel _viewModel;
        private readonly Action? _onClose;  

        public ClientSubscriptionsView(ISubscriptionService subscriptionService, Models.Client client, Action? onClose = null)
        {
            InitializeComponent();
            _onClose = onClose;
            _viewModel = new ClientSubscriptionsViewModel(subscriptionService, client, onClose);
            DataContext = _viewModel;
        }
        private void OnBackClicked(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
            else
            {
                _onClose?.Invoke();
            }
        }
    }
}