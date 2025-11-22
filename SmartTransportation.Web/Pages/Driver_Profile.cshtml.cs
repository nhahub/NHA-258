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

        // -----------------------------
        // Driver Properties
        // -----------------------------
        [BindProperty] public string FullName { get; set; } = "";
        [BindProperty] public string? Phone { get; set; }
        [BindProperty] public string? Address { get; set; }
        [BindProperty] public string? City { get; set; }
        [BindProperty] public string? Country { get; set; }
        [BindProperty] public DateOnly? DateOfBirth { get; set; }
        [BindProperty] public string? Gender { get; set; }
        [BindProperty] public string? DriverLicenseNumber { get; set; }
        [BindProperty] public DateOnly? DriverLicenseExpiry { get; set; }

        // -----------------------------
        // Vehicle Properties
        // -----------------------------
        [BindProperty] public int VehicleId { get; set; }
        [BindProperty] public string? VehicleMake { get; set; }
        [BindProperty] public string? VehicleModel { get; set; }
        [BindProperty] public int? VehicleYear { get; set; }
        [BindProperty] public string? PlateNumber { get; set; }
        [BindProperty] public string? Color { get; set; }
        [BindProperty] public int SeatsCount { get; set; }
        [BindProperty] public string? VehicleLicenseNumber { get; set; }
        [BindProperty] public DateOnly? VehicleLicenseExpiry { get; set; }

        [TempData] public string? SuccessMessage { get; set; }
        [TempData] public string? ErrorMessage { get; set; }

        // -----------------------------
        // GET: Load driver & vehicle
        // -----------------------------
        public async Task<IActionResult> OnGetAsync()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Log_In");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiBaseUrl = _configuration["ApiBaseUrl"];
            var response = await client.GetAsync($"{apiBaseUrl}/api/Driver/profile"); // API reads JWT

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Failed to load profile.";
                return Page();
            }

            var json = await response.Content.ReadAsStringAsync();
            var driverFull = JsonSerializer.Deserialize<DriverFullDTO>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (driverFull != null)
            {
                FullName = driverFull.Driver.FullName ?? "";
                Phone = driverFull.Driver.Phone;
                Address = driverFull.Driver.Address;
                City = driverFull.Driver.City;
                Country = driverFull.Driver.Country;
                DateOfBirth = driverFull.Driver.DateOfBirth;
                Gender = driverFull.Driver.Gender;
                DriverLicenseNumber = driverFull.Driver.DriverLicenseNumber;
                DriverLicenseExpiry = driverFull.Driver.DriverLicenseExpiry;

                if (driverFull.Vehicle != null)
                {
                    VehicleId = driverFull.Vehicle.VehicleId;
                    VehicleMake = driverFull.Vehicle.VehicleMake;
                    VehicleModel = driverFull.Vehicle.VehicleModel;
                    VehicleYear = driverFull.Vehicle.VehicleYear;
                    PlateNumber = driverFull.Vehicle.PlateNumber;
                    Color = driverFull.Vehicle.Color;
                    SeatsCount = driverFull.Vehicle.SeatsCount;
                    VehicleLicenseNumber = driverFull.Vehicle.VehicleLicenseNumber;
                    VehicleLicenseExpiry = driverFull.Vehicle.VehicleLicenseExpiry;
                }
            }

            return Page();
        }

        // -----------------------------
        // POST: Update driver
        // -----------------------------
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Log_In");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiBaseUrl = _configuration["ApiBaseUrl"];
            var dto = new UpdateDriverProfileDTO
            {
                FullName = FullName,
                Phone = Phone,
                Address = Address,
                City = City,
                Country = Country,
                DateOfBirth = DateOfBirth,
                Gender = Gender,
                DriverLicenseNumber = DriverLicenseNumber,
                DriverLicenseExpiry = DriverLicenseExpiry
            };

            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"{apiBaseUrl}/api/Driver/profile", content);

            if (response.IsSuccessStatusCode)
                SuccessMessage = "Profile updated successfully!";
            else
                ErrorMessage = "Failed to update profile.";

            return RedirectToPage();
        }

        // -----------------------------
        // POST: Update vehicle
        // -----------------------------
        public async Task<IActionResult> OnPostUpdateVehicleAsync()
        {
            if (!ModelState.IsValid) return Page();

            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Log_In");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiBaseUrl = _configuration["ApiBaseUrl"];
            var dto = new UpdateVehicleDTO
            {
                VehicleId = VehicleId,
                VehicleMake = VehicleMake,
                VehicleModel = VehicleModel,
                VehicleYear = VehicleYear,
                PlateNumber = PlateNumber,
                Color = Color,
                SeatsCount = SeatsCount,
                VehicleLicenseNumber = VehicleLicenseNumber,
                VehicleLicenseExpiry = VehicleLicenseExpiry
            };

            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"{apiBaseUrl}/api/Driver/vehicle", content);

            if (response.IsSuccessStatusCode)
                SuccessMessage = "Vehicle updated successfully!";
            else
                ErrorMessage = "Failed to update vehicle.";

            return RedirectToPage();
        }
    }
}
