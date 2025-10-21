using Microsoft.EntityFrameworkCore;
using AutoSpace.Data;
using AutoSpace.Services;
using AutoSpace.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Context - SIN SSL (tu código actual está bien)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"=== DATABASE CONNECTION ===");
    Console.WriteLine($"Connection string is null: {string.IsNullOrEmpty(connectionString)}");
    
    if (!string.IsNullOrEmpty(connectionString))
    {
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

// ========== CONFIGURACIÓN CORS SIMPLIFICADA ==========
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(
                "https://autospace-frontend.netlify.app",  
                "http://localhost:8080",
                "http://localhost:3000",
                "https://autospace-backend.onrender.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("*");
    });
});

var app = builder.Build();

// ========== MIDDLEWARE PIPELINE CORREGIDO ==========

// ✅ 1. CORS PRIMERO (maneja automáticamente preflight)
app.UseCors("AllowSpecificOrigins");

// ✅ 2. ROUTING INMEDIATAMENTE DESPUÉS DE CORS
app.UseRouting();

// ✅ 3. MIDDLEWARE DE MANEJO DE ERRORES SIMPLIFICADO
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERROR GLOBAL: {ex.Message}");
        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        
        // NO agregues headers CORS aquí - UseCors ya lo hace
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

// ✅ 4. AUTHORIZATION Y SWAGGER
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ 5. MAP CONTROLLERS (sin UseRouting duplicado)
app.MapControllers();

// ========== ENDPOINTS ESPECIALES ==========

// ✅ ENDPOINT RAÍZ
app.MapGet("/", () => {
    return Results.Json(new {
        message = "AutoSpace API is running!",
        timestamp = DateTime.UtcNow,
        version = "1.0.0",
        status = "active",
        environment = app.Environment.EnvironmentName
    });
});

// ✅ HEALTH CHECK
app.MapGet("/health", async (ApplicationDbContext dbContext) => {
    try
    {
        var canConnect = await dbContext.Database.CanConnectAsync();
        return Results.Json(new {
            status = "Healthy",
            database = canConnect ? "Connected" : "Disconnected",
            timestamp = DateTime.UtcNow,
            service = "AutoSpace API"
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new {
            status = "Unhealthy",
            error = ex.Message,
            timestamp = DateTime.UtcNow
        }, statusCode: 503);
    }
});

// ✅ VERIFICACIÓN DE BASE DE DATOS (mantener igual)
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
            // ... resto del código de verificación
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

app.Run($"http://0.0.0.0:{port}");