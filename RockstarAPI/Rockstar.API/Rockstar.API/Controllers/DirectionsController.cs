// Controllers/DirectionsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rockstar.API.Data;
using Rockstar.API.DTOs.Schedule;
using Rockstar.API.DTOs.Subscription;
using Rockstar.API.DTOs.Trainer;
using Rockstar.API.Models;

namespace Rockstar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DirectionsController : ControllerBase
    {
        private readonly RockstarContext _context;
        private readonly ILogger<DirectionsController> _logger;

        public DirectionsController(RockstarContext context, ILogger<DirectionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Получить все направления
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<Direction>), 200)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var directions = await _context.Directions
                    .Where(d => d.IsActive)
                    // ✅ ДОБАВИТЬ: загрузка связанных услуг
                    .Include(d => d.Services.Where(s => s.IsActive))
                    .OrderBy(d => d.Id)
                    .ToListAsync();
                return Ok(directions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting directions");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
        /// <summary>
        /// Удалить направление (мягкое удаление через IsActive)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteDirection(int id)
        {
            var direction = await _context.Directions.FindAsync(id);
            if (direction == null)
                return NotFound("Направление не найдено");

            // Проверка на связанные активные сущности
            var hasActiveSubscriptions = await _context.Subscriptions.AnyAsync(s => s.DirectionId == id && s.IsActive);
            var hasActiveServices = await _context.Services.AnyAsync(s => s.DirectionId == id && s.IsActive);
            var hasActiveSchedules = await _context.Schedules.AnyAsync(s => s.DirectionId == id && s.IsActive);
            var hasTrainers = await _context.TrainerDirections.AnyAsync(td => td.DirectionId == id);

            if (hasActiveSubscriptions || hasActiveServices || hasActiveSchedules || hasTrainers)
            {
                return BadRequest(new
                {
                    message = "Невозможно удалить: есть связанные активные услуги, абонементы, расписание или тренеры"
                });
            }

            direction.IsActive = false;
            // direction.UpdatedAt = DateTime.UtcNow; // 👈 ЭТУ СТРОКУ НУЖНО УБРАТЬ

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Восстановить удалённое направление
        /// </summary>
        [HttpPut("{id}/restore")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RestoreDirection(int id)
        {
            var direction = await _context.Directions.FindAsync(id);
            if (direction == null || direction.IsActive)
                return NotFound("Направление не найдено или уже активно");

            direction.IsActive = true;
            // direction.UpdatedAt = DateTime.UtcNow; // 👈 ЭТУ СТРОКУ ТОЖЕ УБРАТЬ

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Получить направление по ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Direction), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var direction = await _context.Directions
                    .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);

                if (direction == null)
                    return NotFound("Направление не найдено");

                return Ok(direction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting direction {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить направление по ключу (yoga, fitness, climbing)
        /// </summary>
        [HttpGet("key/{key}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Direction), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByKey(string key)
        {
            try
            {
                var direction = await _context.Directions
                    .FirstOrDefaultAsync(d => d.NameKey == key && d.IsActive);

                if (direction == null)
                    return NotFound("Направление не найдено");

                return Ok(direction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting direction by key {Key}", key);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить услуги направления
        /// </summary>
        [HttpGet("{id}/services")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<Service>), 200)]
        public async Task<IActionResult> GetServices(int id)
        {
            try
            {
                var services = await _context.Services
                    .Where(s => s.DirectionId == id && s.IsActive)
                    .OrderBy(s => s.Id)
                    .ToListAsync();

                return Ok(services);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting services for direction {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить абонементы направления
        /// </summary>
        [HttpGet("{id}/subscriptions")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<SubscriptionDto>), 200)]
        public async Task<IActionResult> GetSubscriptions(int id)
        {
            try
            {
                var subscriptions = await _context.Subscriptions
                    .Include(s => s.Direction)
                    .Where(s => s.DirectionId == id && s.IsActive)
                    .OrderBy(s => s.Id)
                    .ToListAsync();

                var subscriptionDtos = subscriptions.Select(s => new SubscriptionDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    DirectionId = s.DirectionId,
                    DirectionName = s.Direction?.Name ?? "",
                    DirectionKey = s.Direction?.NameKey ?? "",
                    Price = s.Price,
                    SessionsCount = s.SessionsCount,
                    Description = s.Description
                }).ToList();

                return Ok(subscriptionDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscriptions for direction {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить тренеров направления
        /// </summary>
        [HttpGet("{id}/trainers")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<TrainerDto>), 200)]
        public async Task<IActionResult> GetTrainers(int id)
        {
            try
            {
                var trainers = await _context.Trainers
                    .Include(t => t.Direction)
                    .Where(t => t.DirectionId == id && t.IsActive)
                    .OrderBy(t => t.LastName)
                    .ThenBy(t => t.FirstName)
                    .ToListAsync();

                var trainerDtos = trainers.Select(t => new TrainerDto
                {
                    Id = t.Id,
                    FirstName = t.FirstName,
                    LastName = t.LastName,
                    DirectionId = t.DirectionId,
                    DirectionName = t.Direction?.Name ?? "",
                    DirectionKey = t.Direction?.NameKey ?? "",
                    Email = t.Email,
                    Experience = t.Experience,
                    Description = t.Description,
                    PhotoBase64 = t.Photo != null ? Convert.ToBase64String(t.Photo) : null
                }).ToList();

                return Ok(trainerDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trainers for direction {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Создать новое направление (только для админа)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(Direction), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] Direction direction)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Проверка уникальности ключа
                var exists = await _context.Directions
                    .AnyAsync(d => d.NameKey == direction.NameKey);

                if (exists)
                    return BadRequest("Направление с таким ключом уже существует");

                direction.IsActive = true;

                _context.Directions.Add(direction);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = direction.Id }, direction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating direction");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Обновить направление (только для админа)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] Direction direction)
        {
            try
            {
                if (id != direction.Id)
                    return BadRequest("ID не совпадают");

                var existing = await _context.Directions.FindAsync(id);
                if (existing == null)
                    return NotFound("Направление не найдено");

                // Проверка уникальности ключа (если он меняется)
                if (existing.NameKey != direction.NameKey)
                {
                    var exists = await _context.Directions
                        .AnyAsync(d => d.NameKey == direction.NameKey && d.Id != id);

                    if (exists)
                        return BadRequest("Направление с таким ключом уже существует");
                }

                existing.Name = direction.Name;
                existing.NameKey = direction.NameKey;
                existing.Description = direction.Description;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating direction {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
    }
}