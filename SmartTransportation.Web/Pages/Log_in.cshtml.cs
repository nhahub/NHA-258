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
                    // Set JWT cookie
                    Response.Cookies.Append("AuthToken", result.Token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddHours(1)
                    });

                    return RedirectToPage("/Index"); //// Just 
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

}
