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

// ========== CONFIGURACIÓN CORS COMPLETA ==========
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .WithExposedHeaders("*"); // Exponer todos los headers
        });
});

var app = builder.Build();

// ========== MIDDLEWARE PIPELINE ==========

// ✅ CORS DEBE IR PRIMERO EN LA PIPELINE
app.UseCors("AllowAll");

// ✅ MANEJADOR EXPLÍCITO PARA PREFLIGHT REQUESTS
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, PATCH, OPTIONS");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With, X-CSRF-Token, X-API-Key");
        context.Response.Headers.Add("Access-Control-Expose-Headers", "*");
        context.Response.Headers.Add("Access-Control-Max-Age", "86400"); // 24 horas
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
        // Asegurar headers CORS en todas las respuestas
        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Add("Access-Control-Expose-Headers", "*");
        
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERROR GLOBAL: {ex.Message}");
        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        
        // Headers CORS incluso en errores
        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
        context.Response.Headers.Add("Access-Control-Expose-Headers", "*");
        
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
        cors = "enabled"
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
            cors = "enabled"
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new {
            status = "Unhealthy",
            error = ex.Message,
            timestamp = DateTime.UtcNow,
            cors = "enabled"
        }, statusCode: 503);
    }
});

// ✅ ENDPOINTS DE PRUEBA CORS
app.MapGet("/api/test-cors", () => {
    return Results.Json(new {
        message = "CORS GET test successful",
        timestamp = DateTime.UtcNow,
        cors = "enabled",
        method = "GET"
    });
});

app.MapPost("/api/test-cors-post", (TestModel model) => {
    return Results.Json(new {
        message = "CORS POST test successful",
        received = model,
        timestamp = DateTime.UtcNow,
        cors = "enabled",
        method = "POST"
    });
});

app.MapPut("/api/test-cors-put", (TestModel model) => {
    return Results.Json(new {
        message = "CORS PUT test successful",
        received = model,
        timestamp = DateTime.UtcNow,
        cors = "enabled",
        method = "PUT"
    });
});

app.MapDelete("/api/test-cors-delete", () => {
    return Results.Json(new {
        message = "CORS DELETE test successful",
        timestamp = DateTime.UtcNow,
        cors = "enabled",
        method = "DELETE"
    });
});

// ========== INICIALIZACIÓN ==========

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
            
            // Verificar tablas existentes
            var tables = new[] { "Users", "Vehicles", "Subscriptions", "Rates", "Operators", "Tickets" };
            foreach (var table in tables)
            {
                try
                {
                    // Intentar contar registros en cada tabla
                    var count = await dbContext.Set<object>().FromSqlRaw($"SELECT 1 FROM \"{table}\" LIMIT 1").CountAsync();
                    Console.WriteLine($"✅ Table {table}: OK");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Table {table}: {ex.Message}");
                }
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
Console.WriteLine($"CORS Policy: AllowAll");
Console.WriteLine("=== APPLICATION STARTED SUCCESSFULLY ===");

// ✅ ESTA DEBE SER LA ÚLTIMA LÍNEA - INICIA EL SERVIDOR
app.Run($"http://0.0.0.0:{port}");

// Modelo para el test POST
public class TestModel
{
    public string Test { get; set; }
    public int Number { get; set; }
}