using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
public class BaseApiController : ControllerBase
{
    protected int? CurrentUserId
    {
        get
        {
            var claim = User.FindFirst("UserId") ?? User.FindFirst("UserID");
            return claim != null ? int.Parse(claim.Value) : (int?)null;
        }
    }
}
