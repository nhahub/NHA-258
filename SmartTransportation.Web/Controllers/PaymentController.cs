using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SmartTransportation.BLL.DTOs.Payment;
using SmartTransportation.Web.Models.Payment;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SmartTransportation.Web.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;
        private readonly string _stripePublishableKey;

        public PaymentController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = config["ApiBaseUrl"] ?? "https://localhost:5001"; // adjust
            _stripePublishableKey = config["Stripe:PublishableKey"]
                ?? throw new InvalidOperationException("Stripe publishable key not configured.");
        }

        // GET: /Payment/Pay/5
        [HttpGet]
        public IActionResult Pay(int bookingId)
        {
            var model = new PaymentStartViewModel
            {
                BookingId = bookingId,
                StripePublishableKey = _stripePublishableKey,
                Currency = "EGP"
                // Optionally: PlatformFeeAmount = precomputed or from some API
            };

            return View(model);
        }

        // POST: /Payment/Pay
        // Receives BookingId + PaymentMethodId from the form (Stripe.js created it)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(PaymentRequestViewModel model)
        {
            if (!ModelState.IsValid || model.BookingId <= 0 || string.IsNullOrWhiteSpace(model.PaymentMethodId))
            {
                ModelState.AddModelError("", "Invalid payment data.");
                return View("Error");
            }

            var client = _httpClientFactory.CreateClient();

            var apiRequest = new
            {
                bookingId = model.BookingId,
                paymentMethodId = model.PaymentMethodId   // maps to your API DTO
            };

            var response = await client.PostAsJsonAsync(
                $"{_apiBaseUrl}/api/StripePayment/create-and-confirm",
                apiRequest
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                var errorVm = new PaymentResultViewModel
                {
                    BookingId = model.BookingId,
                    Status = "Failed",
                    ErrorMessage = errorText
                };
                return View("PaymentResult", errorVm);
            }

            var apiResult = await response.Content.ReadFromJsonAsync<CreateStripePaymentResponseDto>();

            if (apiResult == null)
            {
                var errorVm = new PaymentResultViewModel
                {
                    BookingId = model.BookingId,
                    Status = "Failed",
                    ErrorMessage = "Empty response from payment API."
                };
                return View("PaymentResult", errorVm);
            }

            var vm = new PaymentResultViewModel
            {
                BookingId = apiResult.BookingId,
                PaymentId = apiResult.PaymentId,
                Amount = apiResult.Amount,
                Currency = apiResult.Currency,
                Status = apiResult.Status
            };

            return View("PaymentResult", vm);
        }
    }
}
