using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.UnitOfWork;
using Stripe;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace SmartTransportation.BLL.Services
{
    public class StripePaymentService
    {
        private readonly decimal _pricePerKm;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _currency;
        private const decimal PlatformFeePercent = 0.05m; // 5%

        public StripePaymentService(IUnitOfWork unitOfWork, IConfiguration config)
        {
            _unitOfWork = unitOfWork;

            StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
            _currency = config["Stripe:Currency"] ?? "EGP";
            _pricePerKm = config.GetValue<decimal>("PaymentSettings:PricePerKm");
        }


        public async Task<Payment> GetPaymentByIdAsync(int paymentId)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null)
                throw new InvalidOperationException("Payment not found.");

            return payment;
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            var payments = await _unitOfWork.Payments.GetAllAsync();
            // If your repository returns IQueryable, you may need .ToListAsync()
            return payments;
        }


        // Create PaymentIntent only
        // Create PaymentIntent only
        public async Task<(Payment payment, string clientSecret)> CreatePaymentIntentAsync(int bookingId)
        {
            // 1) Load booking
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
            if (booking == null)
                throw new InvalidOperationException("Booking not found.");

            // 2) Recalculate TotalAmount from booked segments (distance * pricePerKm * seats)
            //    If there are no segments, we just keep whatever TotalAmount is already stored.
            var bookingSegments = await _unitOfWork.BookingSegments
                .FindAsync(bs => bs.BookingId == bookingId);

            if (bookingSegments.Any())
            {
                var segmentIds = bookingSegments
                    .Select(bs => bs.SegmentId)
                    .Distinct()
                    .ToList();

                var segments = await _unitOfWork.RouteSegments
                    .FindAsync(rs => segmentIds.Contains(rs.SegmentId));

                var totalDistanceKm = segments.Sum(s => s.DistanceKm ?? 0m);

                if (totalDistanceKm <= 0)
                    throw new InvalidOperationException("Total distance for booking segments is zero. Cannot calculate fare.");

                if (_pricePerKm <= 0)
                    throw new InvalidOperationException("PricePerKm is not configured correctly in appsettings.");

                var totalAmount = Math.Round(totalDistanceKm * _pricePerKm * booking.SeatsCount, 2);

                // Save recalculated total onto booking
                booking.TotalAmount = totalAmount;
                _unitOfWork.Bookings.Update(booking);
                await _unitOfWork.SaveAsync();
            }

            // 3) Validate TotalAmount
            if (booking.TotalAmount <= 0)
                throw new InvalidOperationException("Booking total amount must be greater than 0.");

            var totalFare = booking.TotalAmount;

            // 4) Calculate 5% platform fee
            var platformFee = Math.Round(totalFare * PlatformFeePercent, 2);
            if (platformFee <= 0)
                throw new InvalidOperationException("Calculated platform fee must be greater than 0.");

            var amountInMinor = (long)(platformFee * 100); // Stripe uses smallest currency unit

            // 5) Create Stripe PaymentIntent
            var service = new PaymentIntentService();
            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInMinor,
                Currency = _currency.ToLower(),
                Metadata = new Dictionary<string, string>
        {
            { "BookingId", booking.BookingId.ToString() },
            { "TotalFare", totalFare.ToString("F2") },
            { "PlatformFee", platformFee.ToString("F2") }
        },
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                    AllowRedirects = "never"
                }
            };

            var intent = await service.CreateAsync(options);

            // 6) Store Payment (for the platform fee only)
            var payment = new Payment
            {
                BookingId = booking.BookingId,
                Amount = platformFee,
                Currency = _currency,
                StripePaymentIntentId = intent.Id,
                Status = PaymentStatus.Pending.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.SaveAsync();

            return (payment, intent.ClientSecret);
        }



        // Confirm payment (success or failed)
        public async Task<Payment> ConfirmPaymentAsync(int paymentId, string paymentMethodId)
        {
            // Get the payment record
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null)
                throw new InvalidOperationException("Payment not found.");

            var service = new PaymentIntentService();
            PaymentIntent intent;

            try
            {
                // Confirm the payment with Stripe
                intent = await service.ConfirmAsync(payment.StripePaymentIntentId, new PaymentIntentConfirmOptions
                {
                    PaymentMethod = paymentMethodId
                });
            }
            catch (StripeException ex)
            {
                // Payment failed
                payment.Status = PaymentStatus.Failed.ToString();
                payment.LastError = ex.Message;
                _unitOfWork.Payments.Update(payment);
                await _unitOfWork.SaveAsync();
                return payment;
            }

            // Map Stripe payment status to local status
            payment.Status = intent.Status switch
            {
                "succeeded" => PaymentStatus.Succeeded.ToString(),
                "requires_payment_method" => PaymentStatus.Failed.ToString(),
                "canceled" => PaymentStatus.Canceled.ToString(),
                _ => PaymentStatus.Pending.ToString()
            };

            if (intent.Status == "succeeded")
            {
                payment.PaidAt = DateTime.UtcNow;

                // Update booking status and payment status
                var booking = await _unitOfWork.Bookings.GetByIdAsync(payment.BookingId);
                if (booking != null)
                {
                    booking.BookingStatus = "Confirmed";
                    booking.PaymentStatus = "Paid";
                    _unitOfWork.Bookings.Update(booking);

                    // Optional: Mark passengers as checked-in
                    var bookingsWithDetails = await _unitOfWork.Bookings.GetBookingsWithDetailsAsync();
                    var fullBooking = bookingsWithDetails.FirstOrDefault(b => b.BookingId == booking.BookingId);

                    if (fullBooking?.BookingPassengers != null)
                    {
                        foreach (var passenger in fullBooking.BookingPassengers)
                        {
                            passenger.CheckInStatus = true;
                            _unitOfWork.BookingPassengers.Update(passenger);
                        }
                    }
                }
            }

            // Save payment and booking updates
            _unitOfWork.Payments.Update(payment);
            await _unitOfWork.SaveAsync();

            return payment;
        }


        // Refresh status from Stripe (no confirm)
        public async Task<Payment> RefreshStatusFromStripeAsync(int paymentId)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null) throw new InvalidOperationException("Payment not found.");

            var service = new PaymentIntentService();

            PaymentIntent intent;
            try
            {
                intent = await service.GetAsync(payment.StripePaymentIntentId);
            }
            catch (StripeException ex)
            {
                payment.LastError = ex.Message;
                _unitOfWork.Payments.Update(payment);
                await _unitOfWork.SaveAsync();
                throw;
            }

            // Map Stripe status to your PaymentStatus
            payment.Status = intent.Status switch
            {
                "succeeded" => PaymentStatus.Succeeded.ToString(),
                "requires_payment_method" => PaymentStatus.Failed.ToString(),
                "canceled" => PaymentStatus.Canceled.ToString(),
                _ => PaymentStatus.Pending.ToString()
            };

            if (intent.Status == "succeeded")
                payment.PaidAt = DateTime.UtcNow;

            _unitOfWork.Payments.Update(payment);
            await _unitOfWork.SaveAsync();

            return payment;
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByPassengerIdAsync(int passengerId)
        {
            // 1) Get bookings for that passenger
            var bookingsForPassenger = await _unitOfWork.Bookings
                .FindAsync(b => b.BookerUserId == passengerId);

            var bookingIds = bookingsForPassenger
                .Select(b => b.BookingId)
                .ToList();

            if (!bookingIds.Any())
                return Enumerable.Empty<Payment>();

            // 2) Get payments whose BookingId is in that list
            var payments = await _unitOfWork.Payments
                .FindAsync(p => bookingIds.Contains(p.BookingId));

            return payments;
        }
    }
}


