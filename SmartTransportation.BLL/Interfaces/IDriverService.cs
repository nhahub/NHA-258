using SmartTransportation.BLL.DTOs.Driver;
using SmartTransportation.BLL.DTOs.Profile;

public interface IDriverService
{
    Task<DriverProfileDTO> CreateDriverAsync(CreateDriverProfileDTO dto, int userId); // updated
    Task<DriverProfileDTO> UpdateDriverAsync(int driverId, UpdateDriverProfileDTO dto);
    Task<DriverFullDTO> GetDriverFullByIdAsync(int driverId);
    Task<DriverProfileDTO> GetDriverByIdAsync(int driverId);
    Task<IEnumerable<DriverProfileDTO>> GetAllDriversAsync();
    Task<bool> VerifyDriverAsync(int driverId, bool isVerified);
    Task<DriverDashboardDto> GetDashboardAsync(int driverId);

}
