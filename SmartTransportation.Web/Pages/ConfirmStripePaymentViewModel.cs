namespace MVC.Models
{
    public class ConfirmStripePaymentViewModel
    {
        public string ClientSecret { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public int PaymentId { get; set; }
        public int BookingId { get; set; }  // optional but useful for redirect after success
    }
}
