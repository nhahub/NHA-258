using SmartTransportation.BLL.DTOs.Profile;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.BLL.Interfaces
{
    public interface IVehicleService
    {
        Task<VehicleDTO> GetByIdAsync(int vehicleId);
        Task<VehicleDTO> GetVehicleByDriverIdAsync(int driverId);   // NEW
        Task<IEnumerable<VehicleDTO>> GetAllAsync();
        Task<VehicleDTO> CreateVehicleAsync(int driverId, CreateVehicleDTO dto);  // driverId comes from JWT
        Task<VehicleDTO> UpdateVehicleAsync(int vehicleId, UpdateVehicleDTO dto);
        Task<bool> VerifyVehicleAsync(int vehicleId, bool isVerified);
    }

}
