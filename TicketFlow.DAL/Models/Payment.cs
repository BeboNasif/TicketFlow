using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.DAL.Models
{
    public class Payment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }

        [Required]
        [MaxLength(100)]
        public string TransactionId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string PaymentGateway { get; set; } = string.Empty;

        [Precision(18, 2)]
        public decimal Amount { get; set; }
        public PaymentStatusEnum Status { get; set; } = PaymentStatusEnum.Pending;
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey(nameof(OrderId))]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public Order Order { get; set; } = null!;
    }
}