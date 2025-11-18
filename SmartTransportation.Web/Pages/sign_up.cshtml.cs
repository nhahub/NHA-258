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
        public string UserName { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

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
        //Function to check if username already exists
        public async Task<JsonResult> OnGetCheckUsername(string userName)
        {
            var apiBase = _configuration["ApiBaseUrl"];
            using var client = new HttpClient();
            var response = await client.GetAsync($"{apiBase}/api/auth/check-username?username={userName}");

            if (!response.IsSuccessStatusCode)
                return new JsonResult("Error contacting server");

            var isAvailable = bool.Parse(await response.Content.ReadAsStringAsync());
            return isAvailable ? new JsonResult(true) : new JsonResult("Username already exists");
        }
        //Function to check if email already exists
        public async Task<JsonResult> OnGetCheckEmail(string email)
        {
            var apiBase = _configuration["ApiBaseUrl"];
            using var client = new HttpClient();
            var response = await client.GetAsync($"{apiBase}/api/auth/check-email?email={email}");

            if (!response.IsSuccessStatusCode)
                return new JsonResult("Error contacting server");

            var isAvailable = bool.Parse(await response.Content.ReadAsStringAsync());
            return isAvailable ? new JsonResult(true) : new JsonResult("Email already exists");
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

            var apiBaseUrl = _configuration["ApiBaseUrl"];
            using var client = new HttpClient();

            try
            {
                var response = await client.PostAsJsonAsync($"{apiBaseUrl}/api/auth/register", registerDto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

                    if (!string.IsNullOrEmpty(result?.Token))
                    {
                        var cookieExpiration = DateTime.UtcNow.AddHours(1);

                        Response.Cookies.Append("AuthToken", result.Token, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = cookieExpiration
                        });

                        Response.Cookies.Append("UserTypeId", result.UserTypeId.ToString(), new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = cookieExpiration
                        });

                        Response.Cookies.Append("UserName", result.UserName, new CookieOptions
                        {
                            HttpOnly = false,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = cookieExpiration
                        });

                        return RedirectToPage(GetRedirectPageByUserType(result.UserTypeId));
                    }

                    ErrorMessage = "Registration failed. Please try again.";
                    return Page();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                ErrorMessage = $"Registration failed: {errorContent}";
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Registration failed: {ex.Message}";
                return Page();
            }
        }

        private string GetRedirectPageByUserType(int userTypeId)
        {
            return userTypeId switch
            {
                1 => "/AdminDashboard",
                2 => "/Driver_Profile",
                3 => "/customer-profile",
                _ => "/Index"
            };
        }
    }
}
