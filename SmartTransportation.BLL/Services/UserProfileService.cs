using SmartTransportation.BLL.DTOs.Profile;
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.UnitOfWork;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SmartTransportation.BLL.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserProfileService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BaseUserProfileDTO> CreateAsync(CreateUserProfileDTO dto)
        {
            var entity = _mapper.Map<UserProfile>(dto);
            entity.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.UserProfiles.AddAsync(entity);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<BaseUserProfileDTO>(entity);
        }

        public async Task<IEnumerable<BaseUserProfileDTO>> GetAllAsync()
        {
            var entities = await _unitOfWork.UserProfiles.GetAllAsync();
            return _mapper.Map<IEnumerable<BaseUserProfileDTO>>(entities);
        }

        public async Task<BaseUserProfileDTO> GetByUserIdAsync(int userId)
        {
            var entity = await _unitOfWork.UserProfiles.GetByUserIdAsync(userId);
            return _mapper.Map<BaseUserProfileDTO>(entity);
        }

        public async Task<BaseUserProfileDTO> UpdateAsync(int userId, UpdateUserProfileDTO dto)
        {
            var entity = await _unitOfWork.UserProfiles.GetByUserIdAsync(userId);
            if (entity == null) return null;

            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.UserProfiles.Update(entity);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<BaseUserProfileDTO>(entity);
        }
    }
}
