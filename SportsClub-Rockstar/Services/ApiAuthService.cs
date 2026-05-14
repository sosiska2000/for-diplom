using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Rockstar.Admin.WPF.Models;
using Rockstar.Admin.WPF.Services.Interfaces;

namespace Rockstar.Admin.WPF.Services
{
    public class ApiAuthService : IAuthService
    {
        private readonly IApiService _apiService;
        private Client? _currentUser;
        private bool _isAuthenticated;
        private string? _token;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiAuthService(IApiService apiService)
        {
            _apiService = apiService;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public bool IsAuthenticated => _isAuthenticated;
        public Client? CurrentUser => _currentUser;
        public string? Token => _token;

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                var loginRequest = new { email, password };
                var authResponse = await _apiService.PostAsync<AuthResponse>("auth/login", loginRequest);

                if (authResponse == null || !authResponse.Success)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = authResponse?.Message ?? "Ошибка авторизации"
                    };
                }

                _token = authResponse.Token;
                _isAuthenticated = true;

                if (!string.IsNullOrEmpty(_token))
                {
                    _apiService.SetAuthToken(_token);
                }

                if (authResponse.User != null)
                {
                    _currentUser = new Client
                    {
                        Id = authResponse.User.Id,
                        Email = authResponse.User.Email,
                        FirstName = authResponse.User.FirstName,
                        LastName = authResponse.User.LastName,
                        Phone = authResponse.User.Phone,
                        Age = authResponse.User.Age,
                        Role = authResponse.User.Role
                    };

                }

                return new AuthResult
                {
                    Success = true,
                    Token = _token,
                    User = _currentUser
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Login error: {ex.Message}");
                return new AuthResult
                {
                    Success = false,
                    Message = "Ошибка при входе в систему"
                };
            }
        }

        public Task LogoutAsync()
        {
            _currentUser = null;
            _isAuthenticated = false;
            _token = null;
            _apiService.SetAuthToken(null);
            return Task.CompletedTask;
        }

        private class AuthResponse
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public string? Token { get; set; }
            public UserDto? User { get; set; }
        }

        private class UserDto
        {
            public int Id { get; set; }
            public string Email { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string? Phone { get; set; }
            public int? Age { get; set; }
            public string Role { get; set; } = string.Empty;
        }
    }
}