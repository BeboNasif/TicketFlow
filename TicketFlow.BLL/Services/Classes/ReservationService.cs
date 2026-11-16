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
        private readonly ISeatNotificationService _seatNotificationService;

        public ReservationService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ISeatNotificationService seatNotificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _seatNotificationService = seatNotificationService;
        }

        public async Task<ReservationDto> CreateReservationAsync(Guid userId, CreateReservationDto dto)
        {
            var distinctSeatIds = dto.EventSeatIds.Distinct().ToList();

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var availableSeats = await _unitOfWork.EventSeats.GetSeatsForHoldAsync(dto.EventId, distinctSeatIds);

                if (availableSeats.Count != distinctSeatIds.Count)
                {
                    throw new InvalidOperationException("One or more selected seats are no longer available.");
                }

                var reservation = new Reservation
                {
                    UserId = userId,
                    EventId = dto.EventId,
                    ReservedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    Status = ReservationStatusEnum.Pending
                };

                decimal totalAmount = 0;

                foreach (var seat in availableSeats)
                {
                    seat.Status = SeatStatusEnum.Reserved;
                    _unitOfWork.EventSeats.Update(seat);

                    reservation.ReservationItems.Add(new ReservationItem
                    {
                        EventSeatId = seat.Id,
                        Price = seat.Price
                    });

                    totalAmount += seat.Price;
                }

                reservation.TotalAmount = totalAmount;

                await _unitOfWork.Reservations.AddAsync(reservation);
                await _unitOfWork.CommitTransactionAsync();

                // 📢 Broadcast real-time update: Seats are now Reserved
                await _seatNotificationService.NotifySeatStatusChangedAsync(
                    dto.EventId,
                    distinctSeatIds,
                    SeatStatusEnum.Reserved.ToString()
                );

                var createdReservation = await _unitOfWork.Reservations.GetReservationWithItemsAsync(reservation.Id);
                return _mapper.Map<ReservationDto>(createdReservation);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> CancelReservationAsync(Guid userId, Guid reservationId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var reservation = await _unitOfWork.Reservations.GetReservationWithItemsAsync(reservationId);

                if (reservation == null || reservation.UserId != userId)
                    return false;

                if (reservation.Status != ReservationStatusEnum.Pending)
                    throw new InvalidOperationException("Only pending reservations can be cancelled.");

                reservation.Status = ReservationStatusEnum.Cancelled;

                var releasedSeatIds = new List<Guid>();

                foreach (var item in reservation.ReservationItems)
                {
                    if (item.EventSeat != null)
                    {
                        item.EventSeat.Status = SeatStatusEnum.Available;
                        _unitOfWork.EventSeats.Update(item.EventSeat);
                        releasedSeatIds.Add(item.EventSeatId);
                    }
                }

                _unitOfWork.Reservations.Update(reservation);
                await _unitOfWork.CommitTransactionAsync();

                // 📢 Broadcast real-time update: Seats released back to Available
                if (releasedSeatIds.Any())
                {
                    await _seatNotificationService.NotifySeatStatusChangedAsync(
                        reservation.EventId,
                        releasedSeatIds,
                        SeatStatusEnum.Available.ToString()
                    );
                }

                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<int> ProcessExpiredReservationsAsync(CancellationToken cancellationToken = default)
        {
            var expiredReservations = await _unitOfWork.Reservations.GetExpiredPendingReservationsAsync(DateTime.UtcNow, cancellationToken);

            if (!expiredReservations.Any())
                return 0;

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // Track released seats grouped by EventId for notification batching
            var releasedSeatsByEvent = new Dictionary<Guid, List<Guid>>();

            try
            {
                foreach (var reservation in expiredReservations)
                {
                    reservation.Status = ReservationStatusEnum.Expired;

                    if (!releasedSeatsByEvent.ContainsKey(reservation.EventId))
                    {
                        releasedSeatsByEvent[reservation.EventId] = new List<Guid>();
                    }

                    foreach (var item in reservation.ReservationItems)
                    {
                        if (item.EventSeat != null)
                        {
                            item.EventSeat.Status = SeatStatusEnum.Available;
                            _unitOfWork.EventSeats.Update(item.EventSeat);
                            releasedSeatsByEvent[reservation.EventId].Add(item.EventSeatId);
                        }
                    }

                    _unitOfWork.Reservations.Update(reservation);
                }

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                // 📢 Broadcast real-time update per event: Seats are now Available
                foreach (var (eventId, seatIds) in releasedSeatsByEvent)
                {
                    if (seatIds.Any())
                    {
                        await _seatNotificationService.NotifySeatStatusChangedAsync(
                            eventId,
                            seatIds,
                            SeatStatusEnum.Available.ToString()
                        );
                    }
                }

                return expiredReservations.Count;
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
    }
}
