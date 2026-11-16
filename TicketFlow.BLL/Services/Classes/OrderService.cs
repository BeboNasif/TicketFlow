using AutoMapper;
using TicketFlow.BLL.DTOs.Order;
using TicketFlow.BLL.Services.Interfaces;
using TicketFlow.DAL.Models;
using TicketFlow.DAL.Repo.Interfaces;

namespace TicketFlow.BLL.Services.Classes
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ISeatNotificationService _seatNotificationService;

        public OrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ISeatNotificationService seatNotificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _seatNotificationService = seatNotificationService;
        }

        public async Task<OrderDto> CheckoutAsync(Guid userId, CreateOrderDto dto)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var reservation = await _unitOfWork.Reservations.GetReservationWithItemsAsync(dto.ReservationId);

                if (reservation == null || reservation.UserId != userId)
                    throw new KeyNotFoundException("Reservation not found or access denied.");

                if (reservation.Status != ReservationStatusEnum.Pending)
                    throw new InvalidOperationException("Reservation is no longer active.");

                if (reservation.ExpiresAt <= DateTime.UtcNow)
                    throw new InvalidOperationException("Reservation has expired.");

                var totalAmount = reservation.ReservationItems.Sum(ri => ri.Price);

                var order = new Order
                {
                    UserId = userId,
                    TotalAmount = totalAmount,
                    Status = OrderStatusEnum.Paid,
                    CreatedAt = DateTime.UtcNow
                };

                order.Payment = new Payment
                {
                    TransactionId = $"TXN-{Guid.NewGuid().ToString("N")[..12].ToUpper()}",
                    PaymentGateway = dto.PaymentMethod,
                    Amount = totalAmount,
                    Status = PaymentStatusEnum.Success,
                    ProcessedAt = DateTime.UtcNow
                };

                var soldSeatIds = new List<Guid>();

                foreach (var item in reservation.ReservationItems)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        EventSeatId = item.EventSeatId,
                        Price = item.Price
                    });

                    item.EventSeat.Status = SeatStatusEnum.Sold;
                    _unitOfWork.EventSeats.Update(item.EventSeat);
                    soldSeatIds.Add(item.EventSeatId);
                }

                reservation.Status = ReservationStatusEnum.Confirmed;
                _unitOfWork.Reservations.Update(reservation);

                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.CommitTransactionAsync();

                // 📢 Broadcast real-time update: Seats are permanently Sold
                await _seatNotificationService.NotifySeatStatusChangedAsync(
                    reservation.EventId,
                    soldSeatIds,
                    SeatStatusEnum.Sold.ToString()
                );

                var createdOrder = await _unitOfWork.Orders.GetOrderWithDetailsAsync(order.Id);
                return _mapper.Map<OrderDto>(createdOrder);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid userId, Guid orderId)
        {
            var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(orderId);
            if (order == null || order.UserId != userId)
                return null;

            return _mapper.Map<OrderDto>(order);
        }

        public async Task<IReadOnlyList<OrderDto>> GetUserOrdersAsync(Guid userId)
        {
            var orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId);
            return _mapper.Map<IReadOnlyList<OrderDto>>(orders);
        }
    }
}