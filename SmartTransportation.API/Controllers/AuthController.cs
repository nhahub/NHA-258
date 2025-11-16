using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTransportation.BLL.DTOs.Auth;
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.DAL.Models;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{

    //Teste push 

    //Second teste push
    private readonly IAuthService _authService;
    private readonly TransportationContext _context;

    public AuthController(IAuthService authService, TransportationContext context)
    {
        _authService = authService;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState); 

        var result = await _authService.RegisterAsync(dto);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result.Data);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.LoginAsync(dto);

        if (!result.Success)
            return Unauthorized(new { message = result.Message });

        return Ok(result.Data);
    }



    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto dto)
    {
        var result = await _authService.GoogleLoginAsync(dto.IdToken);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result.Data);
    }

    // Remote validation: check if username is available
    [HttpGet("check-username")]
    public async Task<IActionResult> CheckUsername(string username)
    {
        bool exists = await _context.Users.AnyAsync(u => u.UserName == username);
        return Ok(!exists); // true = available
    }

    // Remote validation: check if email is available
    [HttpGet("check-email")]
    public async Task<IActionResult> CheckEmail(string email)
    {
        bool exists = await _context.Users.AnyAsync(u => u.Email == email);
        return Ok(!exists); // true = available
    }
}
