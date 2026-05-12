using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rockstar.API.Data;
using Rockstar.API.DTOs.Service;
using Rockstar.API.Models;
using System.ComponentModel.DataAnnotations;

namespace Rockstar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly RockstarContext _context;
        private readonly ILogger<ServicesController> _logger;

        public ServicesController(RockstarContext context, ILogger<ServicesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Получить все услуги
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<ServiceDto>), 200)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var services = await _context.Services
                    .Include(s => s.Direction)
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.DirectionId)
                    .ThenBy(s => s.Name)
                    .Select(s => new ServiceDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        DirectionId = s.DirectionId,
                        DirectionName = s.Direction != null ? s.Direction.Name : "",
                        DirectionKey = s.Direction != null ? s.Direction.NameKey : "",
                        Price = s.Price,
                        SessionsCount = s.SessionsCount,
                        DurationMinutes = s.DurationMinutes,
                        Description = s.Description,
                        IsActive = s.IsActive
                    })
                    .ToListAsync();

                return Ok(services);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting services");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить услугу по ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ServiceDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var service = await _context.Services
                    .Include(s => s.Direction)
                    .Where(s => s.Id == id && s.IsActive)
                    .Select(s => new ServiceDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        DirectionId = s.DirectionId,
                        DirectionName = s.Direction != null ? s.Direction.Name : "",
                        DirectionKey = s.Direction != null ? s.Direction.NameKey : "",
                        Price = s.Price,
                        SessionsCount = s.SessionsCount,
                        DurationMinutes = s.DurationMinutes,
                        Description = s.Description,
                        IsActive = s.IsActive
                    })
                    .FirstOrDefaultAsync();

                if (service == null)
                    return NotFound("Услуга не найдена");

                return Ok(service);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить услуги по направлению
        /// </summary>
        [HttpGet("by-direction/{directionId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<ServiceDto>), 200)]
        public async Task<IActionResult> GetByDirection(int directionId)
        {
            try
            {
                var services = await _context.Services
                    .Include(s => s.Direction)
                    .Where(s => s.DirectionId == directionId && s.IsActive)
                    .OrderBy(s => s.Name)
                    .Select(s => new ServiceDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        DirectionId = s.DirectionId,
                        DirectionName = s.Direction != null ? s.Direction.Name : "",
                        DirectionKey = s.Direction != null ? s.Direction.NameKey : "",
                        Price = s.Price,
                        SessionsCount = s.SessionsCount,
                        DurationMinutes = s.DurationMinutes,
                        Description = s.Description,
                        IsActive = s.IsActive
                    })
                    .ToListAsync();

                return Ok(services);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting services for direction {DirectionId}", directionId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Создать новую услугу (только для админа)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(ServiceDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateServiceRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Проверка существования направления
                var direction = await _context.Directions.FindAsync(request.DirectionId);
                if (direction == null)
                    return BadRequest("Указанное направление не существует");

                // Проверка уникальности имени в рамках направления
                var exists = await _context.Services
                    .AnyAsync(s => s.DirectionId == request.DirectionId &&
                                  s.Name == request.Name &&
                                  s.IsActive);

                if (exists)
                    return BadRequest("Услуга с таким названием уже существует в этом направлении");

                // Создаем entity из DTO
                var service = new Service
                {
                    DirectionId = request.DirectionId,
                    Name = request.Name,
                    Price = request.Price,
                    SessionsCount = request.SessionsCount,
                    DurationMinutes = request.DurationMinutes,
                    Description = request.Description,
                    IsActive = true
                };

                _context.Services.Add(service);
                await _context.SaveChangesAsync();

                // Загружаем направление для ответа
                await _context.Entry(service)
                    .Reference(s => s.Direction)
                    .LoadAsync();

                var serviceDto = new ServiceDto
                {
                    Id = service.Id,
                    Name = service.Name,
                    DirectionId = service.DirectionId,
                    DirectionName = service.Direction?.Name ?? "",
                    DirectionKey = service.Direction?.NameKey ?? "",
                    Price = service.Price,
                    SessionsCount = service.SessionsCount,
                    DurationMinutes = service.DurationMinutes,
                    Description = service.Description,
                    IsActive = service.IsActive
                };

                return CreatedAtAction(nameof(GetById), new { id = service.Id }, serviceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Обновить услугу (только для админа)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceRequest request)
        {
            try
            {
                var existingService = await _context.Services.FindAsync(id);
                if (existingService == null)
                    return NotFound("Услуга не найдена");

                // Проверка существования направления
                var direction = await _context.Directions.FindAsync(request.DirectionId);
                if (direction == null)
                    return BadRequest("Указанное направление не существует");

                // Проверка уникальности имени (если меняется)
                if (existingService.Name != request.Name || existingService.DirectionId != request.DirectionId)
                {
                    var exists = await _context.Services
                        .AnyAsync(s => s.DirectionId == request.DirectionId &&
                                      s.Name == request.Name &&
                                      s.Id != id &&
                                      s.IsActive);

                    if (exists)
                        return BadRequest("Услуга с таким названием уже существует в этом направлении");
                }

                // Обновление полей
                existingService.Name = request.Name;
                existingService.DirectionId = request.DirectionId;
                existingService.Price = request.Price;
                existingService.SessionsCount = request.SessionsCount;
                existingService.DurationMinutes = request.DurationMinutes;
                existingService.Description = request.Description;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Удалить услугу (ЖЁСТКОЕ удаление, только для админа)
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
                var service = await _context.Services
                    .Include(s => s.Schedules)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (service == null)
                    return NotFound("Услуга не найдена");

                // ✅ ПРОВЕРКА: нельзя удалить, если есть будущие занятия
                if (service.Schedules.Any(s => s.IsActive && s.DateTime >= DateTime.UtcNow))
                {
                    return BadRequest("Нельзя удалить услугу с будущими занятиями в расписании");
                }

                // ✅ УДАЛЯЕМ СВЯЗАННЫЕ ПРОШЛЫЕ ЗАНЯТИЯ
                var pastSchedules = service.Schedules.Where(s => s.DateTime < DateTime.UtcNow).ToList();
                if (pastSchedules.Any())
                {
                    _context.Schedules.RemoveRange(pastSchedules);
                }

                // ✅ ЖЁСТКОЕ УДАЛЕНИЕ услуги
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
        // DTO для создания услуги
        public class CreateServiceRequest
        {
            [Range(1, int.MaxValue, ErrorMessage = "Направление обязательно")]
            public int DirectionId { get; set; }

            [Required(ErrorMessage = "Название обязательно")]
            [StringLength(100, ErrorMessage = "Название не может быть длиннее 100 символов")]
            public string Name { get; set; } = string.Empty;

            [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть больше 0")]
            public decimal Price { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "Количество занятий должно быть не менее 1")]
            public int SessionsCount { get; set; } = 1;

            [Range(1, 1440, ErrorMessage = "Длительность от 1 до 1440 минут")]
            public int? DurationMinutes { get; set; }

            [StringLength(500, ErrorMessage = "Описание не может быть длиннее 500 символов")]
            public string? Description { get; set; }
        }

        // DTO для обновления услуги
        public class UpdateServiceRequest
        {
            public int DirectionId { get; set; }
            public string Name { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public int SessionsCount { get; set; }
            public int? DurationMinutes { get; set; }
            public string? Description { get; set; }
        }
    }
}