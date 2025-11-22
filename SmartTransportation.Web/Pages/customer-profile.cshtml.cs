using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartTransportation.BLL.DTOs.Profile;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SmartTransportation.Web.Pages
{
    [Authorize(Roles = "Passenger")] // <-- CHANGE THIS
    public class Customer_ProfileModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public Customer_ProfileModel(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty] public string FullName { get; set; } = "";
        [BindProperty] public string? Phone { get; set; }
        [BindProperty] public string? Address { get; set; }
        [BindProperty] public string? City { get; set; }
        [BindProperty] public string? Country { get; set; }
        [BindProperty] public DateOnly? DateOfBirth { get; set; }
        [BindProperty] public string? Gender { get; set; }

        [TempData] public string? SuccessMessage { get; set; }
        [TempData] public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Log_In");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiBaseUrl = _configuration["ApiBaseUrl"];
            var response = await client.GetAsync($"{apiBaseUrl}/api/Passenger/profile"); // <-- API endpoint

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Failed to load profile.";
                return Page();
            }

            var json = await response.Content.ReadAsStringAsync();
            var passengerDto = JsonSerializer.Deserialize<BaseUserProfileDTO>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (passengerDto != null)
            {
                FullName = passengerDto.FullName ?? "";
                Phone = passengerDto.Phone;
                Address = passengerDto.Address;
                City = passengerDto.City;
                Country = passengerDto.Country;
                DateOfBirth = passengerDto.DateOfBirth;
                Gender = passengerDto.Gender;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Log_In");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiBaseUrl = _configuration["ApiBaseUrl"];
            var dto = new UpdateUserProfileDTO
            {
                FullName = FullName,
                Phone = Phone,
                Address = Address,
                City = City,
                Country = Country,
                DateOfBirth = DateOfBirth,
                Gender = Gender
            };

            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"{apiBaseUrl}/api/Passenger/profile", content); // <-- API endpoint

            if (response.IsSuccessStatusCode)
                SuccessMessage = "Profile updated successfully!";
            else
                ErrorMessage = "Failed to update profile.";

            return RedirectToPage();
        }
    }
}
