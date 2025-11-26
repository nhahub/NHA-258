using Microsoft.EntityFrameworkCore;
using SmartTransportation.BLL.DTOs.Profile;
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTransportation.BLL.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // -------------------------------
        // Driver verification
        // -------------------------------
        public async Task<bool> VerifyDriverAsync(int driverId, bool isVerified)
        {
            var driver = await _unitOfWork.UserProfiles.GetByUserIdAsync(driverId);
            if (driver == null) return false;

            driver.IsDriverVerified = isVerified;
            _unitOfWork.UserProfiles.Update(driver);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<IEnumerable<DriverProfileDTO>> GetAllDriversAsync(bool? onlyVerified = null)
        {
            var query = _unitOfWork.UserProfiles.GetQueryable().Where(u => u.IsDriver);

            if (onlyVerified.HasValue)
                query = query.Where(d => d.IsDriverVerified == onlyVerified.Value);

            var drivers = await query.ToListAsync();
            return drivers.Select(MapToDriverProfileDTO);
        }

        // -------------------------------
        // Vehicle verification
        // -------------------------------
        public async Task<IEnumerable<VehicleDTO>> GetVehiclesByDriverAsync(int driverId)
        {
            var driver = await _unitOfWork.UserProfiles.GetByUserIdAsync(driverId);
            if (driver == null)
                throw new Exception("Driver not found");

            var vehicles = await _unitOfWork.Vehicles
                .GetQueryable()
                .Where(v => v.DriverId == driverId)
                .ToListAsync();

            return vehicles.Select(MapToVehicleDTO);
        }

        public async Task<bool> VerifyVehicleAsync(int vehicleId, int driverId, bool isVerified)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(vehicleId);
            if (vehicle == null) return false;

            var driver = await _unitOfWork.UserProfiles.GetByUserIdAsync(driverId);
            if (driver == null || !driver.IsDriverVerified)
                return false; // driver not verified

            vehicle.IsVerified = isVerified;
            _unitOfWork.Vehicles.Update(vehicle);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<IEnumerable<VehicleDTO>> GetAllVehiclesAsync(bool? onlyVerified = null)
        {
            var query = _unitOfWork.Vehicles.GetQueryable();

            if (onlyVerified.HasValue)
                query = query.Where(v => v.IsVerified == onlyVerified.Value);

            var vehicles = await query.ToListAsync();
            return vehicles.Select(MapToVehicleDTO);
        }

        // -------------------------------
        // Admin profile
        // -------------------------------
        public async Task<BaseUserProfileDTO> CreateAdminProfileAsync(CreateUserProfileDTO dto, int adminId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(adminId);
            if (user == null) throw new Exception("User not found");

            var existingProfile = await _unitOfWork.UserProfiles.GetByUserIdAsync(adminId);
            if (existingProfile != null)
                return MapToDTO(existingProfile);

            var profile = new UserProfile
            {
                UserId = adminId,
                FullName = dto.FullName,
                Phone = dto.Phone,
                Address = dto.Address,
                City = dto.City,
                Country = dto.Country,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                ProfilePhotoUrl = dto.ProfilePhotoUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.UserProfiles.AddAsync(profile);
            await _unitOfWork.SaveAsync();

            return MapToDTO(profile);
        }

        public async Task<BaseUserProfileDTO> GetAdminProfileAsync(int adminId)
        {
            var entity = await _unitOfWork.UserProfiles.GetByUserIdAsync(adminId);
            return entity == null ? null : MapToDTO(entity);
        }

        public async Task<BaseUserProfileDTO> UpdateAdminProfileAsync(int adminId, UpdateUserProfileDTO dto)
        {
            var entity = await _unitOfWork.UserProfiles.GetByUserIdAsync(adminId);
            if (entity == null) return null;

            entity.FullName = dto.FullName ?? entity.FullName;
            entity.Phone = dto.Phone ?? entity.Phone;
            entity.Address = dto.Address ?? entity.Address;
            entity.City = dto.City ?? entity.City;
            entity.Country = dto.Country ?? entity.Country;
            entity.DateOfBirth = dto.DateOfBirth ?? entity.DateOfBirth;
            entity.Gender = dto.Gender ?? entity.Gender;
            entity.ProfilePhotoUrl = dto.ProfilePhotoUrl ?? entity.ProfilePhotoUrl;
            entity.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.UserProfiles.Update(entity);
            await _unitOfWork.SaveAsync();
            return MapToDTO(entity);
        }

        // -------------------------------
        // Mapping helpers
        // -------------------------------
        private BaseUserProfileDTO MapToDTO(UserProfile entity)
        {
            return new BaseUserProfileDTO
            {
                FullName = entity.FullName,
                Phone = entity.Phone,
                Address = entity.Address,
                City = entity.City,
                Country = entity.Country,
                DateOfBirth = entity.DateOfBirth,
                Gender = entity.Gender,
                ProfilePhotoUrl = entity.ProfilePhotoUrl,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        private DriverProfileDTO MapToDriverProfileDTO(UserProfile entity)
        {
            return new DriverProfileDTO
            {
                UserId = entity.UserId,
                FullName = entity.FullName,
                Phone = entity.Phone,
                Address = entity.Address,
                City = entity.City,
                Country = entity.Country,
                DateOfBirth = entity.DateOfBirth,
                Gender = entity.Gender,
                ProfilePhotoUrl = entity.ProfilePhotoUrl,
                DriverLicenseNumber = entity.DriverLicenseNumber,
                // ✅ Convert DateOnly? to DateTime?
                DriverLicenseExpiry = entity.DriverLicenseExpiry.HasValue
                    ? entity.DriverLicenseExpiry.Value.ToDateTime(TimeOnly.MinValue)
                    : null,
                // ✅ Convert decimal? to double?
                DriverRating = entity.DriverRating.HasValue
                    ? (double?)entity.DriverRating.Value
                    : null,
                IsDriverVerified = entity.IsDriverVerified,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        private VehicleDTO MapToVehicleDTO(Vehicle entity)
        {
            return new VehicleDTO
            {
                VehicleId = entity.VehicleId,
                DriverId = entity.DriverId,
                VehicleMake = entity.VehicleMake,
                VehicleModel = entity.VehicleModel,
                VehicleYear = entity.VehicleYear,
                PlateNumber = entity.PlateNumber,
                Color = entity.Color,
                SeatsCount = entity.SeatsCount,
                VehicleLicenseNumber = entity.VehicleLicenseNumber,
                // ✅ Convert DateOnly? to DateTime?
                VehicleLicenseExpiry = entity.VehicleLicenseExpiry.HasValue
                    ? entity.VehicleLicenseExpiry.Value.ToDateTime(TimeOnly.MinValue)
                    : null,
                IsVerified = entity.IsVerified
            };
        }
    }
}