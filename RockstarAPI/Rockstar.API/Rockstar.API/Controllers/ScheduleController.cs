using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rockstar.API.Data;
using Rockstar.API.DTOs.Enrollment;
using Rockstar.API.DTOs.Schedule;
using Rockstar.API.Models;

namespace Rockstar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly RockstarContext _context;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(RockstarContext context, ILogger<ScheduleController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Получить всё расписание
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<ScheduleDto>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var query = _context.Schedules
                    .Include(s => s.Direction)
                    .Include(s => s.Trainer)
                    .Include(s => s.Service)
                    .Where(s => s.IsActive && s.DateTime >= DateTime.UtcNow.AddDays(-1));

                if (fromDate.HasValue)
                    query = query.Where(s => s.DateTime >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(s => s.DateTime <= toDate.Value);

                var schedules = await query
                    .OrderBy(s => s.DateTime)
                    .ToListAsync();

                var scheduleDtos = schedules.Select(s => new ScheduleDto
                {
                    Id = s.Id,
                    TrainerId = s.TrainerId,
                    TrainerName = s.Trainer != null
                        ? $"{s.Trainer.FirstName} {s.Trainer.LastName}"
                        : "",
                    DirectionId = s.DirectionId,
                    DirectionName = s.Direction.Name,
                    DirectionKey = s.Direction.NameKey,
                    ServiceId = s.ServiceId,
                    ServiceName = s.Service?.Name ?? "",
                    DateTime = s.DateTime,
                    DurationMinutes = s.DurationMinutes,
                    MaxParticipants = s.MaxParticipants,
                    CurrentParticipants = s.CurrentParticipants,
                    Price = s.Price,
                    IsGroup = s.IsGroup
                }).ToList();

                return Ok(scheduleDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить расписание по дате
        /// </summary>
        [HttpGet("by-date")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<ScheduleDto>), 200)]
        public async Task<IActionResult> GetByDate([FromQuery] DateTime date)
        {
            try
            {
                var startOfDay = date.Date;
                var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

                var schedules = await _context.Schedules
                    .Include(s => s.Direction)
                    .Include(s => s.Trainer)
                    .Include(s => s.Service)
                    .Where(s => s.IsActive &&
                               s.DateTime >= startOfDay &&
                               s.DateTime <= endOfDay)
                    .OrderBy(s => s.DateTime)
                    .ToListAsync();

                var scheduleDtos = schedules.Select(s => new ScheduleDto
                {
                    Id = s.Id,
                    TrainerId = s.TrainerId,
                    TrainerName = s.Trainer != null
                        ? $"{s.Trainer.FirstName} {s.Trainer.LastName}"
                        : "",
                    DirectionId = s.DirectionId,
                    DirectionName = s.Direction.Name,
                    DirectionKey = s.Direction.NameKey,
                    ServiceId = s.ServiceId,
                    ServiceName = s.Service?.Name ?? "",
                    DateTime = s.DateTime,
                    DurationMinutes = s.DurationMinutes,
                    MaxParticipants = s.MaxParticipants,
                    CurrentParticipants = s.CurrentParticipants,
                    Price = s.Price,
                    IsGroup = s.IsGroup
                }).ToList();

                return Ok(scheduleDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule by date");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить занятие по ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ScheduleDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var schedule = await _context.Schedules
                    .Include(s => s.Direction)
                    .Include(s => s.Trainer)
                    .Include(s => s.Service)
                    .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

                if (schedule == null)
                    return NotFound("Занятие не найдено");

                var scheduleDto = new ScheduleDto
                {
                    Id = schedule.Id,
                    TrainerId = schedule.TrainerId,
                    TrainerName = schedule.Trainer != null
                        ? $"{schedule.Trainer.FirstName} {schedule.Trainer.LastName}"
                        : "",
                    DirectionId = schedule.DirectionId,
                    DirectionName = schedule.Direction.Name,
                    DirectionKey = schedule.Direction.NameKey,
                    ServiceId = schedule.ServiceId,
                    ServiceName = schedule.Service?.Name ?? "",
                    DateTime = schedule.DateTime,
                    DurationMinutes = schedule.DurationMinutes,
                    MaxParticipants = schedule.MaxParticipants,
                    CurrentParticipants = schedule.CurrentParticipants,
                    Price = schedule.Price,
                    IsGroup = schedule.IsGroup
                };

                return Ok(scheduleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить групповые занятия
        /// </summary>
        [HttpGet("group")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<ScheduleDto>), 200)]
        public async Task<IActionResult> GetGroupSchedules()
        {
            try
            {
                var schedules = await _context.Schedules
                    .Include(s => s.Direction)
                    .Include(s => s.Trainer)
                    .Include(s => s.Service)
                    .Where(s => s.IsActive && s.IsGroup && s.DateTime >= DateTime.UtcNow)
                    .OrderBy(s => s.DateTime)
                    .ToListAsync();

                var scheduleDtos = schedules.Select(s => new ScheduleDto
                {
                    Id = s.Id,
                    TrainerId = s.TrainerId,
                    TrainerName = s.Trainer != null
                        ? $"{s.Trainer.FirstName} {s.Trainer.LastName}"
                        : "",
                    DirectionId = s.DirectionId,
                    DirectionName = s.Direction.Name,
                    DirectionKey = s.Direction.NameKey,
                    ServiceId = s.ServiceId,
                    ServiceName = s.Service?.Name ?? "",
                    DateTime = s.DateTime,
                    DurationMinutes = s.DurationMinutes,
                    MaxParticipants = s.MaxParticipants,
                    CurrentParticipants = s.CurrentParticipants,
                    Price = s.Price,
                    IsGroup = s.IsGroup
                }).ToList();

                return Ok(scheduleDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group schedules");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить индивидуальные занятия
        /// </summary>
        [HttpGet("personal")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<ScheduleDto>), 200)]
        public async Task<IActionResult> GetPersonalSchedules()
        {
            try
            {
                var schedules = await _context.Schedules
                    .Include(s => s.Direction)
                    .Include(s => s.Trainer)
                    .Include(s => s.Service)
                    .Where(s => s.IsActive && !s.IsGroup && s.DateTime >= DateTime.UtcNow)
                    .OrderBy(s => s.DateTime)
                    .ToListAsync();

                var scheduleDtos = schedules.Select(s => new ScheduleDto
                {
                    Id = s.Id,
                    TrainerId = s.TrainerId,
                    TrainerName = s.Trainer != null
                        ? $"{s.Trainer.FirstName} {s.Trainer.LastName}"
                        : "",
                    DirectionId = s.DirectionId,
                    DirectionName = s.Direction.Name,
                    DirectionKey = s.Direction.NameKey,
                    ServiceId = s.ServiceId,
                    ServiceName = s.Service?.Name ?? "",
                    DateTime = s.DateTime,
                    DurationMinutes = s.DurationMinutes,
                    MaxParticipants = s.MaxParticipants,
                    CurrentParticipants = s.CurrentParticipants,
                    Price = s.Price,
                    IsGroup = s.IsGroup
                }).ToList();

                return Ok(scheduleDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting personal schedules");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить расписание тренера
        /// </summary>
        [HttpGet("trainer/{trainerId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<ScheduleDto>), 200)]
        public async Task<IActionResult> GetByTrainer(int trainerId)
        {
            try
            {
                var schedules = await _context.Schedules
                    .Include(s => s.Direction)
                    .Include(s => s.Trainer)
                    .Include(s => s.Service)
                    .Where(s => s.TrainerId == trainerId && s.IsActive && s.DateTime >= DateTime.UtcNow)
                    .OrderBy(s => s.DateTime)
                    .ToListAsync();

                var scheduleDtos = schedules.Select(s => new ScheduleDto
                {
                    Id = s.Id,
                    TrainerId = s.TrainerId,
                    TrainerName = s.Trainer != null
                        ? $"{s.Trainer.FirstName} {s.Trainer.LastName}"
                        : "",
                    DirectionId = s.DirectionId,
                    DirectionName = s.Direction.Name,
                    DirectionKey = s.Direction.NameKey,
                    ServiceId = s.ServiceId,
                    ServiceName = s.Service?.Name ?? "",
                    DateTime = s.DateTime,
                    DurationMinutes = s.DurationMinutes,
                    MaxParticipants = s.MaxParticipants,
                    CurrentParticipants = s.CurrentParticipants,
                    Price = s.Price,
                    IsGroup = s.IsGroup
                }).ToList();

                return Ok(scheduleDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trainer schedule");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить расписание по направлению
        /// </summary>
        [HttpGet("direction/{directionId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<ScheduleDto>), 200)]
        public async Task<IActionResult> GetByDirection(int directionId)
        {
            try
            {
                var schedules = await _context.Schedules
                    .Include(s => s.Direction)
                    .Include(s => s.Trainer)
                    .Include(s => s.Service)
                    .Where(s => s.DirectionId == directionId && s.IsActive && s.DateTime >= DateTime.UtcNow)
                    .OrderBy(s => s.DateTime)
                    .ToListAsync();

                var scheduleDtos = schedules.Select(s => new ScheduleDto
                {
                    Id = s.Id,
                    TrainerId = s.TrainerId,
                    TrainerName = s.Trainer != null
                        ? $"{s.Trainer.FirstName} {s.Trainer.LastName}"
                        : "",
                    DirectionId = s.DirectionId,
                    DirectionName = s.Direction.Name,
                    DirectionKey = s.Direction.NameKey,
                    ServiceId = s.ServiceId,
                    ServiceName = s.Service?.Name ?? "",
                    DateTime = s.DateTime,
                    DurationMinutes = s.DurationMinutes,
                    MaxParticipants = s.MaxParticipants,
                    CurrentParticipants = s.CurrentParticipants,
                    Price = s.Price,
                    IsGroup = s.IsGroup
                }).ToList();

                return Ok(scheduleDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting direction schedule");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить записи на конкретное занятие (для админа)
        /// </summary>
        [HttpGet("{id}/enrollments")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(List<EnrollmentDto>), 200)]
        public async Task<IActionResult> GetEnrollments(int id)
        {
            try
            {
                var enrollments = await _context.Enrollments
                    .Include(e => e.User)
                    .Where(e => e.ScheduleId == id)
                    .OrderBy(e => e.EnrolledAt)
                    .Select(e => new EnrollmentDto
                    {
                        Id = e.Id,
                        UserId = e.UserId,
                        UserName = e.User != null ? $"{e.User.FirstName} {e.User.LastName}" : "Пользователь не найден",
                        UserEmail = e.User != null ? e.User.Email : "",
                        ScheduleId = e.ScheduleId,
                        EnrolledAt = e.EnrolledAt,
                        Status = e.Status
                    })
                    .ToListAsync();

                return Ok(enrollments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting enrollments for schedule {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить доступных клиентов для записи на занятие (для админа)
        /// </summary>
        [HttpGet("{id}/available-clients")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAvailableClients(int id)
        {
            var enrolledUserIds = await _context.Enrollments
                .Where(e => e.ScheduleId == id)
                .Select(e => e.UserId)
                .ToListAsync();

            var availableClients = await _context.Users
                .Where(u => u.IsActive && u.Role == "client" && !enrolledUserIds.Contains(u.Id))
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
                .ToListAsync();

            return Ok(availableClients);
        }

        /// <summary>
        /// Записаться на занятие (для клиента)
        /// </summary>
        [HttpPost("{id}/enroll")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Enroll(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized();

                var schedule = await _context.Schedules
                    .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

                if (schedule == null)
                    return NotFound("Занятие не найдено");

                var existingEnrollment = await _context.Enrollments
                    .AnyAsync(e => e.UserId == userId && e.ScheduleId == id);

                if (existingEnrollment)
                    return BadRequest("Вы уже записаны на это занятие");

                if (schedule.CurrentParticipants >= schedule.MaxParticipants)
                    return BadRequest("Нет свободных мест");

                var enrollment = new Enrollment
                {
                    UserId = userId.Value,
                    ScheduleId = id,
                    EnrolledAt = DateTime.UtcNow,
                    Status = "enrolled"
                };

                _context.Enrollments.Add(enrollment);
                schedule.CurrentParticipants++;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Вы успешно записаны на занятие" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enrolling in schedule {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Добавить клиента на занятие (для админа)
        /// </summary>
        [HttpPost("{id}/add-client/{userId}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AddClient(int id, int userId)
        {
            try
            {
                var schedule = await _context.Schedules
                    .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

                if (schedule == null)
                    return NotFound("Занятие не найдено");

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

                if (user == null)
                    return NotFound("Пользователь не найден");

                var existingEnrollment = await _context.Enrollments
                    .AnyAsync(e => e.UserId == userId && e.ScheduleId == id);

                if (existingEnrollment)
                    return BadRequest("Пользователь уже записан на это занятие");

                if (schedule.CurrentParticipants >= schedule.MaxParticipants)
                    return BadRequest("Нет свободных мест на этом занятии");

                var enrollment = new Enrollment
                {
                    UserId = userId,
                    ScheduleId = id,
                    EnrolledAt = DateTime.UtcNow,
                    Status = "enrolled"
                };

                _context.Enrollments.Add(enrollment);
                schedule.CurrentParticipants++;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Клиент успешно добавлен на занятие" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding client to schedule {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Отменить запись на занятие (для клиента)
        /// </summary>
        [HttpDelete("{id}/cancel")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CancelEnrollment(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized();

                var enrollment = await _context.Enrollments
                    .Include(e => e.Schedule)
                    .FirstOrDefaultAsync(e =>
                        e.ScheduleId == id &&
                        e.UserId == userId &&
                        e.Status == "enrolled");

                if (enrollment == null)
                    return NotFound("Запись не найдена");

                if (enrollment.Schedule.DateTime <= DateTime.UtcNow.AddHours(2))
                    return BadRequest("Нельзя отменить запись менее чем за 2 часа до начала");

                enrollment.Status = "cancelled";
                enrollment.Schedule.CurrentParticipants--;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Запись отменена" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling enrollment {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Удалить клиента с занятия (для админа)
        /// </summary>
        [HttpDelete("{scheduleId}/remove-client/{enrollmentId}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RemoveClient(int scheduleId, int enrollmentId)
        {
            try
            {
                var enrollment = await _context.Enrollments
                    .Include(e => e.Schedule)
                    .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.ScheduleId == scheduleId);

                if (enrollment == null)
                    return NotFound("Запись не найдена");

                if (enrollment.Status == "enrolled")
                {
                    enrollment.Schedule.CurrentParticipants--;
                }

                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Клиент удален с занятия" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing client from schedule");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Создать новое занятие (только для админа)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(ScheduleDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateScheduleDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var direction = await _context.Directions.FindAsync(dto.DirectionId);
                if (direction == null)
                    return BadRequest("Направление не найдено");

                if (dto.TrainerId.HasValue)
                {
                    var trainer = await _context.Trainers
                        .FirstOrDefaultAsync(t => t.Id == dto.TrainerId && t.IsActive);

                    if (trainer == null)
                        return BadRequest("Тренер не найден");

                    var isBusy = await IsTrainerBusy(dto.TrainerId.Value, dto.DateTime, dto.DurationMinutes);
                    if (isBusy)
                    {
                        return BadRequest("Тренер уже занят в указанное время. Выберите другое время.");
                    }
                }

                if (dto.ServiceId.HasValue)
                {
                    var service = await _context.Services
                        .FirstOrDefaultAsync(s => s.Id == dto.ServiceId && s.IsActive);

                    if (service == null)
                        return BadRequest("Услуга не найдена");
                }

                var schedule = new Schedule
                {
                    TrainerId = dto.TrainerId,
                    DirectionId = dto.DirectionId,
                    ServiceId = dto.ServiceId,
                    DateTime = dto.DateTime,
                    DurationMinutes = dto.DurationMinutes,
                    MaxParticipants = dto.MaxParticipants,
                    CurrentParticipants = 0,
                    Price = dto.Price,
                    IsGroup = dto.IsGroup,
                    IsActive = true
                };

                _context.Schedules.Add(schedule);
                await _context.SaveChangesAsync();

                await _context.Entry(schedule)
                    .Reference(s => s.Direction)
                    .LoadAsync();
                await _context.Entry(schedule)
                    .Reference(s => s.Trainer)
                    .LoadAsync();
                await _context.Entry(schedule)
                    .Reference(s => s.Service)
                    .LoadAsync();

                var scheduleDto = new ScheduleDto
                {
                    Id = schedule.Id,
                    TrainerId = schedule.TrainerId,
                    TrainerName = schedule.Trainer != null
                        ? $"{schedule.Trainer.FirstName} {schedule.Trainer.LastName}"
                        : "",
                    DirectionId = schedule.DirectionId,
                    DirectionName = schedule.Direction.Name,
                    DirectionKey = schedule.Direction.NameKey,
                    ServiceId = schedule.ServiceId,
                    ServiceName = schedule.Service?.Name ?? "",
                    DateTime = schedule.DateTime,
                    DurationMinutes = schedule.DurationMinutes,
                    MaxParticipants = schedule.MaxParticipants,
                    CurrentParticipants = schedule.CurrentParticipants,
                    Price = schedule.Price,
                    IsGroup = schedule.IsGroup
                };

                return CreatedAtAction(nameof(GetById), new { id = schedule.Id }, scheduleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating schedule");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Обновить занятие (только для админа)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateScheduleDto dto)
        {
            try
            {
                var schedule = await _context.Schedules.FindAsync(id);
                if (schedule == null)
                    return NotFound("Занятие не найдено");

                var direction = await _context.Directions.FindAsync(dto.DirectionId);
                if (direction == null)
                    return BadRequest("Направление не найдено");

                if (dto.TrainerId.HasValue)
                {
                    var trainer = await _context.Trainers
                        .FirstOrDefaultAsync(t => t.Id == dto.TrainerId && t.IsActive);

                    if (trainer == null)
                        return BadRequest("Тренер не найден");

                    var isBusy = await IsTrainerBusy(dto.TrainerId.Value, dto.DateTime, dto.DurationMinutes, id);
                    if (isBusy)
                    {
                        return BadRequest("Тренер уже занят в указанное время. Выберите другое время.");
                    }
                }

                if (dto.ServiceId.HasValue)
                {
                    var service = await _context.Services
                        .FirstOrDefaultAsync(s => s.Id == dto.ServiceId && s.IsActive);

                    if (service == null)
                        return BadRequest("Услуга не найдена");
                }

                if (dto.MaxParticipants < schedule.CurrentParticipants)
                    return BadRequest($"Нельзя установить максимум участников меньше текущего количества ({schedule.CurrentParticipants})");

                schedule.TrainerId = dto.TrainerId;
                schedule.DirectionId = dto.DirectionId;
                schedule.ServiceId = dto.ServiceId;
                schedule.DateTime = dto.DateTime;
                schedule.DurationMinutes = dto.DurationMinutes;
                schedule.MaxParticipants = dto.MaxParticipants;
                schedule.Price = dto.Price;
                schedule.IsGroup = dto.IsGroup;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating schedule {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
        /// <summary>
        /// Получить ID занятий, которые были удалены после последней проверки
        /// </summary>
        [HttpGet("deleted-schedule-ids")]
        [Authorize]
        [ProducesResponseType(typeof(List<int>), 200)]
        public async Task<IActionResult> GetDeletedScheduleIds([FromQuery] long lastChecked)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized();

                var lastCheckDate = DateTimeOffset.FromUnixTimeMilliseconds(lastChecked).UtcDateTime;

                // Находим записи, которые были удалены (у которых нет schedule в БД)
                var userEnrollments = await _context.Enrollments
                    .Where(e => e.UserId == userId && e.Status == "enrolled")
                    .Select(e => e.ScheduleId)
                    .ToListAsync();

                // Проверяем, какие из этих занятий ещё существуют
                var existingScheduleIds = await _context.Schedules
                    .Where(s => userEnrollments.Contains(s.Id))
                    .Select(s => s.Id)
                    .ToListAsync();

                var deletedIds = userEnrollments.Except(existingScheduleIds).ToList();

                // Также проверяем записи, которые были удалены после lastCheckDate
                var recentlyDeleted = await _context.Enrollments
                    .Where(e => e.UserId == userId && e.UpdatedAt > lastCheckDate && e.Status == "cancelled_by_admin")
                    .Select(e => e.ScheduleId)
                    .ToListAsync();

                deletedIds.AddRange(recentlyDeleted);

                return Ok(deletedIds.Distinct());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deleted schedule ids");
                return Ok(new List<int>());
            }
        }
        /// <summary>
        /// Удалить занятие (только для админа) - удаляет записи и отправляет уведомления клиентам
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var schedule = await _context.Schedules
                    .Include(s => s.Enrollments)
                        .ThenInclude(e => e.User)
                    .Include(s => s.Direction)
                    .Include(s => s.Trainer)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (schedule == null)
                    return NotFound("Занятие не найдено");

                var activeEnrollments = schedule.Enrollments.Where(e => e.Status == "enrolled").ToList();

                // Отправляем уведомления клиентам (в мобильном приложении они увидят при следующем открытии)
                // Для этого просто сохраняем информацию в лог, клиенты сами проверят изменения при загрузке расписания

                _logger.LogInformation($"Deleting schedule {id}. Affected {activeEnrollments.Count} clients. " +
                                       $"Direction: {schedule.Direction?.Name}, DateTime: {schedule.DateTime}");

                // Удаляем все записи на это занятие
                _context.Enrollments.RemoveRange(schedule.Enrollments);

                // Удаляем само занятие
                _context.Schedules.Remove(schedule);

                await _context.SaveChangesAsync();

                return Ok(new { message = "Занятие успешно удалено", affectedClients = activeEnrollments.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting schedule {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Отметить посещение (для админа)
        /// </summary>
        [HttpPut("{scheduleId}/mark-attendance/{enrollmentId}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> MarkAttendance(int scheduleId, int enrollmentId, [FromBody] string status)
        {
            try
            {
                var validStatuses = new[] { "attended", "no_show" };
                if (!validStatuses.Contains(status))
                    return BadRequest($"Недопустимый статус. Допустимые значения: {string.Join(", ", validStatuses)}");

                var enrollment = await _context.Enrollments
                    .Include(e => e.Schedule)
                    .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.ScheduleId == scheduleId);

                if (enrollment == null)
                    return NotFound("Запись не найдена");

                if (enrollment.Schedule.DateTime > DateTime.UtcNow)
                    return BadRequest("Нельзя отметить посещение до начала занятия");

                enrollment.Status = status;
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Посещение отмечено как {status}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking attendance");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Проверка, занят ли тренер в указанное время
        /// </summary>
        private async Task<bool> IsTrainerBusy(int trainerId, DateTime dateTime, int durationMinutes, int? excludeScheduleId = null)
        {
            var startTime = dateTime;
            var endTime = dateTime.AddMinutes(durationMinutes);

            var query = _context.Schedules
                .Where(s => s.TrainerId == trainerId
                            && s.IsActive
                            && s.DateTime < endTime
                            && s.DateTime.AddMinutes(s.DurationMinutes) > startTime);

            if (excludeScheduleId.HasValue)
            {
                query = query.Where(s => s.Id != excludeScheduleId.Value);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// Получить все ПРОШЕДШИЕ занятия (история)
        /// </summary>
        [HttpGet("history")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(List<ScheduleDto>), 200)]
        public async Task<IActionResult> GetHistory([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var query = _context.Schedules
                    .Include(s => s.Direction)
                    .Include(s => s.Trainer)
                    .Include(s => s.Service)
                    .Where(s => s.IsActive && s.DateTime < DateTime.UtcNow);

                if (fromDate.HasValue)
                    query = query.Where(s => s.DateTime >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(s => s.DateTime <= toDate.Value);

                var schedules = await query
                    .OrderByDescending(s => s.DateTime)
                    .ToListAsync();

                var scheduleDtos = schedules.Select(s => new ScheduleDto
                {
                    Id = s.Id,
                    TrainerId = s.TrainerId,
                    TrainerName = s.Trainer != null
                        ? $"{s.Trainer.FirstName} {s.Trainer.LastName}"
                        : "",
                    DirectionId = s.DirectionId,
                    DirectionName = s.Direction.Name,
                    DirectionKey = s.Direction.NameKey,
                    ServiceId = s.ServiceId,
                    ServiceName = s.Service?.Name ?? "",
                    DateTime = s.DateTime,
                    DurationMinutes = s.DurationMinutes,
                    MaxParticipants = s.MaxParticipants,
                    CurrentParticipants = s.CurrentParticipants,
                    Price = s.Price,
                    IsGroup = s.IsGroup
                }).ToList();

                return Ok(scheduleDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting history schedules");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить статистику по расписанию (для админа)
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> GetStatistics([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var startDate = fromDate ?? DateTime.UtcNow.AddMonths(-1);
                var endDate = toDate ?? DateTime.UtcNow;

                var schedules = await _context.Schedules
                    .Include(s => s.Enrollments)
                    .Where(s => s.DateTime >= startDate && s.DateTime <= endDate)
                    .ToListAsync();

                var totalSchedules = schedules.Count;
                var totalParticipants = schedules.Sum(s => s.CurrentParticipants);
                var averageOccupancy = totalSchedules > 0
                    ? schedules.Average(s => (double)s.CurrentParticipants / s.MaxParticipants * 100)
                    : 0;

                var statistics = new
                {
                    Period = $"{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}",
                    TotalSchedules = totalSchedules,
                    TotalParticipants = totalParticipants,
                    AverageOccupancy = Math.Round(averageOccupancy, 1),
                    GroupSessions = schedules.Count(s => s.IsGroup),
                    PersonalSessions = schedules.Count(s => !s.IsGroup),
                    ByDirection = schedules
                        .GroupBy(s => s.DirectionId)
                        .Select(g => new
                        {
                            DirectionId = g.Key,
                            Count = g.Count(),
                            Participants = g.Sum(s => s.CurrentParticipants)
                        })
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule statistics");
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