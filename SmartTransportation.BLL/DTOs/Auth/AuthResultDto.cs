using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;

namespace SmartTransportation.BLL.DTOs.Auth
{
    public class AuthResultDto
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public required AuthResponseDto Data { get; set; }
    }
}
