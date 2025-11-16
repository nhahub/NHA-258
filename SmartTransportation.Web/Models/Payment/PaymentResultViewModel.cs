namespace SmartTransportation.Web.Models.Payment
{
    public class PaymentResultViewModel
    {
        public int BookingId { get; set; }
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string? ErrorMessage { get; set; }
    }
}