//using Microsoft.Extensions.Configuration;
//using SmartTransportation.DAL.Models;
//using SmartTransportation.DAL.Repositories.UnitOfWork;
//using Stripe;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace SmartTransportation.BLL.Services
//{
//    /// <summary>
//    /// Handles Stripe payments for platform fees only.
//    /// Drivers are paid in cash; Stripe is used only for the platform fee.
//    /// </summary>
//    public class StripePaymentService
//    {
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly string _currency;
//        private const decimal PlatformFeePercent = 0.05m; // 5%

//        // ⚠️ Adjust these to match your actual Booking.PaymentStatus values in DB.
//        private const string BookingPaymentStatusPaid = "Paid";
//        private const string BookingPaymentStatusPending = "Pending";

//        public StripePaymentService(IUnitOfWork unitOfWork, IConfiguration config)
//        {
//            _unitOfWork = unitOfWork;

//            StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
//            _currency = config["Stripe:Currency"] ?? "EGP";
//        }

//        /// <summary>
//        /// Egypt-style flow:
//        /// - Compute platform fee from Booking.TotalAmount
//        /// - Create + confirm Stripe PaymentIntent in one step (Confirm = true)
//        /// - Store Payment record
//        /// - Update Booking.PaymentStatus when succeeded
//        /// </summary>
//        public async Task<Payment> CreateAndConfirmPlatformFeeAsync(int bookingId, string paymentMethodId)
//        {
//            if (string.IsNullOrWhiteSpace(paymentMethodId))
//                throw new ArgumentException("PaymentMethodId is required.", nameof(paymentMethodId));

//            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
//            if (booking == null)
//                throw new InvalidOperationException("Booking not found.");

//            if (booking.TotalAmount <= 0)
//                throw new InvalidOperationException("Booking total amount must be greater than 0.");

