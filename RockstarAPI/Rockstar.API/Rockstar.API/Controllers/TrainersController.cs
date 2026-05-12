using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rockstar.API.Data;
using Rockstar.API.DTOs.Trainer;
using Rockstar.API.Models;

namespace Rockstar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainersController : ControllerBase
    {
        private readonly RockstarContext _context;
        private readonly ILogger<TrainersController> _logger;

        public TrainersController(RockstarContext context, ILogger<TrainersController> logger)
        {
            _context = context;
            _logger = logger;
        }
        /// <summary>
        /// Удалить тренера (только для админа)
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
                var trainer = await _context.Trainers
                    .Include(t => t.TrainerDirections)
                    .Include(t => t.Schedules)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (trainer == null)
                    return NotFound("Тренер не найден");

                // Проверка на будущие занятия
                var futureSchedules = trainer.Schedules.Any(s => s.IsActive && s.DateTime >= DateTime.UtcNow);
                if (futureSchedules)
                {
                    return BadRequest("Нельзя удалить тренера с будущими занятиями в расписании");
                }

                // Удаляем связи с направлениями
                if (trainer.TrainerDirections != null && trainer.TrainerDirections.Any())
                {
                    _context.TrainerDirections.RemoveRange(trainer.TrainerDirections);
                }

                // Удаляем прошедшие занятия (опционально)
                var pastSchedules = trainer.Schedules.Where(s => s.DateTime < DateTime.UtcNow).ToList();
                if (pastSchedules.Any())
                {
                    _context.Schedules.RemoveRange(pastSchedules);
                }

                // Удаляем тренера
                _context.Trainers.Remove(trainer);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting trainer {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
        /// <summary>
        /// Получить всех тренеров (с несколькими направлениями)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<TrainerDto>), 200)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var trainers = await _context.Trainers
                    .Include(t => t.TrainerDirections)
                        .ThenInclude(td => td.Direction)
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.LastName)
                    .ThenBy(t => t.FirstName)
                    .ToListAsync();

                var trainerDtos = trainers.Select(t => MapToTrainerDto(t)).ToList();
                return Ok(trainerDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trainers");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить тренера по ID (с несколькими направлениями)
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TrainerDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var trainer = await _context.Trainers
                    .Include(t => t.TrainerDirections)
                        .ThenInclude(td => td.Direction)
                    .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

                if (trainer == null)
                    return NotFound("Тренер не найден");

                return Ok(MapToTrainerDto(trainer));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trainer {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Создать нового тренера (с несколькими направлениями)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(TrainerDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateTrainerDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Проверка уникальности email
                if (!string.IsNullOrEmpty(dto.Email))
                {
                    var exists = await _context.Trainers.AnyAsync(t => t.Email == dto.Email);
                    if (exists)
                        return BadRequest("Тренер с таким email уже существует");
                }

                var trainer = new Trainer
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    Experience = dto.Experience,
                    Description = dto.Description,
                    IsActive = true
                };

                // Хеширование пароля, если указан
                if (!string.IsNullOrEmpty(dto.Password))
                {
                    trainer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                }

                // Обработка фото
                if (!string.IsNullOrEmpty(dto.PhotoBase64))
                {
                    try
                    {
                        trainer.Photo = Convert.FromBase64String(dto.PhotoBase64);
                    }
                    catch
                    {
                        return BadRequest("Неверный формат фото");
                    }
                }

                _context.Trainers.Add(trainer);
                await _context.SaveChangesAsync();

                // 👇 НОВОЕ: добавляем направления
                if (dto.DirectionIds != null && dto.DirectionIds.Any())
                {
                    foreach (var directionId in dto.DirectionIds)
                    {
                        _context.TrainerDirections.Add(new TrainerDirection
                        {
                            TrainerId = trainer.Id,
                            DirectionId = directionId
                        });
                    }
                    await _context.SaveChangesAsync();
                }
                else if (dto.DirectionId.HasValue)
                {
                    // Для обратной совместимости
                    _context.TrainerDirections.Add(new TrainerDirection
                    {
                        TrainerId = trainer.Id,
                        DirectionId = dto.DirectionId.Value
                    });
                    await _context.SaveChangesAsync();
                }

                // Загружаем направления для ответа
                await _context.Entry(trainer)
                    .Collection(t => t.TrainerDirections)
                    .Query()
                    .Include(td => td.Direction)
                    .LoadAsync();

                return CreatedAtAction(nameof(GetById), new { id = trainer.Id }, MapToTrainerDto(trainer));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating trainer");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Обновить тренера (с несколькими направлениями)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateTrainerDto dto)
        {
            try
            {
                var trainer = await _context.Trainers
                    .Include(t => t.TrainerDirections)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (trainer == null)
                    return NotFound("Тренер не найден");

                // Проверка уникальности email
                if (!string.IsNullOrEmpty(dto.Email) && trainer.Email != dto.Email)
                {
                    var exists = await _context.Trainers.AnyAsync(t => t.Email == dto.Email && t.Id != id);
                    if (exists)
                        return BadRequest("Тренер с таким email уже существует");
                }

                // Обновление полей
                trainer.FirstName = dto.FirstName ?? trainer.FirstName;
                trainer.LastName = dto.LastName ?? trainer.LastName;
                trainer.Email = dto.Email ?? trainer.Email;
                trainer.Experience = dto.Experience;
                trainer.Description = dto.Description ?? trainer.Description;

                // Обновление пароля
                if (!string.IsNullOrEmpty(dto.Password))
                {
                    trainer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                }

                // Обновление фото
                if (!string.IsNullOrEmpty(dto.PhotoBase64))
                {
                    try
                    {
                        trainer.Photo = Convert.FromBase64String(dto.PhotoBase64);
                    }
                    catch
                    {
                        return BadRequest("Неверный формат фото");
                    }
                }

                // 👇 НОВОЕ: обновляем направления
                if (dto.DirectionIds != null && dto.DirectionIds.Any())
                {
                    // Удаляем старые связи
                    var oldDirections = _context.TrainerDirections.Where(td => td.TrainerId == id);
                    _context.TrainerDirections.RemoveRange(oldDirections);

                    // Добавляем новые
                    foreach (var directionId in dto.DirectionIds)
                    {
                        _context.TrainerDirections.Add(new TrainerDirection
                        {
                            TrainerId = trainer.Id,
                            DirectionId = directionId
                        });
                    }
                }
                else if (dto.DirectionId.HasValue)
                {
                    var oldDirections = _context.TrainerDirections.Where(td => td.TrainerId == id);
                    _context.TrainerDirections.RemoveRange(oldDirections);
                    _context.TrainerDirections.Add(new TrainerDirection
                    {
                        TrainerId = trainer.Id,
                        DirectionId = dto.DirectionId.Value
                    });
                }

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trainer {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        private TrainerDto MapToTrainerDto(Trainer trainer)
        {
            return new TrainerDto
            {
                Id = trainer.Id,
                FirstName = trainer.FirstName,
                LastName = trainer.LastName,
                DirectionId = trainer.TrainerDirections.FirstOrDefault()?.DirectionId,
                DirectionName = trainer.TrainerDirections.FirstOrDefault()?.Direction?.Name ?? "",
                DirectionKey = trainer.TrainerDirections.FirstOrDefault()?.Direction?.NameKey ?? "",
                // 👇 НОВОЕ: список всех направлений
                Directions = trainer.TrainerDirections.Select(td => new DirectionDto
                {
                    Id = td.Direction.Id,
                    Name = td.Direction.Name,
                    NameKey = td.Direction.NameKey
                }).ToList(),
                Email = trainer.Email,
                Experience = trainer.Experience,
                Description = trainer.Description,
                PhotoBase64 = trainer.Photo != null ? Convert.ToBase64String(trainer.Photo) : null
            };
        }
    }
}