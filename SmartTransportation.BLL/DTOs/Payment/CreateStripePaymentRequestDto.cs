using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTransportation.BLL.DTOs.Payment
{
    public class CreateStripePaymentRequestDto
    {
        public int BookingId { get; set; }

        // for dynamic test cards
        public string PaymentMethodId { get; set; }
    }
}
