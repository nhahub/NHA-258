using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartTransportation.BLL.DTOs.Profile;
using SmartTransportation.BLL.DTOs.Route;
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
        private readonly IRouteService _routeService;

        public Admin2Model(IAdminService adminService, IUnitOfWork unitOfWork, IRouteService routeService)
        {
            _adminService = adminService;
            _unitOfWork = unitOfWork;
            _routeService = routeService;
        }

        // ================== DRIVERS ==================
        public class AdminDriverDTO
        {
            public int DriverId { get; set; }
            public DriverProfileDTO Driver { get; set; }
            public List<VehicleDTO> Vehicles { get; set; } = new();
        }

        public List<AdminDriverDTO> Drivers { get; set; } = new();

        // ================== ROUTES ==================
        // Bound property for create form
        [BindProperty]
        public CreateRouteDTO NewRoute { get; set; } = new();

        // List of routes (loaded on GET)
        public List<RouteDetailsDTO> Routes { get; set; } = new();

        // On GET: load drivers + routes
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

            // Load routes (including segments) for display
            var allRoutes = await _routeService.GetAllRoutesAsync();
            Routes = allRoutes.OrderBy(r => r.RouteName).ToList();

            return Page();
        }

        // Handler to create route (form POST)
        public async Task<IActionResult> OnPostAddRouteAsync()
        {
            // Server-side validation: ensure required fields
            if (string.IsNullOrWhiteSpace(NewRoute.RouteName)
                || string.IsNullOrWhiteSpace(NewRoute.StartLocation)
                || string.IsNullOrWhiteSpace(NewRoute.EndLocation))
            {
                ModelState.AddModelError(string.Empty, "Route Name, Start Location and End Location are required.");
            }

            // Remove empty segments from model (if any)
            if (NewRoute.Segments != null && NewRoute.Segments.Any())
            {
                NewRoute.Segments = NewRoute.Segments
                    .Where(s => !string.IsNullOrWhiteSpace(s.StartPoint) && !string.IsNullOrWhiteSpace(s.EndPoint))
                    .Select((s, idx) =>
                    {
                        s.SegmentOrder = idx + 1;
                        return s;
                    })
                    .ToList();
            }

            if (!ModelState.IsValid)
            {
                // reload display lists and show errors
                var allRoutes = await _routeService.GetAllRoutesAsync();
                Routes = allRoutes.OrderBy(r => r.RouteName).ToList();
                // preserve NewRoute for user to fix
                return Page();
            }

            // If segments were not provided, create a default single segment from Start->End
            if (NewRoute.Segments == null || NewRoute.Segments.Count == 0)
            {
                NewRoute.Segments = new List<CreateSegmentDTO>
                {
                    new CreateSegmentDTO
                    {
                        SegmentOrder = 1,
                        StartPoint = NewRoute.StartLocation,
                        EndPoint = NewRoute.EndLocation,
                        SegmentDistanceKm = 0,
                        SegmentEstimatedMinutes = 0
                    }
                };
            }

            try
            {
                var created = await _routeService.CreateRouteAsync(NewRoute);
                // Clear NewRoute after creation
                NewRoute = new CreateRouteDTO();
                // Redirect to GET to avoid repost
                return RedirectToPage();
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error adding route: {ex.Message}");
                var allRoutes = await _routeService.GetAllRoutesAsync();
                Routes = allRoutes.OrderBy(r => r.RouteName).ToList();
                return Page();
            }
        }

        // ================== DRIVER / VEHICLE VERIFICATION (UNCHANGED) ==================
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
