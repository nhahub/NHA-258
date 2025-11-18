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
                        // Set JWT cookie (1 hour expiration for new registrations)
                        var cookieExpiration = DateTime.UtcNow.AddHours(1);

                        Response.Cookies.Append("AuthToken", result.Token, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = cookieExpiration
                        });

                        // Store UserTypeId in cookie for role-based access control
                        Response.Cookies.Append("UserTypeId", result.UserTypeId.ToString(), new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = cookieExpiration
                        });

                        // Store UserName for display purposes
                        Response.Cookies.Append("UserName", result.UserName, new CookieOptions
                        {
                            HttpOnly = false, // Allow JavaScript access for display
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = cookieExpiration
                        });

                        // Redirect based on user type to their respective profile
                        return RedirectToPage(GetRedirectPageByUserType(result.UserTypeId));
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

        /// <summary>
        /// Determines the redirect page based on user type
        /// UserTypeId: 1 = Admin, 2 = Driver, 3 = Passenger
        /// </summary>
        private string GetRedirectPageByUserType(int userTypeId)
        {
            return userTypeId switch
            {
                1 => "/AdminDashboard", // Admin
                2 => "/Driver_Profile",    // Driver
                3 => "/customer-profile",  // Passenger
                _ => "/Index"              // Default fallback
            };
        }
    }
}
