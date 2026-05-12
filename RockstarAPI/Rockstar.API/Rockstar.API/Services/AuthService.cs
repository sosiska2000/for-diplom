using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Rockstar.API.Data;
using Rockstar.API.DTOs.Auth;
using Rockstar.API.Models;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Rockstar.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> RefreshTokenAsync(string token);
        Task<bool> IsEmailAvailableAsync(string email);
    }

    public class AuthService : IAuthService
    {
        private readonly RockstarContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(RockstarContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", request.Email);

                var user = await _context.Users
                    .Where(u => u.Email == request.Email && u.IsActive)
                    .Select(u => new
                    {
                        u.Id,
                        u.Email,
                        u.FirstName,
                        u.LastName,
                        u.Phone,
                        u.Age,
                        u.Role,
                        u.PasswordHash
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    _logger.LogWarning("User not found: {Email}", request.Email);
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Неверный email или пароль"
                    };
                }

                // Проверка пароля
                bool passwordValid = false;
                try
                {
                    passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
                }
                catch
                {
                    passwordValid = (user.PasswordHash == request.Password);
                }

                if (!passwordValid)
                {
                    _logger.LogWarning("Invalid password for: {Email}", request.Email);
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Неверный email или пароль"
                    };
                }

                // Генерация токена
                var token = GenerateJwtToken(user.Id, user.Email, user.Role);

                _logger.LogInformation("Login successful for: {Email}", request.Email);

                return new AuthResponse
                {
                    Success = true,
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Phone = user.Phone,
                        Age = user.Age,
                        Role = user.Role
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for {Email}", request.Email);
                return new AuthResponse
                {
                    Success = false,
                    Message = "Ошибка при входе в систему"
                };
            }
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("Register attempt for email: {Email}", request.Email);

                // Проверка существования пользователя
                var existingUser = await _context.Users.AnyAsync(u => u.Email == request.Email);
                if (existingUser)
                {
                    return new AuthResponse { Success = false, Message = "Пользователь с таким email уже существует" };
                }

                // Создание нового пользователя
                var user = new User
                {
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Phone = request.Phone,
                    Age = request.Age,
                    Role = "client",
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Генерация токена
                var token = GenerateJwtToken(user.Id, user.Email, user.Role);

                return new AuthResponse
                {
                    Success = true,
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Phone = user.Phone,
                        Age = user.Age,
                        Role = user.Role
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register error for {Email}", request.Email);
                return new AuthResponse { Success = false, Message = "Ошибка при регистрации" };
            }
        }

        public async Task<AuthResponse> RefreshTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ??
                    "default-key-which-should-be-changed-in-production-1234567890");

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = false
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                var userId = principal.FindFirst("id")?.Value;

                if (userId == null)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Недействительный токен"
                    };
                }

                var user = await _context.Users.FindAsync(int.Parse(userId));
                if (user == null || !user.IsActive)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Пользователь не найден"
                    };
                }

                var newToken = GenerateJwtToken(user.Id, user.Email, user.Role);

                return new AuthResponse
                {
                    Success = true,
                    Token = newToken,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Phone = user.Phone,
                        Age = user.Age,
                        Role = user.Role
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refresh token error");
                return new AuthResponse
                {
                    Success = false,
                    Message = "Ошибка при обновлении токена"
                };
            }
        }

        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            return !await _context.Users.AnyAsync(u => u.Email == email);
        }

        private string GenerateJwtToken(int userId, string email, string role)
        {
            var jwtKey = _configuration["Jwt:Key"] ??
                "default-key-which-should-be-changed-in-production-1234567890";
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "Rockstar.API";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "Rockstar.Client";

            Debug.WriteLine($"JWT Key used: {jwtKey}");
            Debug.WriteLine($"JWT Issuer: {jwtIssuer}");
            Debug.WriteLine($"JWT Audience: {jwtAudience}");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim("id", userId.ToString()),
        new Claim(ClaimTypes.Email, email),
        new Claim(ClaimTypes.Role, role),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            Debug.WriteLine($"Generated token: {tokenString}");

            return tokenString;
        }
    }
}