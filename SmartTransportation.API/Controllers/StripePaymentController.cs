using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTransportation.BLL.DTOs.Payment;
using SmartTransportation.BLL.Services;
using SmartTransportation.DAL.Repositories.UnitOfWork;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTransportation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all actions
    public class StripePaymentController : BaseApiController
    {
        private readonly StripePaymentService _stripePaymentService;
        private readonly IUnitOfWork _unitOfWork;

        public StripePaymentController(StripePaymentService stripePaymentService, IUnitOfWork unitOfWork)
        {
            _stripePaymentService = stripePaymentService;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Create a Stripe PaymentIntent and optionally confirm it immediately.
        /// Only the booking owner can create payment for their booking.
        /// </summary>
        [HttpPost("create-and-confirm")]
        public async Task<ActionResult<CreateStripePaymentResponseDto>> CreateAndConfirm([FromBody] CreateStripePaymentRequestDto dto)
        {
            if (dto == null || dto.BookingId <= 0)
                return BadRequest(new { Message = "BookingId is required." });

            try
            {
                var booking = await _unitOfWork.Bookings.GetByIdAsync(dto.BookingId);
                if (booking == null)
                    return NotFound(new { Message = "Booking not found." });

                if (booking.BookerUserId != CurrentUserId)
                    return StatusCode(403, new { Message = "This booking does not belong to you." });

                var (payment, clientSecret) = await _stripePaymentService.CreatePaymentIntentAsync(dto.BookingId);

                if (!string.IsNullOrWhiteSpace(dto.PaymentMethodId))
                {
                    payment = await _stripePaymentService.ConfirmPaymentAsync(payment.PaymentId, dto.PaymentMethodId);
                }

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
                return StatusCode(500, new { Message = "Error processing Stripe payment.", Error = ex.Message });
            }
        }

        /// <summary>
        /// Get payment status by payment ID.
        /// Only the booking owner can access.
        /// </summary>
        [HttpGet("{paymentId}/status")]
        public async Task<IActionResult> GetStatus(int paymentId)
        {
            try
            {
                var payment = await _stripePaymentService.RefreshStatusFromStripeAsync(paymentId);

                var booking = await _unitOfWork.Bookings.GetByIdAsync(payment.BookingId);
                if (booking.BookerUserId != CurrentUserId)
                    return StatusCode(403, new { Message = "You cannot access this payment." });

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

        /// <summary>
        /// List all payments for the current user.
        /// </summary>
        [HttpGet("my-payments")]
        public async Task<IActionResult> GetMyPayments()
        {
            try
            {
                var payments = await _stripePaymentService.GetPaymentsByPassengerIdAsync(CurrentUserId.Value);

                var dtoList = payments.Select(p => new PaymentResponseDto
                {
                    PaymentId = p.PaymentId,
                    BookingId = p.BookingId,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    Status = p.Status,
                    PaidAt = p.PaidAt,
                    PassengerId = CurrentUserId.Value
                }).ToList();

                return Ok(dtoList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error getting your payments.", Error = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific payment by ID.
        /// Only the booking owner can access.
        /// </summary>
        [HttpGet("{paymentId:int}")]
        public async Task<IActionResult> GetById(int paymentId)
        {
            try
            {
                var payment = await _stripePaymentService.GetPaymentByIdAsync(paymentId);

                var booking = await _unitOfWork.Bookings.GetByIdAsync(payment.BookingId);
                if (booking.BookerUserId != CurrentUserId)
                    return StatusCode(403, new { Message = "You cannot access this payment." });

                return Ok(new
                {
                    payment.PaymentId,
                    payment.BookingId,
                    payment.Amount,
                    payment.Currency,
                    payment.Status,
                    payment.StripePaymentIntentId,
                    payment.PaidAt,
                    payment.CreatedAt,
                    payment.LastError
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error getting payment.", Error = ex.Message });
            }
        }
    }
}
