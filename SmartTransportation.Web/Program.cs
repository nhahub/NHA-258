var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------
// Add Services
// --------------------------------------------

// Enable MVC (Controllers + Views)
builder.Services.AddControllersWithViews();

// Enable Razor Pages (optional, if you use them)
builder.Services.AddRazorPages();

// HttpClient (required for your PaymentController)
builder.Services.AddHttpClient();

var app = builder.Build();

// --------------------------------------------
// Configure Middleware
// --------------------------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Allow serving CSS, JS, images, etc.
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// --------------------------------------------
// Routes
// --------------------------------------------

// MVC controller routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Razor pages route (optional)
app.MapRazorPages();

// --------------------------------------------

app.Run();
