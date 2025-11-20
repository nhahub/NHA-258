using SmartTransportation.BLL.DTOs.Profile;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.BLL.Interfaces
{
    public interface IAdminService
    {
        // Driver
        Task<bool> VerifyDriverAsync(int driverId, bool isVerified);
        Task<IEnumerable<DriverProfileDTO>> GetAllDriversAsync(bool? onlyVerified = null);

        // Vehicle
        Task<bool> VerifyVehicleAsync(int vehicleId, int driverId, bool isVerified);
        Task<IEnumerable<VehicleDTO>> GetAllVehiclesAsync(bool? onlyVerified = null);
        Task<IEnumerable<VehicleDTO>> GetVehiclesByDriverAsync(int driverId);  // <-- added

        // Admin profile
        Task<BaseUserProfileDTO> CreateAdminProfileAsync(CreateUserProfileDTO dto, int adminId);
        Task<BaseUserProfileDTO> GetAdminProfileAsync(int adminId);
        Task<BaseUserProfileDTO> UpdateAdminProfileAsync(int adminId, UpdateUserProfileDTO dto);
    }
}
