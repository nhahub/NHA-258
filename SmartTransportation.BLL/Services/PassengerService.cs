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
    public class PassengerService : IUserProfileService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PassengerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // -------------------------------
        // Get passenger profile by userId
        // -------------------------------
        public async Task<BaseUserProfileDTO> GetByUserIdAsync(int userId)
        {
            var entity = await _unitOfWork.UserProfiles.GetByUserIdAsync(userId);
            if (entity == null) return null;

            return MapToDTO(entity);
        }

        // -------------------------------
        // Get all passengers
        // -------------------------------
        public async Task<IEnumerable<BaseUserProfileDTO>> GetAllAsync()
        {
            // Optionally include only users who are passengers via Users.UserTypeId
            var passengers = await _unitOfWork.UserProfiles.GetQueryable()
                .Include(p => p.User) // Include user info
                .Where(p => p.User.UserTypeId == 3) // 3 = Passenger
                .ToListAsync();

            return passengers.Select(MapToDTO);
        }

        // -------------------------------
        // Create passenger profile
        // -------------------------------
        public async Task<BaseUserProfileDTO> CreateAsync(CreateUserProfileDTO dto, int userId)
        {
            // Ensure user exists
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) throw new Exception("User not found");

            // Optional: ensure user type is passenger
            if (user.UserTypeId != 3)
            {
                user.UserTypeId = 3; // 3 = Passenger
                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveAsync();
            }

            // Check if profile already exists
            var existingProfile = await _unitOfWork.UserProfiles.GetByUserIdAsync(userId);
            if (existingProfile != null) return MapToDTO(existingProfile);

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
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.UserProfiles.AddAsync(profile);
            await _unitOfWork.SaveAsync();

            return MapToDTO(profile);
        }

        // -------------------------------
        // Update passenger profile
        // -------------------------------
        public async Task<BaseUserProfileDTO> UpdateAsync(int userId, UpdateUserProfileDTO dto)
        {
            var entity = await _unitOfWork.UserProfiles.GetByUserIdAsync(userId);
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
        // Manual mapping
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
    }
}
