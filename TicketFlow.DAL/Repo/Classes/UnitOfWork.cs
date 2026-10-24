using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using TicketFlow.DAL.Context;
using TicketFlow.DAL.Models;
using TicketFlow.DAL.Repo.Interfaces;

namespace TicketFlow.DAL.Repo.Classes
{
    public class UnitOfWork :IUnitOfWork
    {
        private readonly DBContext _context;
        private IDbContextTransaction? _currentTransaction;
        private readonly ConcurrentDictionary<string, object> _repositories = new();

        public UnitOfWork(DBContext context)
        {
            _context = context;

            EventSeats = new EventSeatRepository(_context);
            Reservations = new ReservationRepository(_context);
            Events = new EventRepository(_context);
            Orders = new OrderRepository(_context);

            Places = new GenericRepository<Place>(_context);
            Seats = new GenericRepository<Seat>(_context);
            Payments = new GenericRepository<Payment>(_context);
            ReservationItems = new GenericRepository<ReservationItem>(_context);
            OrderItems = new GenericRepository<OrderItem>(_context);
        }
        public IEventSeatRepository EventSeats { get; }
        public IReservationRepository Reservations { get; }
        public IEventRepository Events { get; }
        public IOrderRepository Orders { get; }
        public IGenericRepository<Place> Places { get; }
        public IGenericRepository<Seat> Seats { get; }
        public IGenericRepository<ReservationItem> ReservationItems { get; }
        public IGenericRepository<OrderItem> OrderItems { get; }
        public IGenericRepository<Payment> Payments { get; }

        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            var typeName = typeof(TEntity).Name;

            return (IGenericRepository<TEntity>)_repositories.GetOrAdd(typeName, _ => new GenericRepository<TEntity>(_context));
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
                return;

            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await SaveChangesAsync(cancellationToken);

                if (_currentTransaction != null)
                {
                    await _currentTransaction.CommitAsync(cancellationToken);
                }
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.RollbackAsync(cancellationToken);
                }
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
            }

            await _context.DisposeAsync();
        }
    }
}