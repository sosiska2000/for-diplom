using Microsoft.Extensions.DependencyInjection;
using Rockstar.Admin.WPF.Models;
using Rockstar.Admin.WPF.Services.Interfaces;
using Rockstar.Admin.WPF.Views.Main;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Rockstar.Admin.WPF.Views.Directions
{
    public partial class DirectionsView : Page
    {
        private readonly Action<Page> _navigate;
        private readonly IDirectionService _directionService;
        private ObservableCollection<Direction> _customDirections = new();
        private Direction? _selectedDirection;

        public DirectionsView(Action<Page> navigate)
        {
            InitializeComponent();
            _navigate = navigate;
            _directionService = App.Services.GetRequiredService<IDirectionService>();
        }

        private void DirectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string directionKey)
            {
                var direction = _customDirections.FirstOrDefault(d => d.NameKey == directionKey);
                _selectedDirection = direction;
                _navigate(new DirectionDetailView(_navigate, directionKey));
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _navigate(new MainPage(_navigate));
        }
    }

    // Диалоговое окно для добавления направления (без поля "Ключ")
    public class AddDirectionDialog : Window
    {
        public string DirectionName { get; private set; } = string.Empty;
        public string DirectionKey { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;

        private TextBox tbName;
        private TextBox tbDescription;

        public AddDirectionDialog()
        {
            Title = "Добавить направление";
            Width = 450;
            Height = 300;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Owner = Application.Current.MainWindow;

            var mainGrid = new Grid { Margin = new Thickness(20) };
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Название
            mainGrid.Children.Add(new TextBlock
            {
                Text = "Название направления:",
                Margin = new Thickness(0, 0, 0, 5),
                FontWeight = FontWeights.SemiBold
            });
            Grid.SetRow(mainGrid.Children[^1], 0);

            tbName = new TextBox { Margin = new Thickness(0, 0, 0, 15), Height = 35 };
            mainGrid.Children.Add(tbName);
            Grid.SetRow(tbName, 1);

            // Описание
            mainGrid.Children.Add(new TextBlock
            {
                Text = "Описание:",
                Margin = new Thickness(0, 0, 0, 5),
                FontWeight = FontWeights.SemiBold
            });
            Grid.SetRow(mainGrid.Children[^1], 2);

            tbDescription = new TextBox
            {
                Margin = new Thickness(0, 0, 0, 20),
                Height = 80,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            mainGrid.Children.Add(tbDescription);
            Grid.SetRow(tbDescription, 3);

            // Кнопки
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var btnOk = new Button
            {
                Content = "Добавить",
                Width = 100,
                Height = 35,
                Margin = new Thickness(0, 0, 10, 0),
                Background = System.Windows.Media.Brushes.OrangeRed,
                Foreground = System.Windows.Media.Brushes.White,
                Cursor = Cursors.Hand
            };
            btnOk.Click += BtnOk_Click;

            var btnCancel = new Button
            {
                Content = "Отмена",
                Width = 100,
                Height = 35,
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => DialogResult = false;

            buttonPanel.Children.Add(btnOk);
            buttonPanel.Children.Add(btnCancel);

            mainGrid.Children.Add(buttonPanel);
            Grid.SetRow(buttonPanel, 4);

            Content = mainGrid;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                MessageBox.Show("Введите название направления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Генерируем ключ автоматически из названия
            DirectionName = tbName.Text.Trim();
            DirectionKey = DirectionName.ToLower().Replace(" ", "_");
            Description = tbDescription.Text.Trim();

            DialogResult = true;
        }
    }
}