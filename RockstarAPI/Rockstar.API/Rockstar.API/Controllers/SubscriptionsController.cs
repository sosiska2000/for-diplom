using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rockstar.API.Data;
using Rockstar.API.DTOs.Subscription;
using Rockstar.API.Models;

namespace Rockstar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly RockstarContext _context;
        private readonly ILogger<SubscriptionsController> _logger;

        public SubscriptionsController(RockstarContext context, ILogger<SubscriptionsController> logger)
        {
            _context = context;
            _logger = logger;
        }
        /// <summary>
        /// Получить все активные абонементы (для админа)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(List<SubscriptionDto>), 200)]
        public async Task<IActionResult> GetAllSubscriptions()
        {
            try
            {
                var subscriptions = await _context.Subscriptions
                    .Include(s => s.Direction)
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                var result = subscriptions.Select(s => new SubscriptionDto
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

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all subscriptions");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
        /// <summary>
        /// Получить все абонементы пользователя (для админа)
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(List<SubscriptionPurchaseDto>), 200)]
        public async Task<IActionResult> GetUserSubscriptions(int userId)
        {
            try
            {
                var purchases = await _context.Purchases
                    .Include(p => p.Subscription)
                        .ThenInclude(s => s.Direction)
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.PurchaseDate)
                    .ToListAsync();

                var result = purchases.Select(p => new SubscriptionPurchaseDto
                {
                    Id = p.Id,
                    SubscriptionId = p.SubscriptionId,
                    SubscriptionName = p.Subscription.Name,
                    DirectionId = p.Subscription.DirectionId,
                    DirectionName = p.Subscription.Direction?.Name ?? "",
                    Price = p.Subscription.Price,
                    TotalSessions = p.Subscription.SessionsCount,
                    SessionsUsed = p.SessionsUsed,
                    PurchaseDate = p.PurchaseDate,
                    ExpiryDate = p.ExpiryDate,
                    Status = p.Status
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user subscriptions for user {UserId}", userId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Купить абонемент для клиента (админ)
        /// </summary>
        [HttpPost("purchase")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(SubscriptionPurchase), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> PurchaseSubscription([FromBody] PurchaseSubscriptionDto dto)
        {
            try
            {
                // Проверяем существование абонемента
                var subscription = await _context.Subscriptions
                    .FirstOrDefaultAsync(s => s.Id == dto.SubscriptionId && s.IsActive);

                if (subscription == null)
                    return BadRequest(new { message = "Абонемент не найден" });

                // Проверяем существование пользователя
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == dto.UserId && u.IsActive);

                if (user == null)
                    return BadRequest(new { message = "Пользователь не найден" });

                // Создаем покупку
                var purchase = new SubscriptionPurchase
                {
                    UserId = dto.UserId,
                    SubscriptionId = dto.SubscriptionId,
                    PurchaseDate = DateTime.UtcNow,
                    ExpiryDate = dto.ExpiryDate ?? DateTime.UtcNow.AddMonths(12),
                    SessionsUsed = 0,
                    Status = "active"
                };

                _context.Purchases.Add(purchase);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Абонемент успешно добавлен", purchaseId = purchase.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error purchasing subscription for user {UserId}", dto.UserId);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Списать одно занятие по абонементу (админ)
        /// </summary>
        [HttpPost("use-session/{purchaseId}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UseSession(int purchaseId)
        {
            try
            {
                // 👇 УБИРАЕМ УСЛОВИЕ "status = active"
                var purchase = await _context.Purchases
                    .Include(p => p.Subscription)
                    .FirstOrDefaultAsync(p => p.Id == purchaseId);

                if (purchase == null)
                    return BadRequest(new { message = "Абонемент не найден" });

                // Проверяем, не превышен ли лимит
                if (purchase.SessionsUsed >= purchase.Subscription.SessionsCount)
                    return BadRequest(new { message = "Все занятия по абонементу уже использованы" });

                // Увеличиваем количество использованных занятий
                purchase.SessionsUsed++;

                // Если достигли лимита, меняем статус
                if (purchase.SessionsUsed >= purchase.Subscription.SessionsCount)
                    purchase.Status = "used_up";

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Занятие списано",
                    sessionsUsed = purchase.SessionsUsed,
                    sessionsRemaining = purchase.Subscription.SessionsCount - purchase.SessionsUsed
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error using session for purchase {PurchaseId}", purchaseId);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }
        /// Создать новый абонемент (только для админа)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(Subscription), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionDto dto)
        {
            try
            {
                // Проверка существования направления
                var direction = await _context.Directions
                    .FirstOrDefaultAsync(d => d.Id == dto.DirectionId && d.IsActive);

                if (direction == null)
                    return BadRequest(new { message = "Направление не найдено" });

                // Валидация данных
                if (string.IsNullOrWhiteSpace(dto.Name))
                    return BadRequest(new { message = "Название обязательно" });
                if (dto.Price < 0)
                    return BadRequest(new { message = "Цена не может быть отрицательной" });
                if (dto.SessionsCount <= 0)
                    return BadRequest(new { message = "Количество занятий должно быть больше 0" });

                // Создание абонемент
                var subscription = new Subscription
                {
                    Name = dto.Name,
                    DirectionId = dto.DirectionId,
                    Price = dto.Price,
                    SessionsCount = dto.SessionsCount,
                    Description = dto.Description,
                    IsActive = true
                };

                _context.Subscriptions.Add(subscription);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAllSubscriptions), new { id = subscription.Id }, subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Добавить занятие к абонементу (компенсация, админ)
        /// </summary>
        [HttpPost("add-session/{purchaseId}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AddSession(int purchaseId)
        {
            try
            {
                // 👇 УБИРАЕМ УСЛОВИЕ "status = active"
                var purchase = await _context.Purchases
                    .Include(p => p.Subscription)
                    .FirstOrDefaultAsync(p => p.Id == purchaseId);

                if (purchase == null)
                    return BadRequest(new { message = "Абонемент не найден" });

                // Проверяем, можно ли добавить занятие
                if (purchase.SessionsUsed <= 0)
                {
                    // Нельзя уменьшить ниже 0, просто возвращаем успех
                    return Ok(new
                    {
                        success = true,
                        message = "Нельзя добавить занятие, так как использовано 0 занятий",
                        sessionsUsed = purchase.SessionsUsed,
                        sessionsRemaining = purchase.Subscription.SessionsCount - purchase.SessionsUsed
                    });
                }

                // Уменьшаем количество использованных занятий
                purchase.SessionsUsed--;

                // Если статус был used_up, меняем на active
                if (purchase.Status == "used_up")
                    purchase.Status = "active";

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Занятие добавлено",
                    sessionsUsed = purchase.SessionsUsed,
                    sessionsRemaining = purchase.Subscription.SessionsCount - purchase.SessionsUsed
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding session for purchase {PurchaseId}", purchaseId);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }
    }
}