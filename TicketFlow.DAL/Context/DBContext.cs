using System;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TicketFlow.DAL.Models;

namespace TicketFlow.DAL.Context
{
    public class DBContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options) { }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Place> Places => Set<Place>();
        public DbSet<Seat> Seats => Set<Seat>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<EventSeat> EventSeats => Set<EventSeat>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<ReservationItem> ReservationItems => Set<ReservationItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. PostgreSQL Optimistic Concurrency Tokens (xmin system column)
            builder.Entity<Event>()
                .Property(e => e.Version)
                .IsRowVersion();

            builder.Entity<EventSeat>()
                .Property(es => es.Version)
                .IsRowVersion();

            // 2. Precision for Monetary Values
            builder.Entity<EventSeat>()
                .Property(es => es.Price)
                .HasPrecision(18, 2);

            builder.Entity<ReservationItem>()
                .Property(ri => ri.Price)
                .HasPrecision(18, 2);

            builder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            builder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasPrecision(18, 2);

            builder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            // 3. Event Foreign Keys & Delete Behaviors
            builder.Entity<Event>()
                .HasOne(e => e.Place)
                .WithMany(p => p.Events)
                .HasForeignKey(e => e.PlaceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Event>()
                .HasOne(e => e.Organizer)
                .WithMany(u => u.OrganizedEvents)
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Event>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. Reservation Foreign Keys & Delete Behaviors
            builder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Reservation>()
                .HasOne(r => r.Event)
                .WithMany(e => e.Reservations)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            // 5. Order & Reservation 1-to-1 Relationship
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne(o => o.Reservation)
                .WithOne(r => r.Order)
                .HasForeignKey<Order>(o => o.ReservationId)
                .OnDelete(DeleteBehavior.SetNull);

            // 6. Payment 1-to-1 Relationship
            builder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Entity<Seat>()
                .HasOne(s => s.Place)
                .WithMany(p => p.Seats)
                .HasForeignKey(s => s.PlaceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}