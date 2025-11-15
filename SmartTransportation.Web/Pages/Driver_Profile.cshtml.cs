using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace SmartTransportation.Web.Pages
{
    public class Driver_ProfileModel : PageModel
    {
        [BindProperty]
        [Required]
        public string FirstName { get; set; } = "John";

        [BindProperty]
        [Required]
        public string LastName { get; set; } = "Doe";

        [BindProperty]
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "john.doe@example.com";

        [BindProperty]
        [Required]
        [Phone]
        public string Phone { get; set; } = "+1 (555) 123-4567";

        [BindProperty]
        public string VehicleMake { get; set; } = "Tesla";

        [BindProperty]
        public string VehicleModel { get; set; } = "Model 3";

        [BindProperty]
        public int VehicleYear { get; set; } = 2023;

        [BindProperty]
        public string LicensePlate { get; set; } = "ABC-1234";

        public string FullName => $"{FirstName} {LastName}";
        public int TotalTrips { get; set; } = 245;
        public int TotalMiles { get; set; } = 12580;
        public int SafetyScore { get; set; } = 95;

        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
            // TODO: Load user data from database
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // TODO: Update user profile in database
            SuccessMessage = "Profile updated successfully!";
            return Page();
        }

        public IActionResult OnPostUpdateVehicle()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // TODO: Update vehicle information in database
            SuccessMessage = "Vehicle information updated successfully!";
            return Page();
        }
    }
}