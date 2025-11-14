using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SmartTransportation.BLL.Gateways;
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.BLL.Services;
using SmartTransportation.BLL.Settings;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.Generic;
using SmartTransportation.DAL.Repositories.UnitOfWork;
using SmartTransportation.BLL.Utilities;
using System.Text;
using AutoMapper;

namespace SmartTransportation.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<TransportationContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Add HttpClient for gateways
            services.AddHttpClient<IGoogleMapsGateway, GoogleMapsGateway>();
            services.AddHttpClient<IOpenWeatherGateway, OpenWeatherGateway>();

            // Register other services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IRouteService, RouteService>();
            services.AddScoped<ITripService, TripService>();

            services.AddAutoMapper(typeof(MappingProfile).Assembly);

            // Add configuration
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            return services;
        }

        public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
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
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured"))
                    )
                };
            });

            return services;
        }

        // Other extension methods...
    }
}