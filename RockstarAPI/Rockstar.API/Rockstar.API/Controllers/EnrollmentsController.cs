using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rockstar.API.Data;
using Rockstar.API.DTOs.Enrollment;
using Rockstar.API.Models;

namespace Rockstar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrollmentsController : ControllerBase
    {
        private readonly RockstarContext _context;
        private readonly ILogger<EnrollmentsController> _logger;

        public EnrollmentsController(RockstarContext context, ILogger<EnrollmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Получить все записи на занятие (для админа)
        /// </summary>
        [HttpGet("schedule/{scheduleId}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(List<EnrollmentDto>), 200)]
        public async Task<IActionResult> GetBySchedule(int scheduleId)
        {
            try
            {
                var enrollments = await _context.Enrollments
                    .Include(e => e.User)
                    .Include(e => e.Schedule)
                    .Where(e => e.ScheduleId == scheduleId)
                    .OrderBy(e => e.EnrolledAt)
                    .ToListAsync();

                var enrollmentDtos = enrollments.Select(e => new EnrollmentDto
                {
                    Id = e.Id,
                    UserId = e.UserId,
                    UserName = e.User != null ? $"{e.User.FirstName} {e.User.LastName}" : "",
                    UserEmail = e.User?.Email ?? "",
                    ScheduleId = e.ScheduleId,
                    EnrolledAt = e.EnrolledAt,
                    Status = e.Status
                }).ToList();

                return Ok(enrollmentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting enrollments for schedule {ScheduleId}", scheduleId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить записи пользователя
        /// </summary>
        [HttpGet("my")]
        [ProducesResponseType(typeof(List<EnrollmentDto>), 200)]
        public async Task<IActionResult> GetMyEnrollments()
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
                    .Where(e => e.UserId == userId)
                    .OrderByDescending(e => e.Schedule.DateTime)
                    .ToListAsync();

                var enrollmentDtos = enrollments.Select(e => new EnrollmentDto
                {
                    Id = e.Id,
                    UserId = e.UserId,
                    ScheduleId = e.ScheduleId,
                    EnrolledAt = e.EnrolledAt,
                    Status = e.Status,
                    // Добавляем информацию о занятии
                    ScheduleInfo = new
                    {
                        e.Schedule.DateTime,
                        Direction = e.Schedule.Direction?.Name,
                        Trainer = e.Schedule.Trainer != null
                            ? $"{e.Schedule.Trainer.FirstName} {e.Schedule.Trainer.LastName}"
                            : ""
                    }
                }).ToList();

                return Ok(enrollmentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user enrollments");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить доступных клиентов для записи на занятие (для админа)
        /// </summary>
        [HttpGet("available-clients/{scheduleId}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(List<object>), 200)]
        public async Task<IActionResult> GetAvailableClients(int scheduleId)
        {
            try
            {
                // Получаем ID пользователей, уже записанных на это занятие
                var enrolledUserIds = await _context.Enrollments
                    .Where(e => e.ScheduleId == scheduleId)
                    .Select(e => e.UserId)
                    .ToListAsync();

                // Получаем всех активных клиентов, кроме уже записанных
                var availableClients = await _context.Users
                    .Where(u => u.IsActive &&
                               u.Role == "client" &&
                               !enrolledUserIds.Contains(u.Id))
                    .Select(u => new
                    {
                        u.Id,
                        u.FirstName,
                        u.LastName,
                        FullName = u.FirstName + " " + u.LastName,
                        u.Email,
                        u.Phone,
                        u.Age
                    })
                    .OrderBy(u => u.LastName)
                    .ThenBy(u => u.FirstName)
                    .ToListAsync();

                return Ok(availableClients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available clients for schedule {ScheduleId}", scheduleId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Добавить клиента на занятие (для админа)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(EnrollmentDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateEnrollmentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Получаем ScheduleId из DTO
                if (dto.ScheduleId == 0)
                    return BadRequest("Не указан ID занятия");

                // Проверяем существование занятия
                var schedule = await _context.Schedules
                    .FirstOrDefaultAsync(s => s.Id == dto.ScheduleId && s.IsActive);

                if (schedule == null)
                    return BadRequest("Занятие не найдено");

                // Проверяем, не записан ли уже пользователь
                var existingEnrollment = await _context.Enrollments
                    .AnyAsync(e => e.UserId == dto.UserId && e.ScheduleId == dto.ScheduleId);

                if (existingEnrollment)
                    return BadRequest("Пользователь уже записан на это занятие");

                // Проверяем наличие мест
                if (schedule.CurrentParticipants >= schedule.MaxParticipants)
                    return BadRequest("Нет свободных мест на этом занятии");

                // Создаем запись
                var enrollment = new Enrollment
                {
                    UserId = dto.UserId,
                    ScheduleId = dto.ScheduleId,
                    EnrolledAt = DateTime.UtcNow,
                    Status = "enrolled"
                };

                _context.Enrollments.Add(enrollment);

                // Увеличиваем счетчик участников
                schedule.CurrentParticipants++;

                await _context.SaveChangesAsync();

                // Загружаем данные пользователя для ответа
                await _context.Entry(enrollment)
                    .Reference(e => e.User)
                    .LoadAsync();

                var enrollmentDto = new EnrollmentDto
                {
                    Id = enrollment.Id,
                    UserId = enrollment.UserId,
                    UserName = enrollment.User != null
                        ? $"{enrollment.User.FirstName} {enrollment.User.LastName}"
                        : "",
                    UserEmail = enrollment.User?.Email ?? "",
                    ScheduleId = enrollment.ScheduleId,
                    EnrolledAt = enrollment.EnrolledAt,
                    Status = enrollment.Status
                };

                return CreatedAtAction(nameof(GetById), new { id = enrollment.Id }, enrollmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating enrollment");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить запись по ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(EnrollmentDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var enrollment = await _context.Enrollments
                    .Include(e => e.User)
                    .Include(e => e.Schedule)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (enrollment == null)
                    return NotFound("Запись не найдена");

                var enrollmentDto = new EnrollmentDto
                {
                    Id = enrollment.Id,
                    UserId = enrollment.UserId,
                    UserName = enrollment.User != null
                        ? $"{enrollment.User.FirstName} {enrollment.User.LastName}"
                        : "",
                    UserEmail = enrollment.User?.Email ?? "",
                    ScheduleId = enrollment.ScheduleId,
                    EnrolledAt = enrollment.EnrolledAt,
                    Status = enrollment.Status
                };

                return Ok(enrollmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting enrollment {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Обновить статус записи (для админа)
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            try
            {
                var validStatuses = new[] { "enrolled", "attended", "cancelled", "no_show" };
                if (!validStatuses.Contains(status))
                    return BadRequest($"Недопустимый статус. Допустимые значения: {string.Join(", ", validStatuses)}");

                var enrollment = await _context.Enrollments
                    .Include(e => e.Schedule)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (enrollment == null)
                    return NotFound("Запись не найдена");

                // Если отменяем запись, уменьшаем счетчик участников
                if (status == "cancelled" && enrollment.Status == "enrolled")
                {
                    enrollment.Schedule.CurrentParticipants--;
                }

                enrollment.Status = status;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating enrollment status {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Удалить запись (для админа)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var enrollment = await _context.Enrollments
                    .Include(e => e.Schedule)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (enrollment == null)
                    return NotFound("Запись не найдена");

                // Если запись была активной, уменьшаем счетчик участников
                if (enrollment.Status == "enrolled")
                {
                    enrollment.Schedule.CurrentParticipants--;
                }

                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting enrollment {Id}", id);
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