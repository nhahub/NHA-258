using System.Security.Claims;

namespace SmartTransportation.Web.Helpers
{
    public static class ClaimsHelper
    {
        public static int? GetUserId(ClaimsPrincipal user)
        {
            if (user == null) return null;

            var claim = user.FindFirst("UserId")
                        ?? user.FindFirst("UserID")
                        ?? user.FindFirst(ClaimTypes.NameIdentifier)
                        ?? user.FindFirst("sub");

            if (claim != null && int.TryParse(claim.Value, out var id))
                return id;

            return null;
        }
    }
}
