using Microsoft.EntityFrameworkCore;
using AutoSpace.Models;

namespace AutoSpace.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets para todas las entidades
        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Rate> Rates { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Operator> Operators { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Mail> Mails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurar nombres de tablas en snake_case para PostgreSQL
            ConfigureTableNames(modelBuilder);

            // Configurar tipos de datos para PostgreSQL
            ConfigureDataTypes(modelBuilder);

            // Configurar relaciones entre entidades
            ConfigureRelationships(modelBuilder);

            // Configurar índices para mejor performance
            ConfigureIndexes(modelBuilder);

            // Configurar valores por defecto
            ConfigureDefaultValues(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private void ConfigureTableNames(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Vehicle>().ToTable("vehicles");
            modelBuilder.Entity<Rate>().ToTable("rates");
            modelBuilder.Entity<Payment>().ToTable("payments");
            modelBuilder.Entity<Operator>().ToTable("operators");
            modelBuilder.Entity<Subscription>().ToTable("subscriptions");
            modelBuilder.Entity<Ticket>().ToTable("tickets");
            modelBuilder.Entity<Shift>().ToTable("shifts");
            modelBuilder.Entity<Mail>().ToTable("mails");
        }

        private void ConfigureDataTypes(ModelBuilder modelBuilder)
        {
            // Configurar decimales para PostgreSQL
            modelBuilder.Entity<Rate>()
                .Property(r => r.HourPrice)
                .HasColumnType("numeric(18,2)");
                
            modelBuilder.Entity<Rate>()
                .Property(r => r.AddPrice)
                .HasColumnType("numeric(18,2)");
                
            modelBuilder.Entity<Rate>()
                .Property(r => r.MaxPrice)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<Subscription>()
                .Property(s => s.MonthlyPrice)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<Ticket>()
                .Property(t => t.TotalAmount)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<Shift>()
                .Property(s => s.InitialCash)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<Shift>()
                .Property(s => s.FinalCash)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<Shift>()
                .Property(s => s.TotalCashPayments)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<Shift>()
                .Property(s => s.TotalCardPayments)
                .HasColumnType("numeric(18,2)");

            // Configurar strings con longitud máxima
            modelBuilder.Entity<User>()
                .Property(u => u.FullName)
                .HasMaxLength(255);

            modelBuilder.Entity<User>()
                .Property(u => u.Document)
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasMaxLength(255);

            modelBuilder.Entity<User>()
                .Property(u => u.Status)
                .HasMaxLength(50);

            modelBuilder.Entity<Vehicle>()
                .Property(v => v.Plate)
                .HasMaxLength(20);

            modelBuilder.Entity<Vehicle>()
                .Property(v => v.Type)
                .HasMaxLength(50);

            modelBuilder.Entity<Rate>()
                .Property(r => r.TypeVehicle)
                .HasMaxLength(50);

            modelBuilder.Entity<Rate>()
                .Property(r => r.GraceTime)
                .HasMaxLength(50);

            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentMethod)
                .HasMaxLength(50);

            modelBuilder.Entity<Payment>()
                .Property(p => p.ReferenceNumber)
                .HasMaxLength(100);

            modelBuilder.Entity<Operator>()
                .Property(o => o.FullName)
                .HasMaxLength(255);

            modelBuilder.Entity<Operator>()
                .Property(o => o.Document)
                .HasMaxLength(50);

            modelBuilder.Entity<Operator>()
                .Property(o => o.Email)
                .HasMaxLength(255);

            modelBuilder.Entity<Operator>()
                .Property(o => o.Status)
                .HasMaxLength(50);

            modelBuilder.Entity<Subscription>()
                .Property(s => s.Status)
                .HasMaxLength(50);

            modelBuilder.Entity<Ticket>()
                .Property(t => t.TicketNumber)
                .HasMaxLength(50);

            modelBuilder.Entity<Ticket>()
                .Property(t => t.QRCode)
                .HasMaxLength(500);

            modelBuilder.Entity<Mail>()
                .Property(w => w.Subject)
                .HasMaxLength(255);

            // Configurar fechas
            modelBuilder.Entity<Subscription>()
                .Property(s => s.StartDate)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Subscription>()
                .Property(s => s.EndDate)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Ticket>()
                .Property(t => t.EntryTime)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Ticket>()
                .Property(t => t.ExitTime)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentTime)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Shift>()
                .Property(s => s.StartTime)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Shift>()
                .Property(s => s.EndTime)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Mail>()
                .Property(w => w.SentAt)
                .HasColumnType("timestamp without time zone");
        }

        private void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            // User -> Vehicles (One to Many)
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.User)
                .WithMany(u => u.Vehicles)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Subscriptions (One to Many)
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Waits (One to Many)
            modelBuilder.Entity<Mail>()
                .HasOne(w => w.User)
                .WithMany(u => u.Mails)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Vehicle -> Subscriptions (One to Many)
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Vehicle)
                .WithMany(v => v.Subscriptions)
                .HasForeignKey(s => s.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Vehicle -> Tickets (One to Many)
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Vehicle)
                .WithMany(v => v.Tickets)
                .HasForeignKey(t => t.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Operator -> Payments (One to Many)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Operator)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OperatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Operator -> Tickets (One to Many)
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Operator)
                .WithMany(o => o.Tickets)
                .HasForeignKey(t => t.OperatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Operator -> Shifts (One to Many)
            modelBuilder.Entity<Shift>()
                .HasOne(s => s.Operator)
                .WithMany(o => o.Shifts)
                .HasForeignKey(s => s.OperatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Subscription -> Payments (One to Many)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Subscription)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.SubscriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Subscription -> Tickets (One to Many)
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Subscription)
                .WithMany(s => s.Tickets)
                .HasForeignKey(t => t.SubscriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Subscription -> Waits (One to Many)
            modelBuilder.Entity<Mail>()
                .HasOne(w => w.Subscription)
                .WithMany(s => s.Mails)
                .HasForeignKey(w => w.SubscriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Rate -> Tickets (One to Many)
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Rate)
                .WithMany(r => r.Tickets)
                .HasForeignKey(t => t.RateId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurar SubscriptionId como opcional en Ticket
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Subscription)
                .WithMany(s => s.Tickets)
                .HasForeignKey(t => t.SubscriptionId)
                .IsRequired(false);
        }

        private void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            // Índices para campos de búsqueda frecuente
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Document);

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.Plate)
                .IsUnique();

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.UserId);

            modelBuilder.Entity<Operator>()
                .HasIndex(o => o.Email)
                .IsUnique();

            modelBuilder.Entity<Operator>()
                .HasIndex(o => o.Document);

            modelBuilder.Entity<Subscription>()
                .HasIndex(s => s.UserId);

            modelBuilder.Entity<Subscription>()
                .HasIndex(s => s.VehicleId);

            modelBuilder.Entity<Subscription>()
                .HasIndex(s => s.Status);

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.TicketNumber)
                .IsUnique();

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.VehicleId);

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.OperatorId);

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.SubscriptionId);

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.RateId);

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.EntryTime);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.SubscriptionId);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.OperatorId);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.PaymentTime);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.ReferenceNumber)
                .IsUnique();

            modelBuilder.Entity<Shift>()
                .HasIndex(s => s.OperatorId);

            modelBuilder.Entity<Shift>()
                .HasIndex(s => s.StartTime);

            modelBuilder.Entity<Mail>()
                .HasIndex(w => w.UserId);

            modelBuilder.Entity<Mail>()
                .HasIndex(w => w.SubscriptionId);

            modelBuilder.Entity<Mail>()
                .HasIndex(w => w.SentAt);
        }

        private void ConfigureDefaultValues(ModelBuilder modelBuilder)
        {
            // Valores por defecto para Status
            modelBuilder.Entity<User>()
                .Property(u => u.Status)
                .HasDefaultValue("Active");

            modelBuilder.Entity<Operator>()
                .Property(o => o.Status)
                .HasDefaultValue("Active");

            modelBuilder.Entity<Operator>()
                .Property(o => o.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<Subscription>()
                .Property(s => s.Status)
                .HasDefaultValue("Active");

            // Valores por defecto para montos
            modelBuilder.Entity<Rate>()
                .Property(r => r.HourPrice)
                .HasDefaultValue(0m);

            modelBuilder.Entity<Rate>()
                .Property(r => r.AddPrice)
                .HasDefaultValue(0m);

            modelBuilder.Entity<Rate>()
                .Property(r => r.MaxPrice)
                .HasDefaultValue(0m);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasDefaultValue(0m);

            modelBuilder.Entity<Subscription>()
                .Property(s => s.MonthlyPrice)
                .HasDefaultValue(0m);

            modelBuilder.Entity<Ticket>()
                .Property(t => t.TotalAmount)
                .HasDefaultValue(0m);

            modelBuilder.Entity<Ticket>()
                .Property(t => t.TotalMinutes)
                .HasDefaultValue(0);

            modelBuilder.Entity<Shift>()
                .Property(s => s.InitialCash)
                .HasDefaultValue(0m);

            modelBuilder.Entity<Shift>()
                .Property(s => s.TotalCashPayments)
                .HasDefaultValue(0m);

            modelBuilder.Entity<Shift>()
                .Property(s => s.TotalCardPayments)
                .HasDefaultValue(0m);

            // Valores por defecto para fechas
            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentTime)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<Mail>()
                .Property(w => w.SentAt)
                .HasDefaultValueSql("NOW()");
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Aquí puedes agregar lógica adicional antes de guardar cambios
            // como auditoría, logs, etc.
            
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}