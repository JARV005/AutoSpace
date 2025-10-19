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

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// ✅ AGREGAR ENDPOINT RAÍZ PARA RENDER
app.MapGet("/", () => {
    return Results.Json(new {
        message = "AutoSpace API is running!",
        timestamp = DateTime.UtcNow,
        version = "1.0.0",
        status = "active",
        environment = app.Environment.EnvironmentName
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
Console.WriteLine("=== APPLICATION STARTED SUCCESSFULLY ===");

// ✅ ESTA DEBE SER LA ÚLTIMA LÍNEA - INICIA EL SERVIDOR
app.Run($"http://0.0.0.0:{port}");