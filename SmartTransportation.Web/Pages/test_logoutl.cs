using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;

namespace SmartTransportation.Pages
{
    public class TestLogoutModel : PageModel
    {
        private readonly ILogger<TestLogoutModel> _logger;

        public TestLogoutModel(ILogger<TestLogoutModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // Just display the page
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var debug = new StringBuilder();
            debug.AppendLine("=== LOGOUT DEBUG INFO ===");
            debug.AppendLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            debug.AppendLine($"User before logout: {User.Identity?.Name ?? "None"}");
            debug.AppendLine($"IsAuthenticated before: {User.Identity?.IsAuthenticated}");
            debug.AppendLine($"Claims count: {User.Claims.Count()}");

            try
            {
                // Log out
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                debug.AppendLine("✅ SignOutAsync completed");

                // Clear session
                HttpContext.Session.Clear();
                debug.AppendLine("✅ Session cleared");

                // Delete cookies
                var deletedCookies = new List<string>();
                foreach (var cookie in Request.Cookies.Keys)
                {
                    if (cookie.Contains("AspNetCore") || cookie.Contains("Auth"))
                    {
                        Response.Cookies.Delete(cookie);
                        deletedCookies.Add(cookie);
                    }
                }
                debug.AppendLine($"✅ Deleted cookies: {string.Join(", ", deletedCookies)}");

                // Set headers
                Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                Response.Headers["Pragma"] = "no-cache";
                Response.Headers["Expires"] = "-1";
                debug.AppendLine("✅ Cache headers set");

                debug.AppendLine("=== LOGOUT COMPLETED ===");

                _logger.LogInformation(debug.ToString());
                TempData["LogoutDebug"] = debug.ToString();

                // Redirect back to same page to see result
                return RedirectToPage("/TestLogout");
            }
            catch (Exception ex)
            {
                debug.AppendLine($"❌ ERROR: {ex.Message}");
                debug.AppendLine($"Stack: {ex.StackTrace}");
                _logger.LogError(ex, "Logout failed");
                TempData["LogoutDebug"] = debug.ToString();
                return RedirectToPage("/TestLogout");
            }
        }
    }
}