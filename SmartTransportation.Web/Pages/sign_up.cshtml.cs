using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartTransportation.BLL.DTOs.Auth;
using System.ComponentModel.DataAnnotations;

namespace SmartTransportation.Web.Pages
{
    public class Sign_UpModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public Sign_UpModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string UserName { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

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
        [Required(ErrorMessage = "Please select a user type")]
        public int UserTypeId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "You must accept the terms and conditions")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms and conditions")]
        public bool AcceptTerms { get; set; }

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            // Initialize sign up page
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var registerDto = new RegisterRequestDto
            {
                UserName = UserName,
                Email = Email,
                Password = Password,
                UserTypeId = UserTypeId
            };

            // Get API base URL from configuration
            var apiBaseUrl = _configuration["ApiBaseUrl"];

            using var client = new HttpClient();

            try
            {
                // Call the register API
                var response = await client.PostAsJsonAsync($"{apiBaseUrl}/api/auth/register", registerDto);

                if (response.IsSuccessStatusCode)
                {
                    // Deserialize the response
                    var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

                    if (!string.IsNullOrEmpty(result?.Token))
                    {
                        // Set JWT cookie
                        Response.Cookies.Append("AuthToken", result.Token, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddHours(1)
                        });

                        // Redirect to login page or dashboard
                        return RedirectToPage("/Log_in");
                    }
                    else
                    {
                        ErrorMessage = "Registration failed. Please try again.";
                        return Page();
                    }
                }
                else
                {
                    // Read error message from API response
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Registration failed: {errorContent}";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Registration failed: {ex.Message}";
                return Page();
            }
        }
    }
}
