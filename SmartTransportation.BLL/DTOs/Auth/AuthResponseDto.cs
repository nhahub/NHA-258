using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTransportation.BLL.DTOs.Auth
{
    public class AuthResponseDto
    {
        public required string Token { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }

        public static AuthResponseDto Empty => new()
        {
            Token = string.Empty,
            UserName = string.Empty,
            Email = string.Empty
        };
    }
}
