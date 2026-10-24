using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketFlow.DAL.Models;

namespace TicketFlow.DAL.Repo.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IEventSeatRepository EventSeats { get; }
        IReservationRepository Reservations { get; }
        IEventRepository Events { get; }
        IOrderRepository Orders { get; }

        
        IGenericRepository<Place> Places { get; }
        IGenericRepository<Seat> Seats { get; }
        IGenericRepository<ReservationItem> ReservationItems { get; }
        IGenericRepository<OrderItem> OrderItems { get; }
        IGenericRepository<Payment> Payments { get; }

        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}