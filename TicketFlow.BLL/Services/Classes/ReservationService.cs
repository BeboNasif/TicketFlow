using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using TicketFlow.BLL.DTOs.Reservation;
using TicketFlow.BLL.Services.Interfaces;
using TicketFlow.DAL.Models;
using TicketFlow.DAL.Repo.Interfaces;

namespace TicketFlow.BLL.Services.Classes
{
    public class ReservationService : IReservationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReservationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ReservationDto> CreateReservationAsync(Guid userId, CreateReservationDto dto)
        {
            var distinctSeatIds = dto.EventSeatIds.Distinct().ToList();

            // 1. Begin explicit database transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 2. Lock requested seats in Available state
                var availableSeats = await _unitOfWork.EventSeats.GetSeatsForHoldAsync(dto.EventId, distinctSeatIds);

                if (availableSeats.Count != distinctSeatIds.Count)
                {
                    throw new InvalidOperationException("One or more selected seats are no longer available.");
                }

                // 3. Instantiate Reservation
                var reservation = new Reservation
                {
                    UserId = userId,
                    EventId = dto.EventId,
                    ReservedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10), // 10-minute hold window
                    Status = ReservationStatusEnum.Pending
                };

                // 4. Update seats and construct reservation items
                foreach (var seat in availableSeats)
                {
                    seat.Status = SeatStatusEnum.Reserved;
                    _unitOfWork.EventSeats.Update(seat);

                    reservation.ReservationItems.Add(new ReservationItem
                    {
                        EventSeatId = seat.Id,
                        Price = seat.Price
                    });
                }

                await _unitOfWork.Reservations.AddAsync(reservation);

                // 5. Commit transaction (triggers EF Core concurrency validation via PostgreSQL xmin)
                await _unitOfWork.CommitTransactionAsync();

                var createdReservation = await _unitOfWork.Reservations.GetReservationWithItemsAsync(reservation.Id);
                return _mapper.Map<ReservationDto>(createdReservation);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ReservationDto?> GetReservationByIdAsync(Guid userId, Guid reservationId)
        {
            var reservation = await _unitOfWork.Reservations.GetReservationWithItemsAsync(reservationId);
            if (reservation == null || reservation.UserId != userId)
                return null;

            return _mapper.Map<ReservationDto>(reservation);
        }

        public async Task<bool> CancelReservationAsync(Guid userId, Guid reservationId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var reservation = await _unitOfWork.Reservations.GetReservationWithItemsAsync(reservationId);
                if (reservation == null || reservation.UserId != userId || reservation.Status != ReservationStatusEnum.Pending)
                {
                    return false;
                }

                reservation.Status = ReservationStatusEnum.Cancelled;

                // Release reserved seats back to Available
                foreach (var item in reservation.ReservationItems)
                {
                    item.EventSeat.Status = SeatStatusEnum.Available;
                    _unitOfWork.EventSeats.Update(item.EventSeat);
                }

                _unitOfWork.Reservations.Update(reservation);
                await _unitOfWork.CommitTransactionAsync();

                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}