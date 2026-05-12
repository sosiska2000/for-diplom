using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Rockstar.API.Data;
using Rockstar.API.Mappings;
using Rockstar.API.Services;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ==================== ЛОГИРОВАНИЕ ====================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// ==================== CONTROLLERS + JSON ====================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();

// ==================== SWAGGER С ПОДДЕРЖКОЙ JWT ====================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Rockstar Club API",
        Version = "v1",
        Description = "API для управления спортивным клубом Rockstar"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ==================== ENTITY FRAMEWORK + MYSQL ====================
builder.Services.AddDbContext<RockstarContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// ==================== JWT AUTHENTICATION ====================
var jwtKey = builder.Configuration["Jwt:Key"] ??
    throw new InvalidOperationException("Jwt:Key не настроен в конфигурации!");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ??
    throw new InvalidOperationException("Jwt:Issuer не настроен в конфигурации!");
var jwtAudience = builder.Configuration["Jwt:Audience"] ??
    throw new InvalidOperationException("Jwt:Audience не настроен в конфигурации!");

// 🔍 Логирование настроек при старте
Console.WriteLine($"[JWT CONFIG] Key (first 30): '{jwtKey.Substring(0, Math.Min(30, jwtKey.Length))}...'");
Console.WriteLine($"[JWT CONFIG] Issuer: '{jwtIssuer}'");
Console.WriteLine($"[JWT CONFIG] Audience: '{jwtAudience}'");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            LogValidationExceptions = true,
            // 👇 ИСПРАВЛЕНО: используем Email как NameClaim
            NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
            RoleClaimType = ClaimTypes.Role
        };

        // 🔍 ОБРАБОТЧИКИ ДЛЯ ОТЛАДКИ
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                Console.WriteLine($"🔍 [JWT] Request path: {context.Request.Path}");
                Console.WriteLine($"🔍 [JWT] Authorization header: {(authHeader ?? "NULL")}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var identity = context.Principal?.Identity as ClaimsIdentity;
                var claims = identity?.Claims.Select(c => $"{c.Type}: {c.Value}");
                Console.WriteLine($"✅ [JWT] Token validated! User: {identity?.Name}");
                Console.WriteLine($"✅ [JWT] Claims: {string.Join(", ", claims ?? Enumerable.Empty<string>())}");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"❌ [JWT] Authentication FAILED: {context.Exception.Message}");
                Console.WriteLine($"❌ [JWT] Exception type: {context.Exception.GetType().Name}");

                if (context.Exception is SecurityTokenInvalidSignatureException)
                    Console.WriteLine("❌ [JWT] Invalid signature — ключ не совпадает!");
                else if (context.Exception is SecurityTokenExpiredException)
                    Console.WriteLine("❌ [JWT] Token expired");
                else if (context.Exception is SecurityTokenInvalidIssuerException)
                    Console.WriteLine($"❌ [JWT] Invalid issuer. Expected: '{jwtIssuer}'");
                else if (context.Exception is SecurityTokenInvalidAudienceException)
                    Console.WriteLine($"❌ [JWT] Invalid audience. Expected: '{jwtAudience}'");
                else if (context.Exception is SecurityTokenArgumentException)
                    Console.WriteLine("❌ [JWT] Token format error");

                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"⚠️ [JWT] Challenge triggered: {context.AuthenticateFailure}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ==================== СЕРВИСЫ ====================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddLogging();

// ==================== CORS ====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .WithOrigins("http://localhost:3000", "http://localhost:5173") // 👈 Укажите ваши фронтенд-порты
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // 👈 Важно для авторизации
    });
});

// ==================== BUILD APP ====================
var app = builder.Build();

// 🔍 Проверка конфигурации после билда
var configKey = app.Configuration["Jwt:Key"];
Console.WriteLine($"[POST-BUILD] Jwt:Key loaded: {!string.IsNullOrEmpty(configKey)}");

// ==================== PIPELINE ====================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rockstar Club API V1");
        c.RoutePrefix = string.Empty; // Swagger доступен по корню
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication(); // 👈 Важно: перед Authorization!
app.UseAuthorization();

app.MapControllers();

// ==================== MIGRATIONS + SEED ====================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RockstarContext>();
    try
    {
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("✅ Database migrations applied successfully");

        DbInitializer.Initialize(dbContext);
        Console.WriteLine("✅ Database initialized with seed data");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error applying migrations or initializing data: {ex.Message}");
        Console.WriteLine($"❌ Stack: {ex.StackTrace}");
    }
}

Console.WriteLine("🚀 API server starting...");
app.Run();