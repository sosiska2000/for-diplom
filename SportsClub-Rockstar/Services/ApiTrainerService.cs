using Rockstar.Admin.WPF.Models;
using Rockstar.Admin.WPF.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rockstar.Admin.WPF.Services
{
    public class ApiTrainerService : ITrainerService
    {
        private readonly IApiService _apiService;

        public ApiTrainerService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<Trainer>> GetAllAsync()
        {
            try
            {
                var trainersDto = await _apiService.GetAsync<List<TrainerApiDto>>("trainers");
                if (trainersDto == null) return new List<Trainer>();

                Debug.WriteLine($"📥 Получено {trainersDto.Count} тренеров с API");
                var trainers = new List<Trainer>();

                foreach (var dto in trainersDto)
                {
                    var trainer = MapToTrainer(dto);
                    trainers.Add(trainer);
                    Debug.WriteLine($"   Trainer {trainer.Id}: {trainer.FullName}, Directions: {trainer.DirectionsDisplay}");
                }

                return trainers;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in GetAllAsync: {ex.Message}");
                return new List<Trainer>();
            }
        }

        public async Task<Trainer?> GetByIdAsync(int id)
        {
            try
            {
                var trainerDto = await _apiService.GetAsync<TrainerApiDto>($"trainers/{id}");
                return trainerDto != null ? MapToTrainer(trainerDto) : null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<Trainer?> GetByEmailAsync(string email)
        {
            try
            {
                var trainerDto = await _apiService.GetAsync<TrainerApiDto>($"trainers/email/{email}");
                return trainerDto != null ? MapToTrainer(trainerDto) : null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> CreateAsync(Trainer trainer)
        {
            try
            {
                var createDto = new
                {
                    trainer.FirstName,
                    trainer.LastName,
                    DirectionIds = trainer.SelectedDirections.Select(d => d.Id).ToList(),
                    Experience = trainer.ExperienceYears,
                    Description = trainer.Description ?? string.Empty,
                    PhotoBase64 = trainer.Photo != null && trainer.Photo.Length > 0
                        ? Convert.ToBase64String(trainer.Photo)
                        : null
                };

                Debug.WriteLine($"📤 Create DTO: {JsonSerializer.Serialize(createDto)}");

                var result = await _apiService.PostAsync<TrainerApiDto>("trainers", createDto);
                return result != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in CreateAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Trainer trainer)
        {
            try
            {
                var updateDto = new
                {
                    trainer.FirstName,
                    trainer.LastName,
                    DirectionIds = trainer.SelectedDirections.Select(d => d.Id).ToList(),
                    Experience = trainer.ExperienceYears,
                    Description = trainer.Description ?? string.Empty,
                    PhotoBase64 = trainer.Photo != null && trainer.Photo.Length > 0
                        ? Convert.ToBase64String(trainer.Photo)
                        : null
                };

                Debug.WriteLine($"📤 Update DTO: {JsonSerializer.Serialize(updateDto)}");

                var result = await _apiService.PutAsync<TrainerApiDto>($"trainers/{trainer.Id}", updateDto);
                return result != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in UpdateAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                return await _apiService.DeleteAsync($"trainers/{id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in DeleteAsync: {ex.Message}");
                return false;
            }
        }

        public List<string> GetAvailableDirections() => new List<string> { "yoga", "fitness", "climbing" };

        private Trainer MapToTrainer(TrainerApiDto dto)
        {
            Debug.WriteLine($"🔄 Mapping Trainer {dto.Id}: Name={dto.FirstName} {dto.LastName}");
            Debug.WriteLine($"   Experience={dto.Experience}, Description={dto.Description}");
            Debug.WriteLine($"   Directions count from API: {dto.Directions?.Count ?? 0}");

            var trainer = new Trainer
            {
                Id = dto.Id,
                FirstName = dto.FirstName ?? string.Empty,
                LastName = dto.LastName ?? string.Empty,
                Description = dto.Description ?? string.Empty,
                IsActive = dto.IsActive,
                CreatedAt = dto.CreatedAt
            };

            // Фото
            if (!string.IsNullOrEmpty(dto.PhotoBase64))
            {
                try
                {
                    trainer.Photo = Convert.FromBase64String(dto.PhotoBase64);
                    Debug.WriteLine($"   Photo loaded: {trainer.Photo.Length} bytes");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"⚠️ Photo decode failed: {ex.Message}");
                }
            }

            // Рассчитываем дату начала работы из стажа
            if (dto.Experience > 0)
            {
                trainer.EmploymentDate = DateTime.Today.AddYears(-dto.Experience);
                Debug.WriteLine($"   EmploymentDate calculated: {trainer.EmploymentDate:yyyy-MM-dd}");
            }

            // 👇 ВАЖНО: загружаем ВСЕ направления из свойства Directions (НЕ DirectionId!)
            if (dto.Directions != null && dto.Directions.Any())
            {
                trainer.SelectedDirections.Clear();
                foreach (var dir in dto.Directions)
                {
                    trainer.SelectedDirections.Add(new Direction
                    {
                        Id = dir.Id,
                        Name = dir.Name,
                        NameKey = dir.NameKey
                    });
                    Debug.WriteLine($"   Direction added: {dir.Name} (ID={dir.Id})");
                }

                // Для обратной совместимости устанавливаем первое направление
                var firstDir = dto.Directions.First();
                trainer.DirectionId = firstDir.Id;
                trainer.DirectionName = firstDir.Name;
                trainer.DirectionKey = firstDir.NameKey;
            }
            else if (dto.DirectionId.HasValue && !string.IsNullOrWhiteSpace(dto.DirectionName))
            {
                // Fallback для старых данных
                trainer.SelectedDirections.Add(new Direction
                {
                    Id = dto.DirectionId.Value,
                    Name = dto.DirectionName,
                    NameKey = dto.DirectionKey ?? string.Empty
                });
                Debug.WriteLine($"   Direction added (fallback): {dto.DirectionName}");
            }

            Debug.WriteLine($"   Final DirectionsDisplay: {trainer.DirectionsDisplay}");
            return trainer;
        }

        // DTO для десериализации из API
        private class TrainerApiDto
        {
            [JsonPropertyName("id")] public int Id { get; set; }
            [JsonPropertyName("firstName")] public string FirstName { get; set; } = string.Empty;
            [JsonPropertyName("lastName")] public string LastName { get; set; } = string.Empty;
            [JsonPropertyName("description")] public string? Description { get; set; }
            [JsonPropertyName("photoBase64")] public string? PhotoBase64 { get; set; }
            [JsonPropertyName("isActive")] public bool IsActive { get; set; }
            [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }
            [JsonPropertyName("experience")] public int Experience { get; set; }

            // Старые поля (для обратной совместимости)
            [JsonPropertyName("directionId")] public int? DirectionId { get; set; }
            [JsonPropertyName("directionName")] public string DirectionName { get; set; } = string.Empty;
            [JsonPropertyName("directionKey")] public string DirectionKey { get; set; } = string.Empty;

            // 👇 НОВОЕ: список всех направлений
            [JsonPropertyName("directions")] public List<DirectionDto>? Directions { get; set; }
        }

        private class DirectionDto
        {
            [JsonPropertyName("id")] public int Id { get; set; }
            [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
            [JsonPropertyName("nameKey")] public string NameKey { get; set; } = string.Empty;
        }
    }
}