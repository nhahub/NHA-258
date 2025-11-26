using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SmartTransportation.BLL.Interfaces;

namespace SmartTransportation.Web.Pages
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardModel : PageModel
    {
        private readonly ILogger<AdminDashboardModel> _logger;

        public AdminDashboardModel(ILogger<AdminDashboardModel> logger)
        {
            _logger = logger;
        }

        // Basic statistics (these will be updated by JavaScript)
        public int TotalTrips { get; set; } = 0;
        public int ActiveTrips { get; set; } = 0;
        public decimal TotalRevenue { get; set; } = 0m;
        public int TotalBookings { get; set; } = 0;
        public int CompletedTrips { get; set; } = 0;
        public double AverageRating { get; set; } = 0.0;

        public void OnGet()
        {
            _logger.LogInformation("Admin dashboard page loaded. Data will be fetched via API calls.");

            // The page will load and JavaScript will fetch the real data from the API endpoints
            // These are just placeholder values for initial page render
        }
    }
}