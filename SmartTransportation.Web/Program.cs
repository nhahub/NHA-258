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
builder.Services.AddScoped<IUserProfileService, PassengerService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IAdminService, AdminService>();
// ✅ Register StripePaymentService so it can be injected into Razor Pages
builder.Services.AddScoped<StripePaymentService>();

// ===================
// Session (for JWT storage)
// ===================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Added for logout
});

// ===================
// Authentication (Cookie)
// ===================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Log_In";
        options.LogoutPath = "/Logout";             // ✅ Explicit logout path
        options.AccessDeniedPath = "/AccessDenied";
        options.ReturnUrlParameter = "ReturnUrl";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;

        // ✅ Added for proper logout behavior
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;

        // ✅ Events to ensure clean logout
        options.Events = new CookieAuthenticationEvents
        {
            OnSigningOut = async context =>
            {
                // Clear any additional data on logout
                context.HttpContext.Session.Clear();
                await Task.CompletedTask;
            }
        };
    });

// ===================
// Razor Pages Authorization
// ===================
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Dashboard");
    options.Conventions.AuthorizePage("/Driver_Profile");
    options.Conventions.AuthorizePage("/Payment/Pay");
    options.Conventions.AuthorizePage("/customer-profile"); // ✅ Added
    options.Conventions.AuthorizePage("/DriverDashboard");  // ✅ Added
    options.Conventions.AuthorizePage("/AdminDashboard");   // ✅ Added
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

app.UseSession();          // ✅ Must be before Authentication

// ✅ Added: Middleware to prevent caching of authenticated pages
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    await next();
});

app.UseAuthentication();
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