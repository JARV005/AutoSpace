using Microsoft.EntityFrameworkCore;
using AutoSpace.Data;
using AutoSpace.Services;
using AutoSpace.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Context - SIN SSL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"=== DATABASE CONNECTION ===");
    Console.WriteLine($"Connection string is null: {string.IsNullOrEmpty(connectionString)}");
    
    if (!string.IsNullOrEmpty(connectionString))
    {
        // Quitar SSL de la connection string
        var connectionWithoutSSL = connectionString
            .Replace("SSL Mode=Require;", "")
            .Replace("Trust Server Certificate=true;", "");
        
        var safeLog = connectionWithoutSSL.Contains("Password=") 
            ? connectionWithoutSSL.Substring(0, Math.Min(connectionWithoutSSL.IndexOf("Password=") + 15, connectionWithoutSSL.Length)) + "***" 
            : connectionWithoutSSL;
        Console.WriteLine($"Connection: {safeLog}");
        
        options.UseNpgsql(connectionWithoutSSL, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null
            );
        });
    }
    else
    {
        Console.WriteLine("ERROR: Connection string is null or empty!");
    }
});

// Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITicketService, TicketService>();

// Configure Email Settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// ========== CONFIGURACIÓN CORS MEJORADA Y SEGURA ==========
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins(
                    "https://autospace-frontend.netlify.app/",  
                    "http://localhost:8080",                 // Desarrollo local
                    "http://localhost:3000",                 // Desarrollo local alternativo
                    "https://autospace-backend.onrender.com" // El mismo backend
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders("*");
        });
});

var app = builder.Build();

// ========== MIDDLEWARE PIPELINE ==========

// ✅ CORS SEGURO - SOLO ORÍGENES ESPECÍFICOS
app.UseCors("AllowSpecificOrigins");

// ✅ MANEJADOR EXPLÍCITO PARA PREFLIGHT REQUESTS
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        var origin = context.Request.Headers["Origin"].ToString();
        var allowedOrigins = new[] { 
            "https://autospace-frontend.netlify.app/",
            "http://localhost:8080", 
            "http://localhost:3000",
            "https://autospace-backend.onrender.com"
        };
        
        if (allowedOrigins.Contains(origin))
        {
            context.Response.Headers.Add("Access-Control-Allow-Origin", origin);
        }
        
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, PATCH, OPTIONS");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With, X-CSRF-Token, X-API-Key");
        context.Response.Headers.Add("Access-Control-Expose-Headers", "*");
        context.Response.Headers.Add("Access-Control-Max-Age", "86400");
        context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
        return;
    }
    await next();
});

// ✅ MIDDLEWARE DE MANEJO DE ERRORES GLOBAL
app.Use(async (context, next) =>
{
    try
    {
        var origin = context.Request.Headers["Origin"].ToString();
        var allowedOrigins = new[] { 
            "https://autospace-frontend.netlify.app/",
            "http://localhost:8080", 
            "http://localhost:3000",
            "https://autospace-backend.onrender.com"
        };
        
        if (allowedOrigins.Contains(origin))
        {
            context.Response.Headers.Add("Access-Control-Allow-Origin", origin);
            context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        }
        
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERROR GLOBAL: {ex.Message}");
        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        
        // Headers CORS incluso en errores
        var origin = context.Request.Headers["Origin"].ToString();
        var allowedOrigins = new[] { 
            "https://autospace-frontend.netlify.app/",
            "http://localhost:8080", 
            "http://localhost:3000",
            "https://autospace-backend.onrender.com"
        };
        
        if (allowedOrigins.Contains(origin))
        {
            context.Response.Headers.Add("Access-Control-Allow-Origin", origin);
            context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        }
        
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        var errorResponse = new
        {
            error = "Internal Server Error",
            message = ex.Message,
            timestamp = DateTime.UtcNow,
            details = app.Environment.IsDevelopment() ? ex.StackTrace : null
        };
        
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(errorResponse));
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// ========== ENDPOINTS ESPECIALES ==========

// ✅ AGREGAR ENDPOINT RAÍZ PARA RENDER
app.MapGet("/", () => {
    return Results.Json(new {
        message = "AutoSpace API is running!",
        timestamp = DateTime.UtcNow,
        version = "1.0.0",
        status = "active",
        environment = app.Environment.EnvironmentName,
        cors = "specific_origins_only"
    });
});

// ✅ HEALTH CHECK MEJORADO
app.MapGet("/health", async (ApplicationDbContext dbContext) => {
    try
    {
        var canConnect = await dbContext.Database.CanConnectAsync();
        return Results.Json(new {
            status = "Healthy",
            database = canConnect ? "Connected" : "Disconnected",
            timestamp = DateTime.UtcNow,
            service = "AutoSpace API",
            cors = "specific_origins_only"
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new {
            status = "Unhealthy",
            error = ex.Message,
            timestamp = DateTime.UtcNow,
            cors = "specific_origins_only"
        }, statusCode: 503);
    }
});

// ✅ VERIFICACIÓN DE BASE DE DATOS (solo log, no bloqueante)
_ = Task.Run(async () =>
{
    try
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        Console.WriteLine("Testing database connection...");
        var canConnect = await dbContext.Database.CanConnectAsync();
        Console.WriteLine($"Database connection test: {canConnect}");
        
        if (canConnect)
        {
            Console.WriteLine("✅ Database connection successful");
            
            // Verificar tablas usando los DbSets correctos
            try
            {
                Console.WriteLine("=== VERIFICANDO TABLAS DE BASE DE DATOS ===");
                
                var usersCount = await dbContext.Users.CountAsync();
                Console.WriteLine($"✅ Table Users: OK ({usersCount} registros)");
                
                var vehiclesCount = await dbContext.Vehicles.CountAsync();
                Console.WriteLine($"✅ Table Vehicles: OK ({vehiclesCount} registros)");
                
                var subscriptionsCount = await dbContext.Subscriptions.CountAsync();
                Console.WriteLine($"✅ Table Subscriptions: OK ({subscriptionsCount} registros)");
                
                var ratesCount = await dbContext.Rates.CountAsync();
                Console.WriteLine($"✅ Table Rates: OK ({ratesCount} registros)");
                
                var operatorsCount = await dbContext.Operators.CountAsync();
                Console.WriteLine($"✅ Table Operators: OK ({operatorsCount} registros)");
                
                var ticketsCount = await dbContext.Tickets.CountAsync();
                Console.WriteLine($"✅ Table Tickets: OK ({ticketsCount} registros)");
                
                Console.WriteLine("=== VERIFICACIÓN DE TABLAS COMPLETADA ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error verificando tablas: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("❌ Database connection failed");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database connection error: {ex.Message}");
    }
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
Console.WriteLine($"Starting application on port: {port}");
Console.WriteLine($"Application URL: http://0.0.0.0:{port}");
Console.WriteLine($"CORS Policy: AllowSpecificOrigins");
Console.WriteLine("=== APPLICATION STARTED SUCCESSFULLY ===");

// ✅ ESTA DEBE SER LA ÚLTIMA LÍNEA - INICIA EL SERVIDOR
app.Run($"http://0.0.0.0:{port}");