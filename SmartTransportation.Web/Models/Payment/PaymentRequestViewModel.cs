namespace SmartTransportation.Web.Models.Payment
{
    public class PaymentRequestViewModel
    {
        public int BookingId { get; set; }
        public string PaymentMethodId { get; set; } = default!;
    }
}
