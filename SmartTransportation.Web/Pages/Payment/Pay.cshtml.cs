using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using SmartTransportation.Web.Helpers;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
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

        [BindProperty(SupportsGet = true)]
        public int BookingId { get; set; }

        [BindProperty]
        public string PaymentMethodId { get; set; }

        [BindProperty]
        public string ResultStatus { get; set; }

        [BindProperty]
        public string ResultMessage { get; set; }

        public string RedirectUrl { get; set; }

        public string StripePublishableKey => _config["Stripe:PublishableKey"];

        private int? CurrentUserId => ClaimsHelper.GetUserId(User);

        public void OnGet(int bookingId)
        {
            BookingId = bookingId;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (CurrentUserId == null)
                return SetResult("Failed", "You must be logged in to pay for a booking.");

            if (BookingId <= 0 || string.IsNullOrWhiteSpace(PaymentMethodId))
                return SetResult("Failed", "Invalid booking or payment data.");

            var client = _httpClientFactory.CreateClient();
            var apiBase = _config["ApiBaseUrl"] ?? throw new InvalidOperationException("ApiBaseUrl is not configured.");

            // Attach JWT token from session
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // 1️⃣ Verify booking ownership
            BookingDto booking;
            try
            {
                booking = await client.GetFromJsonAsync<BookingDto>($"{apiBase}/api/bookings/{BookingId}");
                if (booking == null || booking.BookerUserId != CurrentUserId.Value)
                    return SetResult("Failed", "This booking does not belong to you.");
            }
            catch
            {
                return SetResult("Failed", "Unable to verify booking ownership.");
            }

            // 2️⃣ Prepare Stripe request
            var apiRequest = new
            {
                BookingId,
                PaymentMethodId
            };

            try
            {
                var response = await client.PostAsJsonAsync($"{apiBase}/api/StripePayment/create-and-confirm", apiRequest);

                if (!response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var errorObj = JsonSerializer.Deserialize<ApiErrorDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return SetResult("Failed", errorObj?.Message ?? $"Payment failed: {content}");
                    }
                    catch
                    {
                        return SetResult("Failed", $"Payment failed: {content}");
                    }
                }

                var result = await response.Content.ReadFromJsonAsync<CreateStripePaymentResponseDto>();
                if (result == null)
                    return SetResult("Failed", "Payment failed: empty API response.");

                if (string.Equals(result.Status, "Succeeded", StringComparison.OrdinalIgnoreCase))
                {
                    ResultStatus = "Succeeded";
                    ResultMessage = "Payment completed successfully 🎉";
                    RedirectUrl = Url.Page("/customer-profile");
                    return Page();
                }

                return SetResult(result.Status, $"Payment status: {result.Status}");
            }
            catch (Exception ex)
            {
                return SetResult("Failed", $"Payment failed: {ex.Message}");
            }
        }

        private IActionResult SetResult(string status, string message)
        {
            ResultStatus = status;
            ResultMessage = message;
            return Page();
        }

        // ---------------- DTOs ----------------
        private class CreateStripePaymentResponseDto
        {
            public int PaymentId { get; set; }
            public int BookingId { get; set; }
            public decimal Amount { get; set; }
            public string Currency { get; set; }
            public string Status { get; set; }
            public string ClientSecret { get; set; }
        }

        private class BookingDto
        {
            public int BookingId { get; set; }
            public int BookerUserId { get; set; }
        }

        private class ApiErrorDto
        {
            public string Message { get; set; }
            public string Error { get; set; }
        }
    }
}
