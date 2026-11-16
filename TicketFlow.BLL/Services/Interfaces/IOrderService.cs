using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketFlow.BLL.DTOs.Order;

namespace TicketFlow.BLL.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CheckoutAsync(Guid userId, CreateOrderDto dto);
        Task<OrderDto?> GetOrderByIdAsync(Guid userId, Guid orderId);
        Task<IReadOnlyList<OrderDto>> GetUserOrdersAsync(Guid userId);
    }
}