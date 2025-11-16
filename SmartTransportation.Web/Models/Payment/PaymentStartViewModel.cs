namespace SmartTransportation.Web.Models.Payment
{
    public class PaymentStartViewModel
    {
        public int BookingId { get; set; }

        // Optional: show amount (platform fee) on the page if you want.
        public decimal? PlatformFeeAmount { get; set; }
        public string Currency { get; set; } = "EGP";

        public string StripePublishableKey { get; set; } = default!;
    }
}
