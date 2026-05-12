using Microsoft.EntityFrameworkCore;
using Rockstar.API.Models;
using System;
using System.Linq;

namespace Rockstar.API.Data
{
    public static class DbInitializer
    {
        public static void Initialize(RockstarContext context)
        {
            // Применяем миграции
            context.Database.Migrate();

            // Инициализация данных только если БД пустая
            if (!context.Directions.Any())
            {
                Console.WriteLine("Инициализация начальных данных...");

                // 1. СОЗДАЕМ НАПРАВЛЕНИЯ
                var yogaDirection = new Direction
                {
                    Name = "Йога",
                    NameKey = "yoga",
                    Description = "Практики для гармонии тела и духа. Йога помогает улучшить гибкость, снять стресс и обрести внутренний баланс.",
                    IsActive = true
                };

                var fitnessDirection = new Direction
                {
                    Name = "Фитнес",
                    NameKey = "fitness",
                    Description = "Тренажерный зал и групповые тренировки для достижения оптимальной физической формы и укрепления здоровья.",
                    IsActive = true
                };

                var climbingDirection = new Direction
                {
                    Name = "Скалолазание",
                    NameKey = "climbing",
                    Description = "Скалодром для всех уровней подготовки. Развивает координацию, силу и выносливость.",
                    IsActive = true
                };

                context.Directions.AddRange(yogaDirection, fitnessDirection, climbingDirection);
                context.SaveChanges();

                // 2. СОЗДАЕМ ТРЕНЕРОВ
                var trainers = new[]
                {
                    new Trainer
                    {
                        FirstName = "Анна",
                        LastName = "Соколова",
                        DirectionId = yogaDirection.Id,
                        Email = "anna.sokolova@rockstar.ru",
                        Experience = 5,
                        Description = "Сертифицированный инструктор по Хатха-йоге и Аштанга-йоге. Проводит индивидуальные и групповые занятия.",
                        IsActive = true
                    },
                    new Trainer
                    {
                        FirstName = "Алексей",
                        LastName = "Иванов",
                        DirectionId = fitnessDirection.Id,
                        Email = "alexey.ivanov@rockstar.ru",
                        Experience = 4,
                        Description = "Тренер по функциональному тренингу и кроссфиту.",
                        IsActive = true
                    },
                    new Trainer
                    {
                        FirstName = "Елена",
                        LastName = "Петрова",
                        DirectionId = climbingDirection.Id,
                        Email = "elena.petrova@rockstar.ru",
                        Experience = 6,
                        Description = "Мастер спорта по скалолазанию. Чемпионка России по боулдерингу.",
                        IsActive = true
                    },
                    new Trainer
                    {
                        FirstName = "Мария",
                        LastName = "Смирнова",
                        DirectionId = yogaDirection.Id,
                        Email = "maria.smirnova@rockstar.ru",
                        Experience = 3,
                        Description = "Инструктор по йоге для начинающих и опытных практиков.",
                        IsActive = true
                    },
                    new Trainer
                    {
                        FirstName = "Дмитрий",
                        LastName = "Волков",
                        DirectionId = fitnessDirection.Id,
                        Email = "dmitry.volkov@rockstar.ru",
                        Experience = 8,
                        Description = "Эксперт по силовым тренировкам и функциональному фитнесу. Мастер спорта по пауэрлифтингу.",
                        IsActive = true
                    }
                };

                context.Trainers.AddRange(trainers);
                context.SaveChanges();

                // 3. СОЗДАЕМ УСЛУГИ
                var services = new[]
                {
                    new Service
                    {
                        DirectionId = yogaDirection.Id,
                        Name = "Хатха-йога",
                        Price = 800m,
                        SessionsCount = 1,
                        DurationMinutes = 60,
                        Description = "Классическое занятие йогой для всех уровней подготовки",
                        IsActive = true
                    },
                    new Service
                    {
                        DirectionId = yogaDirection.Id,
                        Name = "Аштанга-йога",
                        Price = 900m,
                        SessionsCount = 1,
                        DurationMinutes = 75,
                        Description = "Динамическая практика йоги для опытных",
                        IsActive = true
                    },
                    new Service
                    {
                        DirectionId = fitnessDirection.Id,
                        Name = "Силовая тренировка",
                        Price = 700m,
                        SessionsCount = 1,
                        DurationMinutes = 55,
                        Description = "Тренировка с отягощениями для всех групп мышц",
                        IsActive = true
                    },
                    new Service
                    {
                        DirectionId = fitnessDirection.Id,
                        Name = "Функциональный тренинг",
                        Price = 750m,
                        SessionsCount = 1,
                        DurationMinutes = 50,
                        Description = "Развитие координации, выносливости и силы",
                        IsActive = true
                    },
                    new Service
                    {
                        DirectionId = climbingDirection.Id,
                        Name = "Скалолазание для начинающих",
                        Price = 1000m,
                        SessionsCount = 1,
                        DurationMinutes = 90,
                        Description = "Обучение базовым навыкам работы на скалодроме",
                        IsActive = true
                    },
                    new Service
                    {
                        DirectionId = climbingDirection.Id,
                        Name = "Боулдеринг",
                        Price = 850m,
                        SessionsCount = 1,
                        DurationMinutes = 60,
                        Description = "Тренировка на сложность без страховки",
                        IsActive = true
                    }
                };

                context.Services.AddRange(services);
                context.SaveChanges();

                // 4. СОЗДАЕМ АБОНЕМЕНТЫ
                var subscriptions = new[]
                {
                    new Subscription
                    {
                        Name = "Йога Старт",
                        DirectionId = yogaDirection.Id,
                        Price = 3500m,
                        SessionsCount = 8,
                        Description = "Абонемент на 8 занятий по йоге",
                        IsActive = true
                    },
                    new Subscription
                    {
                        Name = "Йога Профи",
                        DirectionId = yogaDirection.Id,
                        Price = 4800m,
                        SessionsCount = 12,
                        Description = "Абонемент на 12 занятий по йоге",
                        IsActive = true
                    },
                    new Subscription
                    {
                        Name = "Фитнес Базовый",
                        DirectionId = fitnessDirection.Id,
                        Price = 3000m,
                        SessionsCount = 10,
                        Description = "Абонемент на 10 посещений зала",
                        IsActive = true
                    },
                    new Subscription
                    {
                        Name = "Фитнес Безлимит",
                        DirectionId = fitnessDirection.Id,
                        Price = 5000m,
                        SessionsCount = 30,
                        Description = "Безлимитное посещение на месяц",
                        IsActive = true
                    },
                    new Subscription
                    {
                        Name = "Скалолазание Старт",
                        DirectionId = climbingDirection.Id,
                        Price = 4000m,
                        SessionsCount = 8,
                        Description = "Абонемент на 8 посещений скалодрома",
                        IsActive = true
                    },
                    new Subscription
                    {
                        Name = "Скалолазание Профи",
                        DirectionId = climbingDirection.Id,
                        Price = 5500m,
                        SessionsCount = 12,
                        Description = "Абонемент на 12 посещений скалодрома",
                        IsActive = true
                    }
                };

                context.Subscriptions.AddRange(subscriptions);
                context.SaveChanges();

                // 5. СОЗДАЕМ АДМИНИСТРАТОРА
                var adminExists = context.Users.Any(u => u.Role == "admin");
                if (!adminExists)
                {
                    var admin = new User
                    {
                        Email = "admin@rockstar.ru",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                        FirstName = "Админ",
                        LastName = "Рокстар",
                        Role = "admin",
                        IsActive = true
                    };
                    context.Users.Add(admin);
                    context.SaveChanges();
                }

                // 6. СОЗДАЕМ ТЕСТОВОГО КЛИЕНТА
                var clientExists = context.Users.Any(u => u.Email == "client@test.ru");
                if (!clientExists)
                {
                    var client = new User
                    {
                        Email = "client@test.ru",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("client123"),
                        FirstName = "Иван",
                        LastName = "Петров",
                        Phone = "+7 (999) 123-45-67",
                        Age = 28,
                        Role = "client",
                        IsActive = true
                    };
                    context.Users.Add(client);
                    context.SaveChanges();
                }

                // 7. СОЗДАЕМ РАСПИСАНИЕ
                var trainersList = context.Trainers.ToList();
                var directionsList = context.Directions.ToList();

                var schedules = new[]
                {
                    new Schedule
                    {
                        TrainerId = trainersList.First(t => t.FirstName == "Анна").Id,
                        DirectionId = yogaDirection.Id,
                        DateTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10),
                        DurationMinutes = 60,
                        MaxParticipants = 15,
                        Price = 800,
                        IsGroup = true,
                        IsActive = true,
                        CurrentParticipants = 0
                    },
                    new Schedule
                    {
                        TrainerId = trainersList.First(t => t.FirstName == "Алексей").Id,
                        DirectionId = fitnessDirection.Id,
                        DateTime = DateTime.UtcNow.Date.AddDays(1).AddHours(18),
                        DurationMinutes = 55,
                        MaxParticipants = 20,
                        Price = 700,
                        IsGroup = true,
                        IsActive = true,
                        CurrentParticipants = 0
                    },
                    new Schedule
                    {
                        TrainerId = trainersList.First(t => t.FirstName == "Елена").Id,
                        DirectionId = climbingDirection.Id,
                        DateTime = DateTime.UtcNow.Date.AddDays(2).AddHours(15),
                        DurationMinutes = 90,
                        MaxParticipants = 10,
                        Price = 1000,
                        IsGroup = true,
                        IsActive = true,
                        CurrentParticipants = 0
                    },
                    new Schedule
                    {
                        TrainerId = trainersList.First(t => t.FirstName == "Мария").Id,
                        DirectionId = yogaDirection.Id,
                        DateTime = DateTime.UtcNow.Date.AddDays(2).AddHours(9),
                        DurationMinutes = 60,
                        MaxParticipants = 12,
                        Price = 800,
                        IsGroup = true,
                        IsActive = true,
                        CurrentParticipants = 0
                    }
                };

                context.Schedules.AddRange(schedules);
                context.SaveChanges();

                Console.WriteLine("Инициализация данных завершена успешно!");
            }
            else
            {
                Console.WriteLine("База данных уже содержит данные, инициализация пропущена");
            }
        }
    }
}