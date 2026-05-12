using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rockstar.API.Data;
using Rockstar.API.DTOs;
using Rockstar.API.DTOs.Auth;
using Rockstar.API.DTOs.Enrollment;
using Rockstar.API.DTOs.Schedule;
using Rockstar.API.Models;

namespace Rockstar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly RockstarContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(RockstarContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Получить всех пользователей (только для админа)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(List<UserListDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetAllUsers([FromQuery] bool includeInactive = false)
        {
            try
            {
                var query = _context.Users
                    .Where(u => u.Role == "client"); // Только клиенты

                if (!includeInactive)
                    query = query.Where(u => u.IsActive);

                var users = await query
                    .OrderBy(u => u.LastName)
                    .ThenBy(u => u.FirstName)
                    .ToListAsync();

                // Получаем дополнительную статистику
                var userIds = users.Select(u => u.Id).ToList();

                var enrollmentsCount = await _context.Enrollments
                    .Where(e => userIds.Contains(e.UserId) && e.Status == "enrolled")
                    .GroupBy(e => e.UserId)
                    .Select(g => new { UserId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.UserId, x => x.Count);

                var purchasesCount = await _context.Purchases
                    .Where(sp => userIds.Contains(sp.UserId) && sp.Status == "active")
                    .GroupBy(sp => sp.UserId)
                    .Select(g => new { UserId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.UserId, x => x.Count);

                var userDtos = users.Select(u => new UserListDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Phone = u.Phone,
                    Age = u.Age,
                    BirthDate = u.BirthDate,  
                    Role = u.Role,
                    IsActive = u.IsActive,
                    ActiveEnrollmentsCount = enrollmentsCount.GetValueOrDefault(u.Id),
                    ActiveSubscriptionsCount = purchasesCount.GetValueOrDefault(u.Id),
                    CreatedAt = u.CreatedAt
                }).ToList();

                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить пользователя по ID (только для админа)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(UserListDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == id && u.Role == "client");

                if (user == null)
                    return NotFound("Пользователь не найден");

                // Получаем статистику
                var activeEnrollments = await _context.Enrollments
                    .CountAsync(e => e.UserId == id && e.Status == "enrolled");

                var activeSubscriptions = await _context.Purchases
                    .CountAsync(sp => sp.UserId == id && sp.Status == "active");

                var userDto = new UserListDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Phone = user.Phone,
                    Age = user.Age,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    ActiveEnrollmentsCount = activeEnrollments,
                    ActiveSubscriptionsCount = activeSubscriptions
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить профиль текущего пользователя (доступно всем авторизованным)
        /// </summary>
        /// <summary>
        /// Получить профиль текущего пользователя
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(typeof(UserProfileDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized("Не удалось определить пользователя");

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return NotFound("Пользователь не найден");

                var profile = new UserProfileDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Phone = user.Phone,
                    Age = user.Age,
                    BirthDate = user.BirthDate,  // 👈 ДОБАВЬТЕ
                    CreatedAt = user.CreatedAt
                };

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Обновить профиль текущего пользователя
        /// </summary>
        [HttpPut("profile")]
        [Authorize]
        [ProducesResponseType(typeof(UserProfileDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized();

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return NotFound("Пользователь не найден");

                // Обновление полей
                if (!string.IsNullOrWhiteSpace(dto.FirstName))
                    user.FirstName = dto.FirstName;

                if (!string.IsNullOrWhiteSpace(dto.LastName))
                    user.LastName = dto.LastName;

                if (!string.IsNullOrWhiteSpace(dto.Phone))
                    user.Phone = dto.Phone;

                if (dto.Age.HasValue)
                    user.Age = dto.Age;

                // Сохраняем дату рождения
                if (dto.BirthDate.HasValue)
                {
                    user.BirthDate = dto.BirthDate.Value.Date;
                    // Пересчитываем возраст на основе даты рождения
                    var today = DateTime.UtcNow.Date;
                    var age = today.Year - user.BirthDate.Value.Year;
                    if (user.BirthDate.Value.Date > today.AddYears(-age)) age--;
                    user.Age = age;
                }

                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var profile = new UserProfileDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Phone = user.Phone,
                    Age = user.Age,
                    BirthDate = user.BirthDate,
                    CreatedAt = user.CreatedAt
                };

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return NotFound("Пользователь не найден");

                // Проверка EMAIL
                if (!string.IsNullOrWhiteSpace(dto.Email) && user.Email != dto.Email)
                {
                    var emailExists = await _context.Users
                        .AnyAsync(u => u.Email == dto.Email && u.Id != id);

                    if (emailExists)
                        return BadRequest(new { message = "Пользователь с таким email уже существует" });

                    user.Email = dto.Email;
                }

                // Обновление полей
                if (!string.IsNullOrWhiteSpace(dto.FirstName))
                    user.FirstName = dto.FirstName;

                if (!string.IsNullOrWhiteSpace(dto.LastName))
                    user.LastName = dto.LastName;

                if (!string.IsNullOrWhiteSpace(dto.Phone))
                    user.Phone = dto.Phone;

                if (dto.Age.HasValue)
                    user.Age = dto.Age;

                // 👇 СОХРАНЯЕМ ДАТУ РОЖДЕНИЯ КАК ЕСТЬ (БЕЗ ПЕРЕСЧЁТА ВОЗРАСТА)
                if (dto.BirthDate.HasValue)
                {
                    user.BirthDate = dto.BirthDate.Value.Date;
                }

                user.IsActive = dto.IsActive;

                if (!string.IsNullOrWhiteSpace(dto.NewPassword))
                {
                    if (dto.NewPassword.Length < 6)
                        return BadRequest(new { message = "Пароль должен содержать минимум 6 символов" });

                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                }

                user.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Изменить пароль
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized();

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return NotFound("Пользователь не найден");

                // Проверка текущего пароля
                bool passwordValid = false;

                try
                {
                    passwordValid = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);
                }
                catch
                {
                    passwordValid = (user.PasswordHash == dto.CurrentPassword);
                }

                if (!passwordValid)
                {
                    return BadRequest(new { message = "Неверный текущий пароль" });
                }

                // Хешируем новый пароль
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

                await _context.SaveChangesAsync();

                return Ok(new { message = "Пароль успешно изменен" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить расписание пользователя
        /// </summary>
        [HttpGet("my-schedule")]
        [Authorize]
        [ProducesResponseType(typeof(List<ScheduleDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetMySchedule()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized();

                var enrollments = await _context.Enrollments
                    .Include(e => e.Schedule)
                        .ThenInclude(s => s.Direction)
                    .Include(e => e.Schedule)
                        .ThenInclude(s => s.Trainer)
                    .Include(e => e.Schedule)
                        .ThenInclude(s => s.Service)
                    .Where(e => e.UserId == userId && e.Status == "enrolled")
                    .OrderBy(e => e.Schedule.DateTime)
                    .ToListAsync();

                var scheduleDtos = enrollments.Select(e => new ScheduleDto
                {
                    Id = e.Schedule.Id,
                    TrainerId = e.Schedule.TrainerId,
                    TrainerName = e.Schedule.Trainer != null
                        ? $"{e.Schedule.Trainer.FirstName} {e.Schedule.Trainer.LastName}"
                        : "",
                    DirectionId = e.Schedule.DirectionId,
                    DirectionName = e.Schedule.Direction.Name,
                    DirectionKey = e.Schedule.Direction.NameKey,
                    ServiceId = e.Schedule.ServiceId,
                    ServiceName = e.Schedule.Service?.Name ?? "",
                    DateTime = e.Schedule.DateTime,
                    DurationMinutes = e.Schedule.DurationMinutes,
                    MaxParticipants = e.Schedule.MaxParticipants,
                    CurrentParticipants = e.Schedule.CurrentParticipants,
                    Price = e.Schedule.Price,
                    IsGroup = e.Schedule.IsGroup
                }).ToList();

                return Ok(scheduleDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user schedule");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить абонементы пользователя
        /// </summary>
        [HttpGet("my-subscriptions")]
        [Authorize]
        [ProducesResponseType(typeof(List<UserSubscriptionDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetMySubscriptions()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized();

                var purchases = await _context.Purchases
                    .Include(sp => sp.Subscription)
                        .ThenInclude(s => s.Direction)
                    .Where(sp => sp.UserId == userId)
                    .OrderByDescending(sp => sp.PurchaseDate)
                    .ToListAsync();

                var subscriptionDtos = purchases.Select(sp => new UserSubscriptionDto
                {
                    PurchaseId = sp.Id,
                    SubscriptionId = sp.SubscriptionId,
                    SubscriptionName = sp.Subscription.Name,
                    Price = sp.Subscription.Price,
                    TotalSessions = sp.Subscription.SessionsCount,
                    SessionsUsed = sp.SessionsUsed,
                    PurchaseDate = sp.PurchaseDate,
                    ExpiryDate = sp.ExpiryDate,
                    Status = sp.Status,
                    DirectionId = sp.Subscription.DirectionId,
                    DirectionName = sp.Subscription.Direction?.Name ?? ""
                }).ToList();

                return Ok(subscriptionDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user subscriptions");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить историю посещений
        /// </summary>
        [HttpGet("attendance-history")]
        [Authorize]
        [ProducesResponseType(typeof(List<EnrollmentDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetAttendanceHistory()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized();

                var history = await _context.Enrollments
                    .Include(e => e.Schedule)
                        .ThenInclude(s => s.Direction)
                    .Include(e => e.Schedule)
                        .ThenInclude(s => s.Trainer)
                    .Where(e => e.UserId == userId && e.Status != "enrolled")
                    .OrderByDescending(e => e.Schedule.DateTime)
                    .Take(50)
                    .ToListAsync();

                var historyDtos = history.Select(e => new EnrollmentDto
                {
                    Id = e.Id,
                    UserId = e.UserId,
                    ScheduleId = e.ScheduleId,
                    EnrolledAt = e.EnrolledAt,
                    Status = e.Status,
                    UserName = $"{e.User?.FirstName} {e.User?.LastName}",
                    UserEmail = e.User?.Email ?? ""
                }).ToList();

                return Ok(historyDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attendance history");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Удалить пользователя (ЖЁСТКОЕ удаление, для админа)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Enrollments)
                    .Include(u => u.Purchases)
                    .FirstOrDefaultAsync(u => u.Id == id && u.Role == "client");

                if (user == null)
                    return NotFound("Пользователь не найден");
                if (user.Enrollments.Any())
                {
                    _context.Enrollments.RemoveRange(user.Enrollments);
                }

                if (user.Purchases.Any())
                {
                    _context.Purchases.RemoveRange(user.Purchases);
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Восстановить пользователя (для админа)
        /// </summary>
        [HttpPost("{id}/restore")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RestoreUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return NotFound("Пользователь не найден");

                user.IsActive = true;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring user {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (int.TryParse(userIdClaim, out int userId))
                return userId;
            return null;
        }
    }
}   