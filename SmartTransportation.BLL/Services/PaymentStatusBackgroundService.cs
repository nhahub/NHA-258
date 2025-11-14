using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using SmartTransportation.BLL.Services;
using SmartTransportation.DAL;
using SmartTransportation.DAL.Models;

namespace SmartTransportation.BLL.Jobs
{
    /// <summary>
    /// Periodically checks pending payments and syncs their status from Stripe.
    /// </summary>
    public class PaymentStatusBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PaymentStatusBackgroundService> _logger;

        // how often to run (you can adjust this)
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        public PaymentStatusBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<PaymentStatusBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PaymentStatusBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingPaymentsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while processing pending payments.");
                }

                try
                {
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // ignore if shutting down
                }
            }

            _logger.LogInformation("PaymentStatusBackgroundService is stopping.");
        }

        private async Task ProcessPendingPaymentsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<TransportationContext>();
            var stripeService = scope.ServiceProvider.GetRequiredService<StripePaymentService>();

            // optional: small grace period so we don't check immediately after creation
            var gracePeriod = DateTime.UtcNow.AddMinutes(-1);

            var pendingPayments = await context.Payments
                .Where(p =>
                    p.Status == PaymentStatus.Pending.ToString() &&
                    p.CreatedAt <= gracePeriod)
                .ToListAsync(cancellationToken);

            if (!pendingPayments.Any())
            {
                _logger.LogDebug("No pending payments to process.");
                return;
            }

            _logger.LogInformation("Processing {Count} pending payment(s).", pendingPayments.Count);

            foreach (var payment in pendingPayments)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    _logger.LogInformation("Refreshing payment {PaymentId} (StripeIntentId={StripeId})",
                        payment.PaymentId,
                        payment.StripePaymentIntentId);

                    await stripeService.RefreshStatusFromStripeAsync(payment.PaymentId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error refreshing payment {PaymentId} from Stripe.",
                        payment.PaymentId);
                }
            }
        }
    }
}
