namespace TicketFlow.DAL.Repo.Classes;

using Microsoft.EntityFrameworkCore;
using TicketFlow.DAL.Context;
using TicketFlow.DAL.Models;
using TicketFlow.DAL.Repo.Interfaces;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(DBContext context) : base(context)
    {
    }

    public async Task<Order?> GetOrderWithDetailsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Payment)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.EventSeat)
                    .ThenInclude(es => es.Seat)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetOrdersByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
                    .Where(o => o.UserId == userId)
                    .Include(o => o.Payment)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.EventSeat)
                            .ThenInclude(es => es.Seat)
                    .OrderByDescending(o => o.CreatedAt)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
    }
}