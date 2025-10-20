using Microsoft.EntityFrameworkCore;
using AutoSpace.Models;

namespace AutoSpace.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Operator> Operators { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Rate> Rates { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Mail> Mails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User - Vehicle (1:N)
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.User)
                .WithMany(u => u.Vehicles)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - Subscription (1:N)
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Vehicle - Subscription (1:N)
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Vehicle)
                .WithMany(v => v.Subscriptions)
                .HasForeignKey(s => s.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Vehicle - Ticket (1:N)
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Vehicle)
                .WithMany(v => v.Tickets)
                .HasForeignKey(t => t.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Operator - Ticket (1:N)
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Operator)
                .WithMany(o => o.Tickets)
                .HasForeignKey(t => t.OperatorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Subscription - Ticket (1:N)
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Subscription)
                .WithMany(s => s.Tickets)
                .HasForeignKey(t => t.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull);

            // Rate - Ticket (1:N)
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Rate)
                .WithMany()
                .HasForeignKey(t => t.RateId)
                .OnDelete(DeleteBehavior.SetNull);

            // Ticket - Payment (1:N)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Ticket)
                .WithMany(t => t.Payments)
                .HasForeignKey(p => p.TicketId)
                .OnDelete(DeleteBehavior.SetNull);

            // Subscription - Payment (1:N)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Subscription)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull);

            // Operator - Payment (1:N)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Operator)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OperatorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Operator - Shift (1:N)
            modelBuilder.Entity<Shift>()
                .HasOne(s => s.Operator)
                .WithMany(o => o.Shifts)
                .HasForeignKey(s => s.OperatorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}