using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.BLL.Services;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// Add Services
// ==========================================

// MVC Controllers + Views
builder.Services.AddControllersWithViews();

// Razor Pages
builder.Services.AddRazorPages();

// HttpClient (for consuming APIs)
builder.Services.AddHttpClient();

// ===================
// Database (EF Core)
// ===================
builder.Services.AddDbContext<TransportationContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===================
// UnitOfWork & BLL Services
// ===================
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

// ===================
// Authentication (Cookie)
// ===================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Log_In";              // Redirect here if not authenticated
        options.AccessDeniedPath = "/AccessDenied"; // Redirect here if user is unauthorized
        options.ReturnUrlParameter = "ReturnUrl";   // Preserve ReturnUrl
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });

// ===================
// Razor Pages Authorization
// ===================
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Dashboard");      // Require login for Dashboard
    options.Conventions.AuthorizePage("/Driver_Profile"); // Require login for Driver Profile
});

var app = builder.Build();

// ==========================================
// Middleware
// ==========================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); // Must come BEFORE authorization
app.UseAuthorization();

// ==========================================
// Routing (MVC + Razor Pages)
// ==========================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapRazorPages();

app.Run();
