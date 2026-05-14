using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Rockstar.Admin.WPF.Models
{
    public class Trainer
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        // Старые поля для совместимости с API
        public int? DirectionId { get; set; }
        public string DirectionName { get; set; } = string.Empty;
        public string DirectionKey { get; set; } = string.Empty;

        // Направления тренера
        public ObservableCollection<Direction> SelectedDirections { get; set; } = new();

        // 👇 УДАЛЯЕМ Email и Password (тренер не должен иметь логин/пароль)
        // public string? Email { get; set; }
        // public string? PasswordHash { get; set; }

        public byte[]? Photo { get; set; }

        // 👇 НОВОЕ: дата начала работы
        private DateTime? _employmentDate;
        public DateTime? EmploymentDate
        {
            get => _employmentDate;
            set
            {
                _employmentDate = value;
                OnPropertyChanged(nameof(EmploymentDate));
                OnPropertyChanged(nameof(ExperienceDisplay));
            }
        }

        // Стаж в годах и месяцах (вычисляемое поле)
        public string ExperienceDisplay
        {
            get
            {
                if (!_employmentDate.HasValue)
                    return "Стаж не указан";

                var today = DateTime.Today;
                var years = today.Year - _employmentDate.Value.Year;
                var months = today.Month - _employmentDate.Value.Month;

                if (months < 0)
                {
                    years--;
                    months += 12;
                }

                if (years == 0 && months == 0)
                    return "Менее месяца";

                if (years == 0)
                    return $"{months} мес.";

                if (months == 0)
                    return $"{years} {GetYearWord(years)}";

                return $"{years} {GetYearWord(years)} {months} мес.";
            }
        }

        private string GetYearWord(int years)
        {
            if (years % 10 == 1 && years % 100 != 11)
                return "год";
            if (years % 10 >= 2 && years % 10 <= 4 && (years % 100 < 10 || years % 100 >= 20))
                return "года";
            return "лет";
        }

        // Стаж в годах (для API)
        public int ExperienceYears
        {
            get
            {
                if (!_employmentDate.HasValue) return 0;
                var today = DateTime.Today;
                var years = today.Year - _employmentDate.Value.Year;
                if (_employmentDate.Value.Date > today.AddYears(-years)) years--;
                return years;
            }
        }

        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        public string FullName => $"{LastName} {FirstName}".Trim();

        public string DirectionsDisplay
        {
            get
            {
                if (SelectedDirections == null || SelectedDirections.Count == 0)
                    return "Направления не указаны";
                return string.Join(", ", SelectedDirections.Select(d => d.Name));
            }
        }

        public Direction? PrimaryDirection => SelectedDirections.FirstOrDefault();

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
}