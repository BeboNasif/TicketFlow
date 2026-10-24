using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.DAL.Models
{
    public enum EventStatusEnum { Draft, Published, Completed, Cancelled }
    public enum SeatStatusEnum { Available, Reserved, Sold }
    public enum ReservationStatusEnum { Pending, Confirmed, Expired, Cancelled }
    public enum OrderStatusEnum{ Pending, Paid, Failed, Refunded }
    public enum PaymentStatusEnum{ Pending, Success, Failed }
}