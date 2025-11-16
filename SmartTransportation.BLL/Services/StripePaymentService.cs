using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.UnitOfWork;
using Stripe;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.BLL.Services
{
    public class StripePaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _currency;
        private const decimal PlatformFeePercent = 0.05m; // 5%

        public StripePaymentService(IUnitOfWork unitOfWork, IConfiguration config)
        {
            _unitOfWork = unitOfWork;

            StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
            _currency = config["Stripe:Currency"] ?? "EGP";
        }

        // Create PaymentIntent only
        public async Task<(Payment payment, string clientSecret)> CreatePaymentIntentAsync(int bookingId)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
            if (booking == null)
                throw new InvalidOperationException("Booking not found.");

            if (booking.TotalAmount <= 0)
                throw new InvalidOperationException("Booking total amount must be greater than 0.");

            var totalFare = booking.TotalAmount;
            var platformFee = Math.Round(totalFare * PlatformFeePercent, 2);

            if (platformFee <= 0)
                throw new InvalidOperationException("Calculated platform fee must be greater than 0.");

            var amountInMinor = (long)(platformFee * 100); // Stripe uses cents

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
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null) throw new InvalidOperationException("Payment not found.");

            var service = new PaymentIntentService();
            PaymentIntent intent;

            try
            {
                intent = await service.ConfirmAsync(payment.StripePaymentIntentId, new PaymentIntentConfirmOptions
                {
                    PaymentMethod = paymentMethodId
                });
            }
            catch (StripeException ex)
            {
                payment.Status = PaymentStatus.Failed.ToString();
                payment.LastError = ex.Message;
                _unitOfWork.Payments.Update(payment);
                await _unitOfWork.SaveAsync();
                return payment;
            }

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
    }
}
