using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using SmartTransportation.API.Extensions;
using SmartTransportation.BLL.DTOs.Notification;
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.BLL.Jobs;
using SmartTransportation.BLL.Services;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.UnitOfWork;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =====================
// Add services
// =====================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// =====================
// Swagger with JWT support
// =====================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Smart Transportation API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token.\nExample: \"Bearer abc123\""
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// =====================
// Inject services
// =====================
builder.Services.AddScoped<StripePaymentService>();
builder.Services.AddHostedService<PaymentStatusBackgroundService>();
builder.Services.AddHttpClient();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddCorsPolicy();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddAutoMapper(typeof(Program)); // or the assembly containing your profiles

// If you have a concrete implementation called DriverService
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();



builder.Services.AddDbContext<TransportationContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// =====================
// JWT Authentication setup
// =====================
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };

    // Map UserType from JWT claims to RoleClaimType
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // If Authorization header is empty → try cookie
            if (string.IsNullOrEmpty(context.Token) &&
                context.Request.Cookies.ContainsKey("AuthToken"))
            {
                context.Token = context.Request.Cookies["AuthToken"];
            }

            return Task.CompletedTask;
        },

        OnTokenValidated = context =>
        {
            var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                var userTypeClaim = claimsIdentity.FindFirst("UserType");
                if (userTypeClaim != null)
                {
                    string roleName = userTypeClaim.Value switch
                    {
                        "1" => "Admin",
                        "2" => "Driver",
                        "3" => "Passenger",
                        _ => "User"
                    };

                    claimsIdentity.AddClaim(new Claim(ClaimsIdentity.DefaultRoleClaimType, roleName));
                }
            }
            return Task.CompletedTask;
        }
    };

});

// =====================
// Role-based authorization using policy
// =====================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireDriver", policy => policy.RequireRole("Driver"));
    options.AddPolicy("RequirePassenger", policy => policy.RequireRole("Passenger"));
});

// =====================
// Build app
// =====================
var app = builder.Build();

// =====================
// Configure middleware
// =====================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Transportation API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
