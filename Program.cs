using Microsoft.EntityFrameworkCore;
using AutoSpace.Data;
using AutoSpace.Services;
using AutoSpace.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"=== DATABASE CONNECTION ===");
    Console.WriteLine($"Connection string is null: {string.IsNullOrEmpty(connectionString)}");
    
    if (!string.IsNullOrEmpty(connectionString))
    {
        // Log segura (sin contraseña)
        var safeLog = connectionString.Contains("Password=") 
            ? connectionString.Substring(0, Math.Min(connectionString.IndexOf("Password=") + 15, connectionString.Length)) + "***" 
            : connectionString;
        Console.WriteLine($"Connection: {safeLog}");
    }
    
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null
        );
    });
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

// SOLO MAPEA LOS CONTROLADORES - elimina todo MapGet
app.MapControllers();

// Apply database migrations on startup
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    Console.WriteLine("Checking database connection...");
    var canConnect = await dbContext.Database.CanConnectAsync();
    Console.WriteLine($"Database can connect: {canConnect}");
    
    if (canConnect)
    {
        Console.WriteLine("Applying database migrations...");
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("Database migrations applied successfully");
        
        // Seed initial data if needed
        await SeedInitialData(dbContext);
    }
    else
    {
        Console.WriteLine("WARNING: Cannot apply migrations - database not accessible");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR during database initialization: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}

// Get port from environment variable (Render provides this)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
Console.WriteLine($"Starting application on port: {port}");
Console.WriteLine($"Application URL: http://0.0.0.0:{port}");
Console.WriteLine("=== APPLICATION STARTED SUCCESSFULLY ===");

app.Run($"http://0.0.0.0:{port}");

// Seed initial data method
static async Task SeedInitialData(ApplicationDbContext context)
{
    // Check if we already have operators
    if (!await context.Operators.AnyAsync())
    {
        Console.WriteLine("Seeding initial operator data...");
        
        var defaultOperator = new Operator
        {
            FullName = "Operador Principal",
            Document = "12345678",
            Email = "operador@autospace.com",
            Status = "Active",
            IsActive = true
        };
        
        context.Operators.Add(defaultOperator);
        await context.SaveChangesAsync();
        Console.WriteLine("Default operator created successfully");
    }

    // Check if we have rates
    if (!await context.Rates.AnyAsync())
    {
        Console.WriteLine("Seeding initial rates data...");
        
        var rates = new[]
        {
            new Rate { TypeVehicle = "Car", HourPrice = 5.00m, AddPrice = 2.50m, MaxPrice = 30.00m, GraceTime = "30" },
            new Rate { TypeVehicle = "Motorcycle", HourPrice = 3.00m, AddPrice = 1.50m, MaxPrice = 20.00m, GraceTime = "30" },
            new Rate { TypeVehicle = "Truck", HourPrice = 8.00m, AddPrice = 4.00m, MaxPrice = 50.00m, GraceTime = "30" }
        };
        
        context.Rates.AddRange(rates);
        await context.SaveChangesAsync();
        Console.WriteLine("Default rates created successfully");
    }

    // Check if we have at least one user
    if (!await context.Users.AnyAsync())
    {
        Console.WriteLine("Seeding initial user data...");
        
        var defaultUser = new User
        {
            FullName = "Usuario Demo",
            Document = "87654321",
            Email = "usuario@demo.com",
            Status = "Active"
        };
        
        context.Users.Add(defaultUser);
        await context.SaveChangesAsync();
        Console.WriteLine("Default user created successfully");
    }
    
    // Check if we have at least one vehicle
    if (!await context.Vehicles.AnyAsync())
    {
        Console.WriteLine("Seeding initial vehicle data...");
        
        var defaultUser = await context.Users.FirstOrDefaultAsync();
        if (defaultUser != null)
        {
            var defaultVehicle = new Vehicle
            {
                Plate = "ABC123",
                Type = "Car",
                UserId = defaultUser.Id
            };
            
            context.Vehicles.Add(defaultVehicle);
            await context.SaveChangesAsync();
            Console.WriteLine("Default vehicle created successfully");
        }
    }
}