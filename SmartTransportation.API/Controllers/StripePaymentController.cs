using Microsoft.AspNetCore.Mvc;
using SmartTransportation.BLL.DTOs.Payment;
using SmartTransportation.BLL.Services;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.UnitOfWork;
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

      

        [HttpPost("create-and-confirm")]
        public async Task<ActionResult<CreateStripePaymentResponseDto>> CreateAndConfirm([FromBody] CreateStripePaymentRequestDto dto)
        {
            if (dto == null || dto.BookingId <= 0)
                return BadRequest(new { Message = "BookingId is required." });

            try
            {
                // Step 1: Create PaymentIntent
                var (payment, clientSecret) = await _stripePaymentService.CreatePaymentIntentAsync(dto.BookingId);

                // Step 2: If PaymentMethodId is passed, confirm immediately
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


        // Optional: refresh without confirming
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

        // GET: api/StripePayment
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var payments = await _stripePaymentService.GetAllPaymentsAsync();

                // You can return entities directly, or map to a DTO
                return Ok(payments.Select(p => new
                {
                    p.PaymentId,
                    p.BookingId,
                    p.Amount,
                    p.Currency,
                    p.Status,
                    p.StripePaymentIntentId,
                    p.PaidAt,
                    p.CreatedAt,
                    p.LastError
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error getting payments.", Error = ex.Message });
            }
        }

        // GET: api/StripePayment/{paymentId}
        [HttpGet("{paymentId:int}")]
        public async Task<IActionResult> GetById(int paymentId)
        {
            try
            {
                var payment = await _stripePaymentService.GetPaymentByIdAsync(paymentId);
                if (payment == null)
                    return NotFound(new { Message = "Payment not found." });

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
                // from service if it throws "Payment not found."
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error getting payment.", Error = ex.Message });
            }
        }



        // GET: api/StripePayment/passenger/{passengerId}
        [HttpGet("passenger/{passengerId:int}")]
        public async Task<IActionResult> GetByPassengerId(int passengerId)
        {
            try
            {
                var payments = await _stripePaymentService.GetPaymentsByPassengerIdAsync(passengerId);

                // We'll include PassengerId in the DTO using the Booking join
                // To avoid another DB hit per payment, you can pre-load bookings in service,
                // but for simplicity we'll just leave PassengerId null or do a minimal join.

                var dtoList = payments.Select(p => new PaymentResponseDto
                {
                    PaymentId = p.PaymentId,
                    BookingId = p.BookingId,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    Status = p.Status,
                    PaidAt = p.PaidAt,
                    // We already filtered by passengerId in service, so we know which one it is:
                    PassengerId = passengerId
                }).ToList();

                return Ok(dtoList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error getting payments by passenger.", Error = ex.Message });
            }
        }




    }
}

















//[HttpPost("create")]
//public async Task<ActionResult<CreateStripePaymentResponseDto>> Create([FromBody] CreateStripePaymentRequestDto dto)
//{
//    if (dto == null || dto.BookingId <= 0)
//        return BadRequest(new { Message = "BookingId is required." });

//    try
//    {
//        var (payment, clientSecret) = await _stripePaymentService.CreatePaymentIntentAsync(dto.BookingId);

//        var response = new CreateStripePaymentResponseDto
//        {
//            PaymentId = payment.PaymentId,
//            BookingId = payment.BookingId,
//            Amount = payment.Amount,
//            Currency = payment.Currency,
//            Status = payment.Status,
//            ClientSecret = clientSecret
//        };

//        return Ok(response);
//    }
//    catch (Exception ex)
//    {
//        return StatusCode(500, new { Message = "Error creating Stripe payment.", Error = ex.Message });
//    }
//}















//using Microsoft.AspNetCore.Mvc;
//using SmartTransportation.BLL.DTOs.Payment;
//using SmartTransportation.BLL.Services;
//using System;
//using System.Threading.Tasks;

//namespace SmartTransportation.API.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class StripePaymentController : ControllerBase
//    {
//        private readonly StripePaymentService _stripePaymentService;

//        public StripePaymentController(StripePaymentService stripePaymentService)
//        {
//            _stripePaymentService = stripePaymentService;
//        }

//        /// <summary>
//        /// Egypt-simple flow:
//        /// Create + confirm platform fee payment in one shot using PaymentMethodId.
//        /// This is what your MVC app will call.
//        /// </summary>
//        [HttpPost("create-and-confirm")]
//        public async Task<ActionResult<CreateStripePaymentResponseDto>> CreateAndConfirm([FromBody] CreateStripePaymentRequestDto dto)
//        {
//            if (dto == null || dto.BookingId <= 0)
//                return BadRequest(new { Message = "BookingId is required." });

//            if (string.IsNullOrWhiteSpace(dto.PaymentMethodId))
//                return BadRequest(new { Message = "PaymentMethodId is required." });

//            try
//            {
//                var payment = await _stripePaymentService.CreateAndConfirmPlatformFeeAsync(dto.BookingId, dto.PaymentMethodId);

//                var response = new CreateStripePaymentResponseDto
//                {
//                    PaymentId = payment.PaymentId,
//                    BookingId = payment.BookingId,
//                    Amount = payment.Amount,      // platform fee
//                    Currency = payment.Currency,
//                    Status = payment.Status
//                };

//                return Ok(response);
//            }
//            catch (InvalidOperationException ex)
//            {
//                return BadRequest(new { Message = ex.Message });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { Message = "Error processing Stripe payment.", Error = ex.Message });
//            }
//        }

//        /// <summary>
//        /// Optional: manually refresh status from Stripe for a given PaymentId.
//        /// Your MVC UI can poll this if needed.
//        /// </summary>
//        [HttpGet("{paymentId}/status")]
//        public async Task<IActionResult> GetStatus(int paymentId)
//        {
//            try
//            {
//                var payment = await _stripePaymentService.RefreshStatusFromStripeAsync(paymentId);
//                return Ok(new
//                {
//                    payment.PaymentId,
//                    payment.BookingId,
//                    payment.Amount,
//                    payment.Currency,
//                    payment.Status,
//                    payment.PaidAt
//                });
//            }
//            catch (InvalidOperationException ex)
//            {
//                return NotFound(new { Message = ex.Message });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { Message = "Error getting payment status.", Error = ex.Message });
//            }
//        }
//    }
//}






