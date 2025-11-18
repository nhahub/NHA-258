using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartTransportation.BLL.DTOs.Auth;
using System.ComponentModel.DataAnnotations;

public class Log_InModel : PageModel
{
    private readonly IConfiguration _configuration;

    public Log_InModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [BindProperty]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public bool RememberMe { get; set; }

    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var loginDto = new { Email, Password };

        // Get API base URL from configuration
        var apiBaseUrl = _configuration["ApiBaseUrl"];

        using var client = new HttpClient();

        try
        {
            // Call the login API
            var response = await client.PostAsJsonAsync($"{apiBaseUrl}/api/auth/login", loginDto);

            if (response.IsSuccessStatusCode)
            {
                // Deserialize directly to AuthResponseDto
                var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

                if (!string.IsNullOrEmpty(result?.Token))
                {
                    // Determine cookie expiration based on RememberMe
                    var cookieExpiration = RememberMe
                        ? DateTime.UtcNow.AddDays(30)
                        : DateTime.UtcNow.AddHours(1);

                    // Set JWT cookie
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

                    // Redirect based on user type
                    return RedirectToPage(GetRedirectPageByUserType(result.UserTypeId));
                }
                else
                {
                    ErrorMessage = "Login failed. Token not received.";
                    return Page();
                }
            }
            else
            {
                ErrorMessage = "Invalid email or password.";
                return Page();
            }
        }
        catch (Exception ex)
        {
            // Optional: log exception
            ErrorMessage = $"Login failed: {ex.Message}";
            return Page();
        }
    }

    /// <summary>
    /// Determines the redirect page based on user type
    /// UserTypeId: 1 = Passenger, 2 = Driver, 3 = Admin
    /// </summary>
    private string GetRedirectPageByUserType(int userTypeId)
    {
        return userTypeId switch
        {
            1 => "/AdminDashboard",  // Passenger
            2 => "/Driver_Profile",    // Driver
            3 => "/customer-profile",    // Admin
            _ => "/Index"              // Default fallback
        };
    }
}
