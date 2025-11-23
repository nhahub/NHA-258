using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartTransportation.BLL.DTOs.Profile;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SmartTransportation.Web.Pages
{
    [Authorize(Roles = "Driver")]
    public class Driver_ProfileModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public Driver_ProfileModel(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty] public UpdateDriverProfileDTO DriverDto { get; set; } = new();
        [BindProperty] public UpdateVehicleDTO VehicleDto { get; set; } = new(); // VehicleId will be ignored on client side

        [TempData] public string? SuccessMessage { get; set; }
        [TempData] public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Log_In");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiBaseUrl = _configuration["ApiBaseUrl"];
            var response = await client.GetAsync($"{apiBaseUrl}/api/Driver/profile");

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Failed to load profile.";
                return Page();
            }

            var json = await response.Content.ReadAsStringAsync();
            var driverFull = JsonSerializer.Deserialize<DriverFullDTO>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (driverFull != null)
            {
                DriverDto.FullName = driverFull.Driver.FullName ?? "";
                DriverDto.Phone = driverFull.Driver.Phone;
                DriverDto.Address = driverFull.Driver.Address;
                DriverDto.City = driverFull.Driver.City;
                DriverDto.Country = driverFull.Driver.Country;
                DriverDto.DateOfBirth = driverFull.Driver.DateOfBirth;
                DriverDto.Gender = driverFull.Driver.Gender;
                DriverDto.DriverLicenseNumber = driverFull.Driver.DriverLicenseNumber;
                DriverDto.DriverLicenseExpiry = driverFull.Driver.DriverLicenseExpiry;

                if (driverFull.Vehicle != null)
                {
                    // Only map the editable vehicle fields. Ignore VehicleId.
                    VehicleDto.VehicleMake = driverFull.Vehicle.VehicleMake;
                    VehicleDto.VehicleModel = driverFull.Vehicle.VehicleModel;
                    VehicleDto.VehicleYear = driverFull.Vehicle.VehicleYear;
                    VehicleDto.PlateNumber = driverFull.Vehicle.PlateNumber;
                    VehicleDto.Color = driverFull.Vehicle.Color;
                    VehicleDto.SeatsCount = driverFull.Vehicle.SeatsCount;
                    VehicleDto.VehicleLicenseNumber = driverFull.Vehicle.VehicleLicenseNumber;
                    VehicleDto.VehicleLicenseExpiry = driverFull.Vehicle.VehicleLicenseExpiry;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateDriverAsync()
        {
            if (!ModelState.IsValid) return Page();

            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Log_In");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiBaseUrl = _configuration["ApiBaseUrl"];
            var content = new StringContent(JsonSerializer.Serialize(DriverDto), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"{apiBaseUrl}/api/Driver/profile", content);

            if (response.IsSuccessStatusCode)
                SuccessMessage = "Profile updated successfully!";
            else
                ErrorMessage = "Failed to update profile.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateVehicleAsync()
        {
            if (!ModelState.IsValid) return Page();

            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Log_In");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiBaseUrl = _configuration["ApiBaseUrl"];
            // Do not send VehicleId; server will detect vehicle by logged-in driver
            var vehiclePayload = new
            {
                VehicleMake = VehicleDto.VehicleMake,
                VehicleModel = VehicleDto.VehicleModel,
                VehicleYear = VehicleDto.VehicleYear,
                PlateNumber = VehicleDto.PlateNumber,
                Color = VehicleDto.Color,
                SeatsCount = VehicleDto.SeatsCount,
                VehicleLicenseNumber = VehicleDto.VehicleLicenseNumber,
                VehicleLicenseExpiry = VehicleDto.VehicleLicenseExpiry
            };

            var content = new StringContent(JsonSerializer.Serialize(vehiclePayload), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"{apiBaseUrl}/api/Driver/vehicle", content);

            if (response.IsSuccessStatusCode)
                SuccessMessage = "Vehicle updated successfully!";
            else
                ErrorMessage = "Failed to update vehicle.";

            return RedirectToPage();
        }
    }
}
