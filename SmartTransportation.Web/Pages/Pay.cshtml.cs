using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SmartTransportation.Web.Pages.Payment
{
    public class PayModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public PayModel(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        // Read from query string on GET
        [BindProperty(SupportsGet = true)]
        public int BookingId { get; set; }
        // Filled from hidden input when Stripe creates PaymentMethod
        [BindProperty]
        public string PaymentMethodId { get; set; }
        // For showing result UI
        [BindProperty]
        public string ResultStatus { get; set; }
        [BindProperty]
        public string ResultMessage { get; set; }
        // For redirect after success
        public string RedirectUrl { get; set; }
        // Stripe publishable key for JS
        public string StripePublishableKey => _config["Stripe:PublishableKey"];
        public void OnGet(int bookingId)
        {
            BookingId = bookingId;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (BookingId <= 0 || string.IsNullOrWhiteSpace(PaymentMethodId))
            {
                ResultMessage = "Invalid booking or payment data.";
                ResultStatus = "Failed";
                return Page();
            }

            var client = _httpClientFactory.CreateClient();
            var apiBase = _config["ApiBaseUrl"] ?? throw new InvalidOperationException("ApiBaseUrl is not configured.");

            var apiRequest = new
            {
                bookingId = BookingId,
                paymentMethodId = PaymentMethodId
            };

            var response = await client.PostAsJsonAsync(
                $"{apiBase}/api/StripePayment/create-and-confirm",
                apiRequest
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                ResultMessage = $"Payment failed: {error}";
                ResultStatus = "Failed";
                return Page();
            }

            var result = await response.Content.ReadFromJsonAsync<CreateStripePaymentResponseDto>();

            if (result == null)
            {
                ResultMessage = "Payment failed: empty API response.";
                ResultStatus = "Failed";
                return Page();
            }

            if (string.Equals(result.Status, "Succeeded", StringComparison.OrdinalIgnoreCase))
            {
                ResultMessage = "Payment completed successfully 🎉";
                ResultStatus = "Succeeded";

                // where you want to send them after success
                RedirectUrl = Url.Page("/Bookings/Index"); // change to your real bookings page
                return Page();
            }

            // Pending / Failed / other
            ResultMessage = $"Payment status: {result.Status}";
            ResultStatus = result.Status;

            return Page();
        }

        // Local DTO matching API response
        private class CreateStripePaymentResponseDto
        {
            public int PaymentId { get; set; }
            public int BookingId { get; set; }
            public decimal Amount { get; set; }
            public string Currency { get; set; }
            public string Status { get; set; }
            public string ClientSecret { get; set; }
        }
    }
}