//            // 🔒 Prevent double-charging platform fee using existing booking.PaymentStatus
//            if (string.Equals(booking.PaymentStatus, BookingPaymentStatusPaid, StringComparison.OrdinalIgnoreCase))
//                throw new InvalidOperationException("Platform fee has already been paid for this booking.");

//            var totalFare = booking.TotalAmount;
//            var platformFee = Math.Round(totalFare * PlatformFeePercent, 2);

//            if (platformFee <= 0)
//                throw new InvalidOperationException("Calculated platform fee must be greater than 0.");

//            var amountInMinor = (long)(platformFee * 100); // Stripe uses smallest currency unit

//            var paymentIntentService = new PaymentIntentService();

//            var options = new PaymentIntentCreateOptions
//            {
//                Amount = amountInMinor,
//                Currency = _currency.ToLower(),
//                PaymentMethod = paymentMethodId,    // backend-confirm style
//                Confirm = true,                     // confirm in same call
//                Metadata = new Dictionary<string, string>
//                {
//                    { "BookingId", booking.BookingId.ToString() },
//                    { "TotalFare", totalFare.ToString("F2") },
//                    { "PlatformFee", platformFee.ToString("F2") }
//                },
//                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
//                {
//                    Enabled = true,
//                    AllowRedirects = "never"
//                }
//            };

//            PaymentIntent intent;
//            try
//            {
//                intent = await paymentIntentService.CreateAsync(options);
//            }
//            catch (StripeException ex)
//            {
//                // Stripe failed at creation/confirm stage — still store a failed Payment for auditing.
//                var failedPayment = new Payment
//                {
//                    BookingId = booking.BookingId,
//                    Amount = platformFee,
//                    Currency = _currency,
//                    Status = PaymentStatus.Failed.ToString(),
//                    LastError = ex.Message,
//                    CreatedAt = DateTime.UtcNow
//                };

//                await _unitOfWork.Payments.AddAsync(failedPayment);
//                await _unitOfWork.SaveAsync();

//                return failedPayment;
//            }

//            var status = intent.Status switch
//            {
//                "succeeded" => PaymentStatus.Succeeded.ToString(),
//                "requires_payment_method" => PaymentStatus.Failed.ToString(),
//                "canceled" => PaymentStatus.Canceled.ToString(),
//                _ => PaymentStatus.Pending.ToString()
//            };

//            var payment = new Payment
//            {
//                BookingId = booking.BookingId,
//                Amount = platformFee,               // PLATFORM FEE ONLY
//                Currency = _currency,
//                StripePaymentIntentId = intent.Id,
//                Status = status,
//                CreatedAt = DateTime.UtcNow
//            };

//            if (intent.Status == "succeeded")
//            {
//                payment.PaidAt = DateTime.UtcNow;

//                // 🟢 Use your existing Booking.PaymentStatus string
//                booking.PaymentStatus = BookingPaymentStatusPaid;
//                _unitOfWork.Bookings.Update(booking);
//            }

//            await _unitOfWork.Payments.AddAsync(payment);
//            await _unitOfWork.SaveAsync();

//            return payment;
//        }

//        /// <summary>
//        /// Refreshes the local Payment status from Stripe without confirming it.
//        /// Used by background job or manual polling.
//        /// Also keeps Booking.PaymentStatus in sync.
//        /// </summary>
//        public async Task<Payment> RefreshStatusFromStripeAsync(int paymentId)
//        {
//            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
//            if (payment == null) throw new InvalidOperationException("Payment not found.");

//            var service = new PaymentIntentService();

//            PaymentIntent intent;
//            try
//            {
//                intent = await service.GetAsync(payment.StripePaymentIntentId);
//            }
//            catch (StripeException ex)
//            {
//                payment.LastError = ex.Message;
//                _unitOfWork.Payments.Update(payment);
//                await _unitOfWork.SaveAsync();
//                throw;
//            }

//            payment.Status = intent.Status switch
//            {
//                "succeeded" => PaymentStatus.Succeeded.ToString(),
//                "requires_payment_method" => PaymentStatus.Failed.ToString(),
//                "canceled" => PaymentStatus.Canceled.ToString(),
//                _ => PaymentStatus.Pending.ToString()
//            };

//            if (intent.Status == "succeeded" && payment.PaidAt == null)
//            {
//                payment.PaidAt = DateTime.UtcNow;

//                // Booking.PaymentStatus sync
//                var booking = await _unitOfWork.Bookings.GetByIdAsync(payment.BookingId);
//                if (booking != null &&
//                    !string.Equals(booking.PaymentStatus, BookingPaymentStatusPaid, StringComparison.OrdinalIgnoreCase))
//                {
//                    booking.PaymentStatus = BookingPaymentStatusPaid;
//                    _unitOfWork.Bookings.Update(booking);
//                }
//            }

//            _unitOfWork.Payments.Update(payment);
//            await _unitOfWork.SaveAsync();

//            return payment;
//        }
//    }
//}

