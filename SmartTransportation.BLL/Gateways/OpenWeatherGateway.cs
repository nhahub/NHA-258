// Path: SmartTransportation.BLL/Gateways/OpenWeatherGateway.cs
// *** VERSION 16: Realistic Simulation (Location + Season + Time + Random) ***
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.DAL.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SmartTransportation.BLL.Gateways
{
    public class OpenWeatherGateway : IOpenWeatherGateway
    {
        private static readonly Random _random = new();
        private readonly IConfiguration _configuration;

        public OpenWeatherGateway(HttpClient httpClient, IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Weather> FetchWeatherDataAsync(int routeId, decimal lat, decimal lon)
        {
            await Task.Delay(50); // Simulate network delay

            // --- Dynamic Weather Calculation ---
            int month = DateTime.Now.Month;
            int hour = DateTime.Now.Hour;
            
            // 1. Determine Season
            Season season = month switch
            {
                >= 11 or <= 2 => Season.Winter,    // Nov, Dec, Jan, Feb
                >= 6 and <= 9 => Season.Summer,    // Jun, Jul, Aug, Sep
                _ => Season.SpringAutumn           // Mar, Apr, May, Oct
            };

            // 2. Determine Geographical Zone based on Latitude
            Zone zone = lat switch
            {
                > 31.0m => Zone.North,     // e.g., Alexandria, Matrouh
                > 29.0m => Zone.Central,   // e.g., Cairo, Giza, Tanta
                _ => Zone.South            // e.g., Minya, Aswan
            };

            // 3. Get Base Temp from our "Mock Database"
            decimal baseTemp = GetAverageTemp(season, zone);

            // 4. Adjust for Day/Night
            if (hour is < 6 or > 18) // 6 PM to 6 AM
            {
                baseTemp -= GetNightTimeDrop(season); // Colder at night
            }

            // 5. Add Random fluctuation (+/- 5 degrees)
            decimal randomAdjustment = (decimal)(_random.NextDouble() * 10.0) - 5.0m;
            baseTemp += randomAdjustment;

            // 6. Simulate Condition
            string condition = baseTemp switch
            {
                < 15 => "Cool (Simulated)",
                > 30 => "Hot (Simulated)",
                _ => "Clear (Simulated)"
            };

            return new Weather
            {
                RouteId = routeId,
                WeatherDate = DateOnly.FromDateTime(DateTime.Today),
                Temperature = Math.Round(baseTemp, 1),
                Condition = condition,
                WindSpeed = _random.Next(5, 25),
                Humidity = _random.Next(30, 60)
            };
        }

        private static decimal GetAverageTemp(Season season, Zone zone) => (season, zone) switch
        {
            (Season.Winter, Zone.North) => 14m,
            (Season.Winter, Zone.Central) => 15m,
            (Season.Winter, Zone.South) => 17m,
            (Season.Summer, Zone.North) => 27m,
            (Season.Summer, Zone.Central) => 30m,
            (Season.Summer, Zone.South) => 34m,
            (Season.SpringAutumn, Zone.North) => 24m,
            (Season.SpringAutumn, Zone.Central) => 27m,
            (Season.SpringAutumn, Zone.South) => 30m,
            _ => 27m
        };

        private static decimal GetNightTimeDrop(Season season) => season switch
        {
            Season.Summer => 5.0m,
            Season.Winter => 8.0m,
            _ => 7.0m
        };

        private enum Season { Winter, Summer, SpringAutumn }
        private enum Zone { North, Central, South }
    }
}