using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace SmartTransportation.Web.Pages
{
    public class Booking
    {
        public string ServiceType { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public string BookingTime { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class Customer_ProfileModel : PageModel
    {
        [BindProperty]
        [Required]
        public string FirstName { get; set; } = "Jane";

        [BindProperty]
        [Required]
        public string LastName { get; set; } = "Smith";

        [BindProperty]
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "jane.smith@example.com";

        [BindProperty]
        [Required]
        [Phone]
        public string Phone { get; set; } = "+1 (555) 987-6543";

        [BindProperty]
        public string Address { get; set; } = "123 Main Street, New York, NY 10001";

        [BindProperty]
        public string CardNumber { get; set; } = string.Empty;

        [BindProperty]
        public string ExpiryDate { get; set; } = string.Empty;

        [BindProperty]
        public string CVV { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}";
        public string CustomerId { get; set; } = "CUST-12345";

        public List<Booking> Bookings { get; set; } = new();

        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
            // TODO: Load customer data from database
            // Sample bookings
            Bookings = new List<Booking>
            {
                new Booking
                {
                    ServiceType = "Vehicle Ride - Airport Transfer",
                    BookingDate = DateTime.Now.AddDays(2),
                    BookingTime = "10:00 AM",
                    Status = "Confirmed"
                },
                new Booking
                {
                    ServiceType = "Vehicle Maintenance",
                    BookingDate = DateTime.Now.AddDays(7),
                    BookingTime = "2:30 PM",
                    Status = "Pending"
                }
            };
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // TODO: Update customer profile in database
            SuccessMessage = "Profile updated successfully!";
            OnGet(); // Reload bookings
            return Page();
        }

        public IActionResult OnPostUpdatePayment()
        {
            // TODO: Update payment information in database
            SuccessMessage = "Payment information updated successfully!";
            OnGet(); // Reload bookings
            return Page();
        }
    }
}