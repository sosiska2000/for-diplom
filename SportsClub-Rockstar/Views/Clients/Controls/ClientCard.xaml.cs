using Rockstar.Admin.WPF.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Rockstar.Admin.WPF.Views.Clients.Controls
{
    public partial class ClientCard : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ClientProperty =
            DependencyProperty.Register(nameof(Client), typeof(Client), typeof(ClientCard),
                new PropertyMetadata(null, OnClientChanged));

        public static readonly DependencyProperty EditCommandProperty =
            DependencyProperty.Register(nameof(EditCommand), typeof(ICommand), typeof(ClientCard),
                new PropertyMetadata(null));

        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register(nameof(DeleteCommand), typeof(ICommand), typeof(ClientCard),
                new PropertyMetadata(null));

        public ClientCard()
        {
            InitializeComponent();
        }

        public Client Client
        {
            get => (Client)GetValue(ClientProperty);
            set => SetValue(ClientProperty, value);
        }

        public ICommand EditCommand
        {
            get => (ICommand)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }

        public ICommand DeleteCommand
        {
            get => (ICommand)GetValue(DeleteCommandProperty);
            set => SetValue(DeleteCommandProperty, value);
        }

        public string FullName => Client?.FullName ?? "Нет данных";
        public string Email => Client?.Email ?? "Email не указан";
        public string PhoneText => !string.IsNullOrWhiteSpace(Client?.Phone) ? $"📞 {Client.Phone}" : "📞 Не указан";
        public string AgeText => Client?.Age != null ? $"📅 {Client.Age} лет" : "📅 Возраст не указан";

        // Инициалы для аватара
        public string Initials
        {
            get
            {
                if (Client == null) return "?";
                string firstName = Client.FirstName?.Trim() ?? "";
                string lastName = Client.LastName?.Trim() ?? "";

                if (firstName.Length > 0 && lastName.Length > 0)
                    return $"{firstName[0]}{lastName[0]}".ToUpper();
                if (firstName.Length > 0)
                    return firstName[0].ToString().ToUpper();
                if (lastName.Length > 0)
                    return lastName[0].ToString().ToUpper();
                return "?";
            }
        }

        private static void OnClientChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ClientCard card)
            {
                card.OnPropertyChanged(nameof(FullName));
                card.OnPropertyChanged(nameof(Email));
                card.OnPropertyChanged(nameof(PhoneText));
                card.OnPropertyChanged(nameof(AgeText));
                card.OnPropertyChanged(nameof(Initials));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}