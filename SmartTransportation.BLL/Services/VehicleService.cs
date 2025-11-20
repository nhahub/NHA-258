using SmartTransportation.BLL.DTOs.Profile;
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class VehicleService : IVehicleService
{
    private readonly IUnitOfWork _unitOfWork;

    public VehicleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<VehicleDTO> CreateVehicleAsync(int driverId, CreateVehicleDTO dto)
    {
        var entity = new Vehicle
        {
            DriverId = driverId,  // assign from JWT
            VehicleMake = dto.VehicleMake,
            VehicleModel = dto.VehicleModel,
            VehicleYear = dto.VehicleYear,
            PlateNumber = dto.PlateNumber,
            Color = dto.Color,
            SeatsCount = dto.SeatsCount,
            VehicleLicenseNumber = dto.VehicleLicenseNumber,
            VehicleLicenseExpiry = dto.VehicleLicenseExpiry,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Vehicles.AddAsync(entity);
        await _unitOfWork.SaveAsync();

        return MapToVehicleDTO(entity);
    }

    public async Task<VehicleDTO> UpdateVehicleAsync(int vehicleId, UpdateVehicleDTO dto)
    {
        var entity = await _unitOfWork.Vehicles.GetByIdAsync(vehicleId);
        if (entity == null) return null;

        entity.VehicleMake = dto.VehicleMake ?? entity.VehicleMake;
        entity.VehicleModel = dto.VehicleModel ?? entity.VehicleModel;
        entity.VehicleYear = dto.VehicleYear ?? entity.VehicleYear;
        entity.PlateNumber = dto.PlateNumber ?? entity.PlateNumber;
        entity.Color = dto.Color ?? entity.Color;
        entity.SeatsCount = dto.SeatsCount;
        entity.VehicleLicenseNumber = dto.VehicleLicenseNumber ?? entity.VehicleLicenseNumber;
        entity.VehicleLicenseExpiry = dto.VehicleLicenseExpiry ?? entity.VehicleLicenseExpiry;

        _unitOfWork.Vehicles.Update(entity);
        await _unitOfWork.SaveAsync();

        return MapToVehicleDTO(entity);
    }

    public async Task<VehicleDTO> GetByIdAsync(int vehicleId)
    {
        var entity = await _unitOfWork.Vehicles.GetByIdAsync(vehicleId);
        return entity == null ? null : MapToVehicleDTO(entity);
    }

    public async Task<VehicleDTO> GetVehicleByDriverIdAsync(int driverId)
    {
        var entity = (await _unitOfWork.Vehicles.GetAllAsync())
                     .FirstOrDefault(v => v.DriverId == driverId);
        return entity == null ? null : MapToVehicleDTO(entity);
    }

    public async Task<IEnumerable<VehicleDTO>> GetAllAsync()
    {
        var entities = await _unitOfWork.Vehicles.GetAllAsync();
        return entities.Select(MapToVehicleDTO);
    }

    public async Task<bool> VerifyVehicleAsync(int vehicleId, bool isVerified)
    {
        var entity = await _unitOfWork.Vehicles.GetByIdAsync(vehicleId);
        if (entity == null) return false;

        entity.IsVerified = isVerified;
        _unitOfWork.Vehicles.Update(entity);
        await _unitOfWork.SaveAsync();
        return true;
    }

    // ----------------------
    // Manual mapping helper
    // ----------------------
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
