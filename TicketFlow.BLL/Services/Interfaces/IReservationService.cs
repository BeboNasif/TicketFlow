using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketFlow.BLL.DTOs.Reservation;

namespace TicketFlow.BLL.Services.Interfaces
{
    public interface IReservationService
    {
        Task<ReservationDto> CreateReservationAsync(Guid userId, CreateReservationDto dto);
        Task<ReservationDto?> GetReservationByIdAsync(Guid userId, Guid reservationId);
        Task<bool> CancelReservationAsync(Guid userId, Guid reservationId);
        Task<int> ProcessExpiredReservationsAsync(CancellationToken cancellationToken = default);
    }
}