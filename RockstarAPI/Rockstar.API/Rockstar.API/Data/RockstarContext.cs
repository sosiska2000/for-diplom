using Microsoft.EntityFrameworkCore;
using Rockstar.API.Models;

namespace Rockstar.API.Data
{
    public class RockstarContext : DbContext
    {
        public RockstarContext(DbContextOptions<RockstarContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<TrainerDirection> TrainerDirections { get; set; } // 👈 НОВОЕ
        public DbSet<Direction> Directions { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<SubscriptionPurchase> Purchases { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<RecurringSchedule> RecurringSchedules { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==================== USERS ====================
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).HasColumnName("email").IsRequired();
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
                entity.Property(e => e.FirstName).HasColumnName("first_name").IsRequired();
                entity.Property(e => e.LastName).HasColumnName("last_name").IsRequired();
                entity.Property(e => e.Phone).HasColumnName("phone");
                entity.Property(e => e.Age).HasColumnName("age");
                entity.Property(e => e.Role).HasColumnName("role").IsRequired();
                entity.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();

                entity.Property(e => e.BirthDate).HasColumnName("birth_date");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasIndex(e => e.Email).IsUnique();
            });

            // ==================== TRAINERS ====================
            modelBuilder.Entity<Trainer>(entity =>
            {
                entity.ToTable("trainers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).HasColumnName("first_name").IsRequired();
                entity.Property(e => e.LastName).HasColumnName("last_name").IsRequired();

                // 👇 Оставляем для обратной совместимости, но не используем
                entity.Property(e => e.DirectionId).HasColumnName("direction_id");

                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
                entity.Property(e => e.Photo).HasColumnName("photo");
                entity.Property(e => e.Experience).HasColumnName("experience");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();

                // Старая связь (оставляем для обратной совместимости)
                entity.HasOne(t => t.Direction)
                    .WithMany(d => d.Trainers)
                    .HasForeignKey(t => t.DirectionId);
            });

            // ==================== TRAINER DIRECTIONS (НОВОЕ) ====================
            modelBuilder.Entity<TrainerDirection>(entity =>
            {
                entity.ToTable("trainer_directions");
                entity.HasKey(e => new { e.TrainerId, e.DirectionId });

                entity.Property(e => e.TrainerId).HasColumnName("trainer_id");
                entity.Property(e => e.DirectionId).HasColumnName("direction_id");

                // Связь с тренером
                entity.HasOne(td => td.Trainer)
                    .WithMany(t => t.TrainerDirections)
                    .HasForeignKey(td => td.TrainerId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Связь с направлением
                entity.HasOne(td => td.Direction)
                    .WithMany(d => d.TrainerDirections)
                    .HasForeignKey(td => td.DirectionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================== DIRECTIONS ====================
            modelBuilder.Entity<Direction>(entity =>
            {
                entity.ToTable("directions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
                entity.Property(e => e.NameKey).HasColumnName("name_key").IsRequired();
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();

                entity.HasIndex(e => e.NameKey).IsUnique();
            });

            // ==================== SERVICES ====================
            modelBuilder.Entity<Service>(entity =>
            {
                entity.ToTable("services");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DirectionId).HasColumnName("direction_id").IsRequired();
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
                entity.Property(e => e.Price).HasColumnName("price").IsRequired();
                entity.Property(e => e.SessionsCount).HasColumnName("sessions_count").IsRequired();
                entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();

                entity.HasOne(s => s.Direction)
                    .WithMany(d => d.Services)
                    .HasForeignKey(s => s.DirectionId);
            });
            // ==================== RECURRING SCHEDULES ====================
            modelBuilder.Entity<RecurringSchedule>(entity =>
            {
                entity.ToTable("recurring_schedules");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.TrainerId).HasColumnName("trainer_id");
                entity.Property(e => e.DirectionId).HasColumnName("direction_id").IsRequired();
                entity.Property(e => e.ServiceId).HasColumnName("service_id");
                entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
                entity.Property(e => e.MaxParticipants).HasColumnName("max_participants");
                entity.Property(e => e.Price).HasColumnName("price");
                entity.Property(e => e.IsGroup).HasColumnName("is_group");

                entity.Property(e => e.Pattern).HasColumnName("pattern").IsRequired();
                entity.Property(e => e.Interval).HasColumnName("interval");
                entity.Property(e => e.WeekDays).HasColumnName("week_days");
                entity.Property(e => e.DayOfMonth).HasColumnName("day_of_month");

                entity.Property(e => e.StartDate).HasColumnName("start_date").IsRequired();
                entity.Property(e => e.EndDate).HasColumnName("end_date");
                entity.Property(e => e.MaxOccurrences).HasColumnName("max_occurrences");
                entity.Property(e => e.IsActive).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.HasOne(r => r.Trainer)
                    .WithMany()
                    .HasForeignKey(r => r.TrainerId);

                entity.HasOne(r => r.Direction)
                    .WithMany()
                    .HasForeignKey(r => r.DirectionId);

                entity.HasOne(r => r.Service)
                    .WithMany()
                    .HasForeignKey(r => r.ServiceId);
            });
            // ==================== SUBSCRIPTIONS ====================
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.ToTable("subscriptions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
                entity.Property(e => e.DirectionId).HasColumnName("direction_id");
                entity.Property(e => e.Price).HasColumnName("price").IsRequired();
                entity.Property(e => e.SessionsCount).HasColumnName("sessions_count").IsRequired();
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();

                entity.HasOne(s => s.Direction)
                    .WithMany(d => d.Subscriptions)
                    .HasForeignKey(s => s.DirectionId);
            });

            // ==================== SUBSCRIPTION PURCHASES ====================
            modelBuilder.Entity<SubscriptionPurchase>(entity =>
            {
                entity.ToTable("subscription_purchases");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
                entity.Property(e => e.SubscriptionId).HasColumnName("subscription_id").IsRequired();
                entity.Property(e => e.PurchaseDate).HasColumnName("purchase_date").IsRequired();
                entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");
                entity.Property(e => e.SessionsUsed).HasColumnName("sessions_used").IsRequired();
                entity.Property(e => e.Status).HasColumnName("status").IsRequired();

                entity.HasOne(sp => sp.User)
                    .WithMany(u => u.Purchases)
                    .HasForeignKey(sp => sp.UserId);

                entity.HasOne(sp => sp.Subscription)
                    .WithMany(s => s.Purchases)
                    .HasForeignKey(sp => sp.SubscriptionId);
            });

            // ==================== SCHEDULE ====================
            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.ToTable("schedule");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TrainerId).HasColumnName("trainer_id");
                entity.Property(e => e.DirectionId).HasColumnName("direction_id").IsRequired();
                entity.Property(e => e.ServiceId).HasColumnName("service_id");
                entity.Property(e => e.DateTime).HasColumnName("datetime").IsRequired();
                entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes").IsRequired();
                entity.Property(e => e.MaxParticipants).HasColumnName("max_participants").IsRequired();
                entity.Property(e => e.CurrentParticipants).HasColumnName("current_participants").IsRequired();
                entity.Property(e => e.Price).HasColumnName("price");
                entity.Property(e => e.IsGroup).HasColumnName("is_group").IsRequired();
                entity.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();

                entity.HasOne(s => s.Trainer)
                    .WithMany(t => t.Schedules)
                    .HasForeignKey(s => s.TrainerId);

                entity.HasOne(s => s.Direction)
                    .WithMany(d => d.Schedules)
                    .HasForeignKey(s => s.DirectionId);

                entity.HasOne(s => s.Service)
                    .WithMany(svc => svc.Schedules)
                    .HasForeignKey(s => s.ServiceId);
            });

            // ==================== ENROLLMENTS ====================
            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.ToTable("enrollments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
                entity.Property(e => e.ScheduleId).HasColumnName("schedule_id").IsRequired();
                entity.Property(e => e.EnrolledAt).HasColumnName("enrolled_at").IsRequired();
                entity.Property(e => e.Status).HasColumnName("status").IsRequired();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Enrollments)
                    .HasForeignKey(e => e.UserId);

                entity.HasOne(e => e.Schedule)
                    .WithMany(s => s.Enrollments)
                    .HasForeignKey(e => e.ScheduleId);

                // Уникальность: пользователь может быть записан на занятие только один раз
                entity.HasIndex(e => new { e.UserId, e.ScheduleId }).IsUnique();
            });
        }
    }
}