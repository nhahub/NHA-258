using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using SmartTransportation.BLL.DTOs.Auth;

namespace SmartTransportation.BLL.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResultDto> RegisterAsync(RegisterRequestDto model);
        Task<AuthResultDto> LoginAsync(LoginRequestDto model);
        Task<AuthResultDto> GoogleLoginAsync(string idToken);
    }
}
