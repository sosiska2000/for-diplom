using Rockstar.Admin.WPF.Models;
using Rockstar.Admin.WPF.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Rockstar.Admin.WPF.Services
{
    public class ApiScheduleService : IScheduleService
    {
        private readonly IApiService _apiService;

        public ApiScheduleService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<Schedule>> GetGroupSchedulesAsync()
        {
            try
            {
                return await _apiService.GetAsync<List<Schedule>>("schedule/group") ?? new List<Schedule>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error: {ex.Message}");
                return new List<Schedule>();
            }
        }

        public async Task<Schedule?> GetScheduleByIdAsync(int id)
        {
            try
            {
                return await _apiService.GetAsync<Schedule>($"schedule/{id}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Schedule>> GetHistoryAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var url = "schedule/history";
                var queryParams = new List<string>();

                if (fromDate.HasValue)
                    queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                if (toDate.HasValue)
                    queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");

                if (queryParams.Any())
                    url += "?" + string.Join("&", queryParams);

                Debug.WriteLine($"📥 Fetching history from: {url}");
                return await _apiService.GetAsync<List<Schedule>>(url) ?? new List<Schedule>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in GetHistoryAsync: {ex.Message}");
                return new List<Schedule>();
            }
        }

        public async Task<List<Schedule>> GetAllSchedulesAsync()
        {
            try
            {
                Debug.WriteLine("📥 Fetching ALL schedules (including past)");
                return await _apiService.GetAsync<List<Schedule>>("schedule") ?? new List<Schedule>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in GetAllSchedulesAsync: {ex.Message}");
                return new List<Schedule>();
            }
        }

        public async Task<bool> CreateScheduleAsync(Schedule schedule)
        {
            try
            {
                var createDto = new
                {
                    schedule.TrainerId,
                    schedule.DirectionId,
                    schedule.ServiceId,
                    schedule.DateTime,
                    schedule.DurationMinutes,
                    schedule.MaxParticipants,
                    schedule.Price,
                    IsGroup = true
                };

                var result = await _apiService.PostAsync<Schedule>("schedule", createDto);
                return result != null;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"💥 HttpRequestException: {ex.Message}");

                string errorMessage = ParseErrorMessage(ex.Message);

                // Показываем красивое сообщение
                ShowErrorDialog(errorMessage, "schedule");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Exception: {ex.Message}");
                ShowErrorDialog(ex.Message, "schedule");
                return false;
            }
        }

        public async Task<bool> UpdateScheduleAsync(Schedule schedule)
        {
            try
            {
                var updateDto = new
                {
                    schedule.TrainerId,
                    schedule.DirectionId,
                    schedule.ServiceId,
                    schedule.DateTime,
                    schedule.DurationMinutes,
                    schedule.MaxParticipants,
                    schedule.Price,
                    IsGroup = true
                };

                var result = await _apiService.PutAsync<Schedule>($"schedule/{schedule.Id}", updateDto);
                return result != null;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"💥 HttpRequestException: {ex.Message}");

                string errorMessage = ParseErrorMessage(ex.Message);

                ShowErrorDialog(errorMessage, "schedule");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Exception: {ex.Message}");
                ShowErrorDialog(ex.Message, "schedule");
                return false;
            }
        }

        public async Task<bool> DeleteScheduleAsync(int id)
        {
            try
            {
                return await _apiService.DeleteAsync($"schedule/{id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error: {ex.Message}");
                ShowErrorDialog(ex.Message, "delete");
                return false;
            }
        }

        public async Task<List<Enrollment>> GetEnrollmentsByScheduleIdAsync(int scheduleId)
        {
            try
            {
                return await _apiService.GetAsync<List<Enrollment>>($"schedule/{scheduleId}/enrollments")
                       ?? new List<Enrollment>();
            }
            catch
            {
                return new List<Enrollment>();
            }
        }

        public async Task<List<Client>> GetAvailableClientsAsync(int scheduleId)
        {
            try
            {
                return await _apiService.GetAsync<List<Client>>($"schedule/{scheduleId}/available-clients")
                       ?? new List<Client>();
            }
            catch
            {
                return new List<Client>();
            }
        }

        public async Task<bool> AddClientToScheduleAsync(int scheduleId, int userId)
        {
            try
            {
                var result = await _apiService.PostAsync<object>($"schedule/{scheduleId}/add-client/{userId}", null);
                return result != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error: {ex.Message}");
                ShowErrorDialog(ex.Message, "client");
                return false;
            }
        }

        public async Task<bool> RemoveClientFromScheduleAsync(int enrollmentId)
        {
            try
            {
                return await _apiService.DeleteAsync($"enrollments/{enrollmentId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Direction>> GetDirectionsAsync()
        {
            try
            {
                return await _apiService.GetAsync<List<Direction>>("directions") ?? new List<Direction>();
            }
            catch
            {
                return new List<Direction>();
            }
        }

        public async Task<List<Service>> GetServicesByDirectionAsync(int directionId)
        {
            try
            {
                return await _apiService.GetAsync<List<Service>>($"directions/{directionId}/services")
                       ?? new List<Service>();
            }
            catch
            {
                return new List<Service>();
            }
        }

        public async Task<List<Trainer>> GetTrainersAsync()
        {
            try
            {
                return await _apiService.GetAsync<List<Trainer>>("trainers") ?? new List<Trainer>();
            }
            catch
            {
                return new List<Trainer>();
            }
        }

        // 👇 ПАРСИМ ОШИБКУ ОТ СЕРВЕРА
        private string ParseErrorMessage(string error)
        {
            if (string.IsNullOrEmpty(error))
                return "Неизвестная ошибка";

            // Пытаемся распарсить JSON
            try
            {
                if (error.Contains("{"))
                {
                    using var doc = JsonDocument.Parse(error);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("message", out var message))
                        return message.GetString() ?? error;
                    if (root.TryGetProperty("error", out var errorProp))
                        return errorProp.GetString() ?? error;
                    if (root.TryGetProperty("title", out var title))
                        return title.GetString() ?? error;
                }
            }
            catch { }

            // Проверяем на русские сообщения
            if (error.Contains("тренер") && error.Contains("занят"))
                return "У тренера уже есть занятие в это время.\nПожалуйста, выберите другое время.";

            if (error.Contains("already") && error.Contains("busy"))
                return "У тренера уже есть занятие в это время.\nПожалуйста, выберите другое время.";

            if (error.Contains("not found"))
                return "Запрашиваемые данные не найдены.";

            if (error.Contains("unauthorized") || error.Contains("401"))
                return "Недостаточно прав для выполнения операции.\nПожалуйста, войдите в систему заново.";

            // Убираем технические детали из сообщения
            if (error.Contains("Ошибка API:"))
            {
                var parts = error.Split(" - ");
                if (parts.Length > 1)
                    return parts[1];
            }

            return error;
        }

        // 👇 КРАСИВЫЙ ДИАЛОГ ОШИБКИ
        private void ShowErrorDialog(string message, string context)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var dialog = new Window
                {
                    Title = "Ошибка",
                    Width = 450,
                    Height = 280,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize,
                    Background = System.Windows.Media.Brushes.White,
                    WindowStyle = WindowStyle.None,
                    AllowsTransparency = true,
                    BorderBrush = System.Windows.Media.Brushes.LightGray,
                    BorderThickness = new Thickness(1)
                };

                var grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.Margin = new Thickness(20);

                // Заголовок с иконкой
                var titlePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 15) };

                var icon = new TextBlock
                {
                    Text = "⚠️",
                    FontSize = 28,
                    Margin = new Thickness(0, 0, 15, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                var titleText = new TextBlock
                {
                    Text = context == "schedule" ? "Не удалось сохранить занятие" : "Операция не выполнена",
                    FontSize = 18,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = System.Windows.Media.Brushes.DarkRed,
                    VerticalAlignment = VerticalAlignment.Center
                };

                titlePanel.Children.Add(icon);
                titlePanel.Children.Add(titleText);

                Grid.SetRow(titlePanel, 0);
                grid.Children.Add(titlePanel);

                // Сообщение
                var messageText = new TextBlock
                {
                    Text = message,
                    FontSize = 14,
                    Foreground = System.Windows.Media.Brushes.Black,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 20)
                };
                Grid.SetRow(messageText, 1);
                grid.Children.Add(messageText);

                // Кнопка OK
                var okButton = new Button
                {
                    Content = "Понятно",
                    Width = 120,
                    Height = 35,
                    FontSize = 14,
                    Background = System.Windows.Media.Brushes.DarkRed,
                    Foreground = System.Windows.Media.Brushes.White,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                okButton.Click += (s, e) => dialog.Close();

                Grid.SetRow(okButton, 2);
                grid.Children.Add(okButton);

                dialog.Content = grid;
                dialog.ShowDialog();
            });
        }
    }
}