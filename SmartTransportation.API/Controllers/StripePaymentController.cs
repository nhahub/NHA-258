using Microsoft.AspNetCore.Mvc;
using SmartTransportation.BLL.DTOs.Payment;
using SmartTransportation.BLL.Services;
using System;
using System.Threading.Tasks;

namespace SmartTransportation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StripePaymentController : ControllerBase
    {
        private readonly StripePaymentService _stripePaymentService;

        public StripePaymentController(StripePaymentService stripePaymentService)
        {
            _stripePaymentService = stripePaymentService;
        }

        [HttpPost("create")]
        public async Task<ActionResult<CreateStripePaymentResponseDto>> Create([FromBody] CreateStripePaymentRequestDto dto)
        {
            if (dto == null || dto.BookingId <= 0)
                return BadRequest(new { Message = "BookingId is required." });

            try
            {
                var (payment, clientSecret) = await _stripePaymentService.CreatePaymentIntentAsync(dto.BookingId);

                var response = new CreateStripePaymentResponseDto
                {
                    PaymentId = payment.PaymentId,
                    BookingId = payment.BookingId,
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    Status = payment.Status,
                    ClientSecret = clientSecret
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error creating Stripe payment.", Error = ex.Message });
            }
        }

        // Optional: endpoint to refresh status without webhooks
        [HttpGet("{paymentId}/status")]
        public async Task<IActionResult> GetStatus(int paymentId)
        {
            try
            {
                var payment = await _stripePaymentService.RefreshStatusFromStripeAsync(paymentId);
                return Ok(new
                {
                    payment.PaymentId,
                    payment.BookingId,
                    payment.Amount,
                    payment.Currency,
                    payment.Status,
                    payment.PaidAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error getting payment status.", Error = ex.Message });
            }
        }
    }
}
