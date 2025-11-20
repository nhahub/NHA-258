using SmartTransportation.BLL.DTOs.Profile;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.BLL.Interfaces
{
    public interface IUserProfileService
    {
        Task<BaseUserProfileDTO> GetByUserIdAsync(int userId);
        Task<IEnumerable<BaseUserProfileDTO>> GetAllAsync();
        Task<BaseUserProfileDTO> CreateAsync(CreateUserProfileDTO dto);
        Task<BaseUserProfileDTO> UpdateAsync(int userId, UpdateUserProfileDTO dto);
    }
}
