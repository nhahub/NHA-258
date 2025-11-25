using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SmartTransportation.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(ILogger<LogoutModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // If someone tries to access via GET, log them out anyway
            _logger.LogWarning("Logout accessed via GET - redirecting to POST");
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("=== LOGOUT STARTED ===");
                _logger.LogInformation("User before logout: {User}", User.Identity?.Name ?? "Unknown");
                _logger.LogInformation("IsAuthenticated before: {IsAuth}", User.Identity?.IsAuthenticated);

                // Step 1: Sign out using Cookie Authentication
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                _logger.LogInformation("SignOutAsync completed");

                // Step 2: Clear session
                if (HttpContext.Session != null)
                {
                    HttpContext.Session.Clear();
                    _logger.LogInformation("Session cleared");
                }

                // Step 3: Delete all authentication cookies manually
                var cookiesToDelete = new List<string>();
                foreach (var cookie in Request.Cookies.Keys)
                {
                    if (cookie.Contains("AspNetCore", StringComparison.OrdinalIgnoreCase) ||
                        cookie.Contains("Auth", StringComparison.OrdinalIgnoreCase) ||
                        cookie.Contains("Identity", StringComparison.OrdinalIgnoreCase))
                    {
                        Response.Cookies.Delete(cookie);
                        cookiesToDelete.Add(cookie);
                    }
                }
                _logger.LogInformation("Deleted cookies: {Cookies}", string.Join(", ", cookiesToDelete));

                // Step 4: Set aggressive cache control headers
                Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, max-age=0";
                Response.Headers["Pragma"] = "no-cache";
                Response.Headers["Expires"] = "-1";
                Response.Headers["Clear-Site-Data"] = "\"cache\", \"cookies\", \"storage\"";

                _logger.LogInformation("=== LOGOUT COMPLETED ===");

                // Step 5: Set success message
                TempData["InfoMessage"] = "You have been successfully logged out.";

                // Step 6: Redirect with cache buster
                var redirectUrl = $"/Index?logout={DateTime.Now.Ticks}";
                _logger.LogInformation("Redirecting to: {Url}", redirectUrl);

                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR during logout");
                TempData["InfoMessage"] = "Logout completed with errors.";
                return Redirect($"/Index?logout={DateTime.Now.Ticks}");
            }
        }
    }
}