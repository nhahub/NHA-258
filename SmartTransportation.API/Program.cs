using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartTransportation.API.Extensions;
using SmartTransportation.BLL.DTOs.Notification;
using SmartTransportation.BLL.Jobs;
using SmartTransportation.BLL.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// injecting payment service
builder.Services.AddScoped<StripePaymentService>();
builder.Services.AddHostedService<PaymentStatusBackgroundService>();

// Add HttpClient
builder.Services.AddHttpClient();

// Add custom services
builder.Services.AddApplicationServices(builder.Configuration);

// Add CORS policy
builder.Services.AddCorsPolicy();

// Add JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Transportation API V1");
        c.RoutePrefix = string.Empty; // This will serve the Swagger UI at the root URL
    });
}

// Use CORS
app.UseCors("AllowAll");

app.UseHttpsRedirection();

// Add authentication & authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
