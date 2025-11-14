using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTransportation.BLL.DTOs.Auth
{
    public class GoogleLoginRequestDto
    {
        public required string IdToken { get; set; }
    }
}
