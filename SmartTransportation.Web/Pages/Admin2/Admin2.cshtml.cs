using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartTransportation.BLL.DTOs.Profile;
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.UnitOfWork;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTransportation.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class Admin2Model : PageModel
    {
        private readonly IAdminService _adminService;
        private readonly IUnitOfWork _unitOfWork;

        public Admin2Model(IAdminService adminService, IUnitOfWork unitOfWork)
        {
            _adminService = adminService;
            _unitOfWork = unitOfWork;
        }

        public class AdminDriverDTO
        {
            public int DriverId { get; set; }
            public DriverProfileDTO Driver { get; set; }
            public List<VehicleDTO> Vehicles { get; set; } = new();
        }

        public List<AdminDriverDTO> Drivers { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(bool? onlyVerified = null)
        {
            Drivers = new List<AdminDriverDTO>();

            var driverEntities = await _unitOfWork.UserProfiles
                .GetQueryable()
                .Where(u => u.IsDriver && (!onlyVerified.HasValue || u.IsDriverVerified == onlyVerified.Value))
                .ToListAsync();

            foreach (var driverEntity in driverEntities)
            {
                var driverDto = MapToDriverProfileDTO(driverEntity);
                var vehicles = (await _adminService.GetVehiclesByDriverAsync(driverEntity.UserId)).ToList();

                Drivers.Add(new AdminDriverDTO
                {
                    DriverId = driverEntity.UserId,
                    Driver = driverDto,
                    Vehicles = vehicles
                });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostVerifyDriverAsync(int driverId, bool isVerified)
        {
            var ok = await _adminService.VerifyDriverAsync(driverId, isVerified);
            if (!ok)
                return new JsonResult(new { success = false, message = "Driver not found" });

            if (!isVerified)
            {
                var vehicles = await _adminService.GetVehiclesByDriverAsync(driverId);
                foreach (var v in vehicles)
                {
                    await _adminService.VerifyVehicleAsync(v.VehicleId, driverId, false);
                }
            }

            return new JsonResult(new
            {
                success = true,
                message = isVerified ? "Driver verified successfully" : "Driver rejected successfully"
            });
        }

        public async Task<IActionResult> OnPostVerifyVehicleAsync(int driverId, int vehicleId, bool isVerified)
        {
            var driver = await _unitOfWork.UserProfiles.GetByUserIdAsync(driverId);
            if (driver == null)
                return new JsonResult(new { success = false, message = "Driver not found" });

            if (!driver.IsDriverVerified && isVerified)
                return new JsonResult(new { success = false, message = "Cannot verify vehicle because driver is not verified." });

            var ok = await _adminService.VerifyVehicleAsync(vehicleId, driverId, isVerified);
            if (!ok)
                return new JsonResult(new { success = false, message = "Vehicle not found" });

            return new JsonResult(new
            {
                success = true,
                message = isVerified ? "Vehicle verified successfully" : "Vehicle rejected successfully"
            });
        }

        private DriverProfileDTO MapToDriverProfileDTO(UserProfile entity)
        {
            return new DriverProfileDTO
            {
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
    }
}
