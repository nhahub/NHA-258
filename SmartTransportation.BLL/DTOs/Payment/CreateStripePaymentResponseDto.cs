namespace SmartTransportation.BLL.DTOs.Payment
{
    public class CreateStripePaymentResponseDto
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }

        // This goes to the frontend to confirm the payment via Stripe.js
        //public string ClientSecret { get; set; }
    }
}
