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
            _apiBaseUrl = config["ApiBaseUrl"] ?? "https://localhost:7004"; 
            _stripePublishableKey = config["Stripe:PublishableKey"] ?? throw new InvalidOperationException("Stripe publishable key not configured.");
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
                // Optionally: PlatformFeeAmount = TODO: compute or load via API
            };

            return View(model);
        }

        // POST: /Payment/Pay
        // This receives BookingId + PaymentMethodId from the Stripe.js frontend.
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

            // This must match your API DTO
            var apiRequest = new
            {
                bookingId = model.BookingId,
                paymentMethodId = model.PaymentMethodId
            };

            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/StripePayment/create-and-confirm", apiRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                var errorViewModel = new PaymentResultViewModel
                {
                    BookingId = model.BookingId,
                    Status = "Failed",
                    ErrorMessage = errorText
                };
                return View("PaymentResult", errorViewModel);
            }

            var apiResult = await response.Content.ReadFromJsonAsync<CreateStripePaymentResponseDto>();

            if (apiResult == null)
            {
                var errorViewModel = new PaymentResultViewModel
                {
                    BookingId = model.BookingId,
                    Status = "Failed",
                    ErrorMessage = "Empty response from payment API."
                };
                return View("PaymentResult", errorViewModel);
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































//using Microsoft.AspNetCore.Mvc;
//using SmartTransportation.Web.Models;
//using SmartTransportation.BLL.DTOs.Payment;
//using SmartTransportation.Web.Models.Payment;

//public class PaymentController : Controller
//{
//    private readonly IHttpClientFactory _clientFactory;

//    public PaymentController(IHttpClientFactory clientFactory)
//    {
//        _clientFactory = clientFactory;
//    }

//    [HttpGet]
//    public IActionResult Pay(int bookingId)
//    {
//        return View(new CreateStripePaymentRequestDto { BookingId = bookingId });
//    }

//    [HttpPost]
//    public async Task<IActionResult> Pay(CreateStripePaymentRequestDto dto)
//    {
//        var client = _clientFactory.CreateClient();
//        client.BaseAddress = new Uri("https://localhost:5001/"); // API URL

//        var response = await client.PostAsJsonAsync("api/StripePayment/create", dto);

//        if (!response.IsSuccessStatusCode)
//        {
//            ModelState.AddModelError("", "Could not create payment");
//            return View(dto);
//        }

//        var result = await response.Content.ReadFromJsonAsync<CreateStripePaymentResponseDto>();

//        // Send to payment UI
//        return RedirectToAction("Confirm", new
//        {
//            clientSecret = result.ClientSecret,
//            amount = result.Amount,
//            currency = result.Currency,
//            paymentId = result.PaymentId,
//            bookingId = dto.BookingId
//        });

//    }

//    [HttpGet]
//    public IActionResult Confirm(string clientSecret, decimal amount, string currency, int paymentId, int bookingId)
//    {
//        var model = new ConfirmStripePaymentViewModel
//        {
//            ClientSecret = clientSecret,
//            Amount = amount,
//            Currency = currency,
//            PaymentId = paymentId,
//            BookingId = bookingId
//        };

//        return View(model);
//    }

//}
