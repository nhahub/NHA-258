using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTransportation.BLL.DTOs.Payment
{
    public class PaymentResponseDto
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        // Optional, only if you want to expose it (requires Booking join)
        public int? PassengerId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = default!;
        public string Status { get; set; } = default!;
        public DateTime? PaidAt { get; set; }
    }
}
