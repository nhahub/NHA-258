using SmartTransportation.BLL.DTOs.Profile;
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.UnitOfWork;
using Microsoft.EntityFrameworkCore;
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
        var existing = await _unitOfWork.Vehicles
            .GetQueryable()
            .FirstOrDefaultAsync(v => v.DriverId == driverId);

        if (existing != null)
            throw new Exception("Driver already has a vehicle.");

        var entity = new Vehicle
        {
            DriverId = driverId,
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

        // Safe partial updates
        if (!string.IsNullOrWhiteSpace(dto.VehicleMake)) entity.VehicleMake = dto.VehicleMake;
        if (!string.IsNullOrWhiteSpace(dto.VehicleModel)) entity.VehicleModel = dto.VehicleModel;
        if (dto.VehicleYear.HasValue) entity.VehicleYear = dto.VehicleYear.Value;
        if (!string.IsNullOrWhiteSpace(dto.PlateNumber)) entity.PlateNumber = dto.PlateNumber;
        if (!string.IsNullOrWhiteSpace(dto.Color)) entity.Color = dto.Color;
        if (dto.SeatsCount > 0) entity.SeatsCount = dto.SeatsCount;
        if (!string.IsNullOrWhiteSpace(dto.VehicleLicenseNumber)) entity.VehicleLicenseNumber = dto.VehicleLicenseNumber;
        if (dto.VehicleLicenseExpiry.HasValue) entity.VehicleLicenseExpiry = dto.VehicleLicenseExpiry.Value;

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
        var entity = await _unitOfWork.Vehicles
            .GetQueryable()
            .FirstOrDefaultAsync(v => v.DriverId == driverId);

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
