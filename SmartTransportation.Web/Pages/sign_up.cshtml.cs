using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace SmartTransportation.Web.Pages
{
    public class Sign_UpModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Please confirm your password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [BindProperty]
        public string? VehicleType { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "You must accept the terms and conditions")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms and conditions")]
        public bool AcceptTerms { get; set; }

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            // Initialize sign up page
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // TODO: Implement user registration logic
            // Redirect to dashboard or profile page
            return RedirectToPage("/Driver_Profile");
        }
    }
}