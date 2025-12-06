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
using System.Linq.Expressions;
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
        [BindProperty] public CreateRouteDTO NewRoute { get; set; } = new();
        [BindProperty] public UpdateRouteDTO EditRoute { get; set; } = new();
        public List<RouteDetailsDTO> Routes { get; set; } = new();
        public bool IsEditing { get; set; }
        public int? EditingRouteId { get; set; }

        // GET: Load drivers + routes
        public async Task<IActionResult> OnGetAsync(bool? onlyVerified = null)
        {
            await LoadDataAsync(onlyVerified);
            return Page();
        }

        private async Task LoadDataAsync(bool? onlyVerified = null)
        {
            // Load Drivers
            var driverEntities = await _unitOfWork.UserProfiles
                .GetQueryable()
                .Where(u => u.IsDriver && (!onlyVerified.HasValue || u.IsDriverVerified == onlyVerified.Value))
                .ToListAsync();

            Drivers = new List<AdminDriverDTO>();
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

            // Load Routes
            var allRoutes = await _routeService.GetAllRoutesAsync();
            Routes = allRoutes.OrderBy(r => r.RouteName).ToList();
        }

        // ================== ROUTE CRUD ==================
        public async Task<IActionResult> OnPostAddRouteAsync()
        {
            // Manual validation
            if (string.IsNullOrWhiteSpace(NewRoute.RouteName))
                ModelState.AddModelError(nameof(NewRoute.RouteName), "Route Name is required.");
            if (string.IsNullOrWhiteSpace(NewRoute.StartLocation))
                ModelState.AddModelError(nameof(NewRoute.StartLocation), "Start Location is required.");
            if (string.IsNullOrWhiteSpace(NewRoute.EndLocation))
                ModelState.AddModelError(nameof(NewRoute.EndLocation), "End Location is required.");

            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            // Clean segments
            if (NewRoute.Segments?.Any() == true)
            {
                NewRoute.Segments = NewRoute.Segments
                    .Where(s => !string.IsNullOrWhiteSpace(s.StartPoint) && !string.IsNullOrWhiteSpace(s.EndPoint))
                    .Select((s, idx) => { s.SegmentOrder = idx + 1; return s; })
                    .ToList();
            }

            // If no valid segments, create default
            if (NewRoute.Segments == null || !NewRoute.Segments.Any())
            {
                NewRoute.Segments = new List<CreateSegmentDTO>
        {
            new() { SegmentOrder = 1, StartPoint = NewRoute.StartLocation, EndPoint = NewRoute.EndLocation }
        };
            }

            try
            {
                await _routeService.CreateRouteAsync(NewRoute);
                TempData["SuccessMessage"] = "Route created successfully!";
                NewRoute = new CreateRouteDTO(); // reset form
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                await LoadDataAsync();
                return Page();
            }
        }


        public async Task<IActionResult> OnPostUpdateRouteAsync()
        {
            if (EditRoute?.RouteId <= 0 || string.IsNullOrWhiteSpace(EditRoute.RouteName) ||
                string.IsNullOrWhiteSpace(EditRoute.StartLocation) || string.IsNullOrWhiteSpace(EditRoute.EndLocation))
            {
                ModelState.AddModelError("", "Invalid route data.");
                await LoadDataAsync();
                return Page();
            }

            if (EditRoute.Segments?.Any() == true)
            {
                EditRoute.Segments = EditRoute.Segments
                    .Where(s => !string.IsNullOrWhiteSpace(s.StartPoint) && !string.IsNullOrWhiteSpace(s.EndPoint))
                    .Select((s, idx) => { s.SegmentOrder = idx + 1; return s; })
                    .ToList();
            }

            if (EditRoute.Segments == null || !EditRoute.Segments.Any())
            {
                EditRoute.Segments = new List<CreateSegmentDTO>
                {
                    new() { SegmentOrder = 1, StartPoint = EditRoute.StartLocation, EndPoint = EditRoute.EndLocation }
                };
            }

            try
            {
                var updated = await _routeService.UpdateRouteAsync(EditRoute.RouteId, EditRoute);
                if (updated == null)
                {
                    ModelState.AddModelError("", "Route not found.");
                    await LoadDataAsync();
                    return Page();
                }

                TempData["SuccessMessage"] = "Route updated successfully!";
                EditRoute = new UpdateRouteDTO();
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                await LoadDataAsync();
                return Page();
            }
        }
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeleteRouteAsync(int routeId)
        {
            try
            {
                var deleted = await _routeService.DeleteRouteAsync(routeId); // << use _routeService here
                if (!deleted)
                    return new JsonResult(new { success = false, message = "Route not found" });

                return new JsonResult(new { success = true, message = "Route and related data deleted successfully!" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Delete failed: {ex.Message}" });
            }
        }



        public async Task<IActionResult> OnGetEditRouteAsync(int routeId)
        {
            await LoadDataAsync();
            var route = await _routeService.GetRouteDetailsByIdAsync(routeId);
            if (route != null)
            {
                EditRoute = new UpdateRouteDTO
                {
                    RouteId = route.RouteId,
                    RouteName = route.RouteName,
                    StartLocation = route.StartLocation,
                    EndLocation = route.EndLocation,
                    RouteType = route.RouteType,
                    IsCircular = route.IsCircular,
                    Segments = route.Segments.Select(s => new CreateSegmentDTO
                    {
                        SegmentOrder = s.SegmentOrder,
                        StartPoint = s.StartPoint,
                        EndPoint = s.EndPoint,
                        SegmentDistanceKm = s.DistanceKm,
                        SegmentEstimatedMinutes = s.EstimatedMinutes
                    }).ToList()
                };
                IsEditing = true;
                EditingRouteId = routeId;
            }
            return Page();
        }

        // ================== DRIVER / VEHICLE VERIFICATION ==================
        public async Task<IActionResult> OnPostVerifyDriverAsync(int driverId, bool isVerified)
        {
            var ok = await _adminService.VerifyDriverAsync(driverId, isVerified);
            if (!ok) return new JsonResult(new { success = false, message = "Driver not found" });

            if (!isVerified)
            {
                var vehicles = await _adminService.GetVehiclesByDriverAsync(driverId);
                foreach (var v in vehicles) await _adminService.VerifyVehicleAsync(v.VehicleId, driverId, false);
            }

            return new JsonResult(new { success = true, message = isVerified ? "Verified!" : "Rejected!" });
        }

        public async Task<IActionResult> OnPostVerifyVehicleAsync(int driverId, int vehicleId, bool isVerified)
        {
            var driver = await _unitOfWork.UserProfiles.GetByUserIdAsync(driverId);
            if (driver == null) return new JsonResult(new { success = false, message = "Driver not found" });
            if (!driver.IsDriverVerified && isVerified)
                return new JsonResult(new { success = false, message = "Driver must be verified first" });

            var ok = await _adminService.VerifyVehicleAsync(vehicleId, driverId, isVerified);
            if (!ok) return new JsonResult(new { success = false, message = "Vehicle not found" });

            return new JsonResult(new { success = true, message = isVerified ? "Verified!" : "Rejected!" });
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
