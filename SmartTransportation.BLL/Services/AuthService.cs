using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartTransportation.BLL.DTOs.Auth;
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.DAL.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Google.Apis.Auth;
using BC = BCrypt.Net.BCrypt;

namespace SmartTransportation.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly TransportationContext _context;
        private readonly IConfiguration _config;

        public AuthService(TransportationContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<AuthResultDto> RegisterAsync(RegisterRequestDto model)
        {
            var emptyResponse = new AuthResponseDto 
            { 
                Token = string.Empty,
                UserName = string.Empty,
                Email = string.Empty
            };

            if (await _context.Users.AnyAsync(u => u.UserName == model.UserName))
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Username already exists.",
                    Data = emptyResponse
                };
            }

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Email already exists.",
                    Data = emptyResponse
                };
            }

            var user = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                PasswordHash = BC.HashPassword(model.Password),
                UserTypeId = model.UserTypeId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            return new AuthResultDto
            {
                Success = true,
                Message = "Registration successful.",
                Data = token
            };
        }

        public async Task<AuthResultDto> LoginAsync(LoginRequestDto model)
        {
            var emptyResponse = new AuthResponseDto 
            { 
                Token = string.Empty,
                UserName = string.Empty,
                Email = string.Empty
            };

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                return new AuthResultDto 
                { 
                    Success = false, 
                    Message = "Email not found.",
                    Data = emptyResponse
                };
            }

            if (!BC.Verify(model.Password, user.PasswordHash))
            {
                return new AuthResultDto 
                { 
                    Success = false, 
                    Message = "Incorrect password.",
                    Data = emptyResponse
                };
            }

            if (!user.IsActive)
            {
                return new AuthResultDto 
                { 
                    Success = false, 
                    Message = "User account is not active.",
                    Data = emptyResponse
                };
            }

            var token = GenerateJwtToken(user);
            return new AuthResultDto
            {
                Success = true,
                Message = "Login successful.",
                Data = token
            };
        }

        public async Task<AuthResultDto> GoogleLoginAsync(string idToken)
        {
            var emptyResponse = new AuthResponseDto 
            { 
                Token = string.Empty,
                UserName = string.Empty,
                Email = string.Empty
            };

            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

                if (user == null)
                {
                    user = new User
                    {
                        UserName = payload.Name.Replace(" ", ""),
                        Email = payload.Email,
                        PasswordHash = BC.HashPassword(Guid.NewGuid().ToString()), 
                        UserTypeId = 3, // Passenger role
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                var token = GenerateJwtToken(user);
                return new AuthResultDto
                {
                    Success = true,
                    Message = "Google login successful",
                    Data = token
                };
            }
            catch
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Invalid Google token",
                    Data = emptyResponse
                };
            }
        }

        // Generate JWT token (shared)
        private AuthResponseDto GenerateJwtToken(User user)
        {
            var jwt = _config.GetSection("Jwt");
            var secret = jwt["Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("UserID", user.UserId.ToString()),
                new Claim("UserType", user.UserTypeId.ToString())
            };

            var expireMinutes = jwt["ExpireMinutes"] ?? "60";
            if (!double.TryParse(expireMinutes, out var expirationMinutes))
            {
                expirationMinutes = 60;
            }

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: creds
            );

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                UserName = user.UserName,
                Email = user.Email
            };
        }
    }
}
