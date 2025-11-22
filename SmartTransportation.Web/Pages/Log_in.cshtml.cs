using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartTransportation.BLL.DTOs.Auth;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

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
        var apiBaseUrl = _configuration["ApiBaseUrl"];

        using var client = new HttpClient();

        try
        {
            var response = await client.PostAsJsonAsync($"{apiBaseUrl}/api/auth/login", loginDto);

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Invalid email or password.";
                return Page();
            }

            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            if (result == null || string.IsNullOrEmpty(result.Token))
            {
                ErrorMessage = "Login failed. Token not received.";
                return Page();
            }

            // ---------------------------
            // Set JWT cookie for API calls
            // ---------------------------
            var cookieExpiration = RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddHours(1);
            Response.Cookies.Append("AuthToken", result.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = cookieExpiration
            });

            // ---------------------------
            // Create claims and sign in
            // ---------------------------
            var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, result.UserName ?? ""),
    new Claim("UserTypeId", result.UserTypeId.ToString()), // store user type as claim
    new Claim(ClaimTypes.Role, result.UserTypeId switch
    {
        1 => "Admin",
        2 => "Driver",
        3 => "Passenger",
        _ => "Guest"
    })
};

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = RememberMe,
                    ExpiresUtc = cookieExpiration
                });

            // ---------------------------
            // Redirect based on UserType
            // ---------------------------
            return RedirectToPage(result.UserTypeId switch
            {
                2 => "/Driver_Profile",     // Driver
                3 => "/customer-profile",   //
                1 => "/AdminDashboard",     // Passenger
                _ => "/Index"
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login failed: {ex.Message}";
            return Page();
        }
    }
}
