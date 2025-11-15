using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using SmartTransportation.BLL.DTOs.Payment;

public class PaymentController : Controller
{
    private readonly IHttpClientFactory _clientFactory;

    public PaymentController(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    [HttpGet]
    public IActionResult Pay(int bookingId)
    {
        return View(new CreateStripePaymentRequestDto { BookingId = bookingId });
    }

    [HttpPost]
    public async Task<IActionResult> Pay(CreateStripePaymentRequestDto dto)
    {
        var client = _clientFactory.CreateClient();
        client.BaseAddress = new Uri("https://localhost:5001/"); // API URL

        var response = await client.PostAsJsonAsync("api/StripePayment/create", dto);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Could not create payment");
            return View(dto);
        }

        var result = await response.Content.ReadFromJsonAsync<CreateStripePaymentResponseDto>();

        // Send to payment UI
        return RedirectToAction("Confirm", new
        {
            clientSecret = result.ClientSecret,
            amount = result.Amount,
            currency = result.Currency,
            paymentId = result.PaymentId,
            bookingId = dto.BookingId
        });

    }

    [HttpGet]
    public IActionResult Confirm(string clientSecret, decimal amount, string currency, int paymentId, int bookingId)
    {
        var model = new ConfirmStripePaymentViewModel
        {
            ClientSecret = clientSecret,
            Amount = amount,
            Currency = currency,
            PaymentId = paymentId,
            BookingId = bookingId
        };

        return View(model);
    }

}
