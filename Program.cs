using Microsoft.EntityFrameworkCore;
using AutoSpace.Data;
using AutoSpace.Services;

var builder = WebApplication.CreateBuilder(args);

// Log inicial para debugging
Console.WriteLine("=== AUTOSPACE API STARTING ===");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");

// Configurar Kestrel para Render
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Configuration with environment variables
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    
    // Log para debugging (sin mostrar password completo)
    Console.WriteLine("=== DATABASE CONFIGURATION ===");
    Console.WriteLine($"Connection String Present: {!string.IsNullOrEmpty(connectionString)}");
    
    if (!string.IsNullOrEmpty(connectionString))
    {
        var safeLog = connectionString.Length > 100 ? connectionString.Substring(0, 100) + "..." : connectionString;
        Console.WriteLine($"Connection String Preview: {safeLog}");
        
        // Verificar partes críticas
        var hasHost = connectionString.Contains("Host=") || connectionString.Contains("Server=");
        var hasDatabase = connectionString.Contains("Database=");
        var hasUser = connectionString.Contains("Username=") || connectionString.Contains("User Id=");
        var hasSSL = connectionString.Contains("SSL Mode=");
        
        Console.WriteLine($"Has Host: {hasHost}, Has Database: {hasDatabase}, Has User: {hasUser}, Has SSL: {hasSSL}");
    }
    else
    {
        Console.WriteLine("ERROR: Connection string is null or empty!");
        Console.WriteLine("Available environment variables:");
        foreach (var envVar in Environment.GetEnvironmentVariables().Keys)
        {
            Console.WriteLine($"  {envVar}");
        }
    }
    
    try
    {
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null
            );
        });
        Console.WriteLine("Database context configured successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR configuring database: {ex.Message}");
        throw;
    }
});

// Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITicketService, TicketService>();

// Configure Email Settings from environment variables
builder.Services.Configure<EmailSettings>(options =>
{
    options.SmtpServer = builder.Configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
    options.Port = int.Parse(builder.Configuration["EmailSettings:Port"] ?? "587");
    options.SenderName = builder.Configuration["EmailSettings:SenderName"] ?? "AutoSpace";
    options.SenderEmail = builder.Configuration["EmailSettings:SenderEmail"] ?? "";
    options.Username = builder.Configuration["EmailSettings:Username"] ?? "";
    options.Password = builder.Configuration["EmailSettings:Password"] ?? "";
});

// CORS configuration from environment variables
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:8080", "https://localhost:8080", "http://localhost:3000", "https://localhost:3000" };

Console.WriteLine($"Allowed CORS origins: {string.Join(", ", allowedOrigins)}");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // En producción, solo Swagger sin UI o con configuración segura
    app.UseSwagger();
}

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

// Health check endpoints - FIXED: Added proper return types
app.MapGet("/", () => Results.Json(new { 
    status = "AutoSpace API is running", 
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName,
    databaseConfigured = !string.IsNullOrEmpty(builder.Configuration.GetConnectionString("DefaultConnection"))
}));

app.MapGet("/health", () => Results.Json(new { 
    status = "Healthy", 
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
}));

// Test database connection endpoint - FIXED: Added proper return types
app.MapGet("/test-db", async (ApplicationDbContext dbContext) =>
{
    try
    {
        Console.WriteLine("Testing database connection...");
        var canConnect = await dbContext.Database.CanConnectAsync();
        Console.WriteLine($"Database connection result: {canConnect}");
        
        return Results.Json(new { 
            database = canConnect ? "Connected successfully" : "Cannot connect",
            timestamp = DateTime.UtcNow,
            details = canConnect ? "Database is accessible" : "Check connection string and network"
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database connection error: {ex.Message}");
        return Results.Json(new { 
            database = "Error: " + ex.Message,
            timestamp = DateTime.UtcNow,
            errorType = ex.GetType().Name
        });
    }
});

// Apply migrations in background
async Task ApplyMigrations()
{
    try
    {
        Console.WriteLine("Starting database migrations...");
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Check if we can connect first
        var canConnect = await dbContext.Database.CanConnectAsync();
        Console.WriteLine($"Database can connect before migrations: {canConnect}");
        
        if (canConnect)
        {
            Console.WriteLine("Applying migrations...");
            await dbContext.Database.MigrateAsync();
            Console.WriteLine("Migrations applied successfully");
        }
        else
        {
            Console.WriteLine("WARNING: Cannot apply migrations - database not accessible");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR during migrations: {ex.Message}");
        Console.WriteLine($"Full error: {ex}");
        // Don't crash the app - just log the error
    }
}

// Start migrations in background without blocking app startup
_ = Task.Run(ApplyMigrations);

// Get port from Render environment variable
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
Console.WriteLine($"Starting application on port: {port}");
Console.WriteLine($"Application URL: http://0.0.0.0:{port}");
Console.WriteLine("=== APPLICATION STARTED ===");

app.Run($"http://0.0.0.0:{port}");