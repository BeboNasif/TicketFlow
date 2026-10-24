using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TicketFlow.DAL.Models;

namespace TicketFlow.DAL.Context
{
    public class DBContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options) { }

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
        }
    }
}