namespace SmartTransportation.Web.Models.Payment
{
    public class CreateStripePaymentViewModel
    {
        public string ClientSecret { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public int PaymentId { get; set; }
        public int BookingId { get; set; }  
    }
}
