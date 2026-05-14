using Microsoft.Extensions.DependencyInjection;
using Rockstar.Admin.WPF.Models;
using Rockstar.Admin.WPF.Services.Interfaces;
using Rockstar.Admin.WPF.ViewModels.Schedule;
using Rockstar.Admin.WPF.Views.Main;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

// 👇 Добавьте алиас для класса Schedule, чтобы избежать конфликта с пространством имён
using ScheduleModel = Rockstar.Admin.WPF.Models.Schedule;

namespace Rockstar.Admin.WPF.Views.Schedule
{
    public partial class ScheduleHistoryView : Page, INotifyPropertyChanged
    {
        private readonly Action<Page> _navigate;
        private readonly IScheduleService _scheduleService;
        private ObservableCollection<ScheduleModel> _allHistory = new();
        private ObservableCollection<ScheduleModel> _filteredHistory = new();
        private string _searchText = string.Empty;

        public ScheduleHistoryView(Action<Page> navigate)
        {
            InitializeComponent();
            _navigate = navigate;
            _scheduleService = App.Services.GetRequiredService<IScheduleService>();

            DataContext = this;
            LoadHistory();
        }

        public ObservableCollection<ScheduleModel> HistoryItems
        {
            get => _filteredHistory;
            set
            {
                _filteredHistory = value;
                OnPropertyChanged(nameof(HistoryItems));
                OnPropertyChanged(nameof(HistoryCount));
            }
        }

        public int HistoryCount => HistoryItems?.Count ?? 0;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                ApplyFilters();
            }
        }

        private async void LoadHistory()
        {
            try
            {
                var history = await _scheduleService.GetHistoryAsync();
                _allHistory = new ObservableCollection<ScheduleModel>(history.OrderByDescending(s => s.DateTime));
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки истории: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters()
        {
            var filtered = _allHistory.AsEnumerable();

            // Фильтр по дате
            if (dpFromDate.SelectedDate.HasValue)
            {
                var fromDate = dpFromDate.SelectedDate.Value.Date;
                filtered = filtered.Where(s => s.DateTime.Date >= fromDate);
            }
            if (dpToDate.SelectedDate.HasValue)
            {
                var toDate = dpToDate.SelectedDate.Value.Date.AddDays(1);
                filtered = filtered.Where(s => s.DateTime.Date <= toDate);
            }

            // Поиск
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(s =>
                    s.DirectionName.ToLower().Contains(searchLower) ||
                    s.ServiceName.ToLower().Contains(searchLower) ||
                    s.TrainerName.ToLower().Contains(searchLower));
            }

            HistoryItems = new ObservableCollection<ScheduleModel>(filtered);
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchText = tbSearch.Text;
        }

        private void ScheduleCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is int scheduleId)
            {
                _navigate(new ScheduleDetailsView(_navigate, scheduleId));
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _navigate(new ScheduleView(_navigate));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
