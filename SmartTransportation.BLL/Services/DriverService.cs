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
    public class DriverService : IDriverService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DriverService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DriverProfileDTO> CreateDriverAsync(CreateDriverProfileDTO dto, int userId)
        {
            // Fetch the user first
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) throw new Exception("User not found");

            // Automatically mark user as driver if UserTypeId == 2
            if (user.UserTypeId != 2)
            {
                user.UserTypeId = 2;  // 2 = Driver
                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveAsync();
            }

            // Check if the user already has a profile
            var existingProfile = await _unitOfWork.UserProfiles.GetByUserIdAsync(userId);
            if (existingProfile != null)
                return MapToDriverProfileDTO(existingProfile);

            // Create driver profile
            var profile = new UserProfile
            {
                UserId = userId,
                FullName = dto.FullName,
                Phone = dto.Phone,
                Address = dto.Address,
                City = dto.City,
                Country = dto.Country,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                ProfilePhotoUrl = dto.ProfilePhotoUrl,
                DriverLicenseNumber = dto.DriverLicenseNumber,
                DriverLicenseExpiry = dto.DriverLicenseExpiry,
                IsDriver = true,
                IsDriverVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.UserProfiles.AddAsync(profile);
            await _unitOfWork.SaveAsync();

            return MapToDriverProfileDTO(profile);
        }

        // -------------------------------
        // Get driver with vehicle
        // -------------------------------
        public async Task<DriverFullDTO> GetDriverFullByIdAsync(int driverId)
        {
            var profile = await _unitOfWork.UserProfiles.GetByUserIdAsync(driverId);
            if (profile == null) return null;

            var vehicle = await _unitOfWork.Vehicles.GetQueryable()
                .FirstOrDefaultAsync(v => v.DriverId == driverId);

            return new DriverFullDTO
            {
                Driver = MapToDriverProfileDTO(profile),
                Vehicle = vehicle != null ? MapToVehicleDTO(vehicle) : null
            };
        }

        // -------------------------------
        // Get all drivers
        // -------------------------------
        public async Task<IEnumerable<DriverProfileDTO>> GetAllDriversAsync()
        {
            var drivers = await _unitOfWork.UserProfiles.GetQueryable()
                .Where(d => d.IsDriver)
                .ToListAsync();

            return drivers.Select(MapToDriverProfileDTO);
        }

        // -------------------------------
        // Get driver by Id
        // -------------------------------
        public async Task<DriverProfileDTO> GetDriverByIdAsync(int driverId)
        {
            var entity = await _unitOfWork.UserProfiles.GetByUserIdAsync(driverId);
            return entity == null ? null : MapToDriverProfileDTO(entity);
        }

        // -------------------------------
        // Update driver profile
        // -------------------------------
        public async Task<DriverProfileDTO> UpdateDriverAsync(int driverId, UpdateDriverProfileDTO dto)
        {
            var entity = await _unitOfWork.UserProfiles.GetByUserIdAsync(driverId);
            if (entity == null) return null;

            entity.FullName = dto.FullName ?? entity.FullName;
            entity.Phone = dto.Phone ?? entity.Phone;
            entity.Address = dto.Address ?? entity.Address;
            entity.City = dto.City ?? entity.City;
            entity.Country = dto.Country ?? entity.Country;
            entity.DateOfBirth = dto.DateOfBirth ?? entity.DateOfBirth;
            entity.Gender = dto.Gender ?? entity.Gender;
            entity.ProfilePhotoUrl = dto.ProfilePhotoUrl ?? entity.ProfilePhotoUrl;
            entity.DriverLicenseNumber = dto.DriverLicenseNumber ?? entity.DriverLicenseNumber;
            entity.DriverLicenseExpiry = dto.DriverLicenseExpiry ?? entity.DriverLicenseExpiry;
            entity.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.UserProfiles.Update(entity);
            await _unitOfWork.SaveAsync();

            return MapToDriverProfileDTO(entity);
        }

        // -------------------------------
        // Verify driver
        // -------------------------------
        public async Task<bool> VerifyDriverAsync(int driverId, bool isVerified)
        {
            var entity = await _unitOfWork.UserProfiles.GetByUserIdAsync(driverId);
            if (entity == null) return false;

            entity.IsDriverVerified = isVerified;
            _unitOfWork.UserProfiles.Update(entity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // -------------------------------
        // Manual mapping helpers
        // -------------------------------
        private DriverProfileDTO MapToDriverProfileDTO(UserProfile entity)
        {
            return new DriverProfileDTO
            {
                //UserId = entity.UserId,
                FullName = entity.FullName,
                Phone = entity.Phone,
                Address = entity.Address,
                City = entity.City,
                Country = entity.Country,
                DateOfBirth = entity.DateOfBirth,
                Gender = entity.Gender,
                ProfilePhotoUrl = entity.ProfilePhotoUrl,
                DriverLicenseNumber = entity.DriverLicenseNumber,
                DriverLicenseExpiry = entity.DriverLicenseExpiry,
                DriverRating = entity.DriverRating,
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
                VehicleLicenseExpiry = entity.VehicleLicenseExpiry,
                IsVerified = entity.IsVerified
            };
        }
    }
}
