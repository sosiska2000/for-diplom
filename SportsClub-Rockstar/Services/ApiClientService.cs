using Rockstar.Admin.WPF.Models;
using Rockstar.Admin.WPF.Services.DTOs;
using Rockstar.Admin.WPF.Services.Interfaces;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;

namespace Rockstar.Admin.WPF.Services
{
    public class ApiClientService : IClientService
    {
        private readonly IApiService _apiService;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiClientService(IApiService apiService)
        {
            _apiService = apiService;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<List<Client>> GetAllAsync()
        {
            try
            {
                var token = _apiService.GetAuthToken();
                if (string.IsNullOrEmpty(token))
                    return new List<Client>();

                var users = await _apiService.GetAsync<List<UserListDto>>("users");
                return users?.Select(MapToClient).ToList() ?? new List<Client>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in GetAllAsync: {ex.Message}");
                return new List<Client>();
            }
        }

        public async Task<bool> CreateAsync(Client client)
        {
            try
            {
                var registerRequest = new
                {
                    Email = client.Email,
                    Password = client.PlainPassword ?? "TempPass123!",
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    Phone = client.Phone,
                    Age = client.Age
                };

                var result = await _apiService.PostAsync<AuthResponse>("auth/register", registerRequest);
                return result?.Success == true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in CreateAsync: {ex.Message}");
                return false;
            }
        }



        public async Task<bool> UpdateAsync(Client client)
        {
            try
            {
                Debug.WriteLine($"📤 Updating client: {client.Id}, Email: '{client.Email}', BirthDate: {client.BirthDate?.ToShortDateString()}");

                var updateDto = new ClientUpdateDto
                {
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    Email = client.Email,
                    Phone = client.Phone,
                    Age = client.Age,
                    IsActive = client.IsActive,
                    BirthDate = client.BirthDate  
                };

                await _apiService.PutAsync<object>($"users/{client.Id}", updateDto);
                return true;
            }
            catch (HttpRequestException ex)
                when ((int?)ex.StatusCode is >= 400 and < 600)
            {
                Debug.WriteLine($"💥 HTTP Error in UpdateAsync: {ex.StatusCode}");
                return false;
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
                return await _apiService.DeleteAsync($"users/{id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in DeleteAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Client>> GetAllIncludingDeletedAsync()
        {
            try
            {
                var users = await _apiService.GetAsync<List<UserListDto>>("users?includeInactive=true");
                return users?.Select(MapToClient).ToList() ?? new List<Client>();
            }
            catch
            {
                return await GetAllAsync();
            }
        }

        private Client MapToClient(UserListDto dto)
        {
            return new Client
            {
                Id = dto.Id,
                Email = dto.Email ?? string.Empty,
                FirstName = dto.FirstName ?? string.Empty,
                LastName = dto.LastName ?? string.Empty,
                Phone = dto.Phone,
                Age = dto.Age,
                BirthDate = dto.BirthDate,
                IsActive = dto.IsActive,
                Role = dto.Role ?? "client",
                CreatedAt = dto.CreatedAt
            };
        }

        private class UserListDto
        {
            public int Id { get; set; }
            public string Email { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string? Phone { get; set; }
            public int? Age { get; set; }
            public DateTime? BirthDate { get; set; }
            public string Role { get; set; } = string.Empty;
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        private class AuthResponse
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
        }
    }
}