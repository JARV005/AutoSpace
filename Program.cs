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

// SOLO verificar conexión, NO migraciones
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
    // No bloquear la aplicación si hay error de BD
}

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
Console.WriteLine($"Starting application on port: {port}");
Console.WriteLine($"Application URL: http://0.0.0.0:{port}");
Console.WriteLine("=== APPLICATION STARTED SUCCESSFULLY ===");

app.Run($"http://0.0.0.0:{port}");