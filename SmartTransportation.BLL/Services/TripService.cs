using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartTransportation.BLL.DTOs.Location;
using SmartTransportation.BLL.DTOs.Trip;
using SmartTransportation.BLL.Exceptions;
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Models.Common;
using SmartTransportation.DAL.Repositories.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartTransportation.BLL.Services
{
    public class TripService : ITripService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRouteService _routeService;
        private readonly IMapper _mapper;

        public TripService(IUnitOfWork unitOfWork, IRouteService routeService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _routeService = routeService;
            _mapper = mapper;
        }

        // -------------------------------
        // Create trip (driver only)
        // -------------------------------
        public async Task<TripDetailsDTO> CreateTripAsync(CreateTripDTO tripDto, int driverId)
        {
            var driver = await _unitOfWork.Users.GetByIdAsync(driverId);
            if (driver == null) throw new BusinessException($"Driver {driverId} not found.");

            var route = await _routeService.GetRouteDetailsByIdAsync(tripDto.RouteId);
            if (route == null) throw new BusinessException($"Route {tripDto.RouteId} not found.");

            var trip = _mapper.Map<Trip>(tripDto);
            trip.DriverId = driverId;
            trip.Status = "Scheduled";
            trip.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Trips.AddAsync(trip);
            await _unitOfWork.SaveAsync();

            return await GetTripDetailsByIdAsync(trip.TripId)
                   ?? throw new BusinessException("Failed to create trip");
        }

        // -------------------------------
        // Get trip details by Id
        // -------------------------------
        public async Task<TripDetailsDTO?> GetTripDetailsByIdAsync(int tripId)
        {
            var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
            if (trip == null) return null;

            var tripDto = _mapper.Map<TripDetailsDTO>(trip);

            // Route details with segments, weather, map locations
            tripDto.Route = await _routeService.GetRouteDetailsByIdAsync(trip.RouteId);

            // Trip locations
            var locations = await _unitOfWork.TripLocations.GetLocationsByTripIdAsync(tripId);
            tripDto.TripLocations = _mapper.Map<List<TripLocationDTO>>(locations);

            // Optional: driver info
            var driverProfile = await _unitOfWork.UserProfiles.GetByUserIdAsync(trip.DriverId);
            tripDto.DriverName = driverProfile?.FullName ?? "Unknown Driver";

            return tripDto;
        }

        // -------------------------------
        // Get trips by route
        // -------------------------------
        public async Task<IEnumerable<TripDetailsDTO>> GetTripsByRouteIdAsync(int routeId)
        {
            var trips = await _unitOfWork.Trips.GetTripsByRouteIdAsync(routeId);
            var tripDtos = new List<TripDetailsDTO>();

            foreach (var trip in trips)
            {
                var details = await GetTripDetailsByIdAsync(trip.TripId);
                if (details != null) tripDtos.Add(details);
            }

            return tripDtos;
        }

        // -------------------------------
        // Start a trip (driver only)
        // -------------------------------
        public async Task<TripDetailsDTO> StartTripAsync(int tripId)
        {
            var trip = await _unitOfWork.Trips.GetByIdAsync(tripId)
                       ?? throw new BusinessException("Trip not found.");

            if (trip.Status != "Scheduled")
                throw new BusinessException($"Trip already {trip.Status}, cannot start.");

            trip.Status = "Active";
            trip.StartTime = DateTime.UtcNow;

            _unitOfWork.Trips.Update(trip);
            await _unitOfWork.SaveAsync();

            return await GetTripDetailsByIdAsync(tripId)
                   ?? throw new BusinessException("Failed to start trip");
        }

        // -------------------------------
        // Complete a trip (driver only)
        // -------------------------------
        public async Task<TripDetailsDTO> CompleteTripAsync(int tripId)
        {
            var trip = await _unitOfWork.Trips.GetByIdAsync(tripId)
                       ?? throw new BusinessException("Trip not found.");

            if (trip.Status != "Active")
                throw new BusinessException($"Trip is {trip.Status}, only active trips can be completed.");

            trip.Status = "Completed";
            trip.EndTime = DateTime.UtcNow;

            _unitOfWork.Trips.Update(trip);
            await _unitOfWork.SaveAsync();

            return await GetTripDetailsByIdAsync(tripId)
                   ?? throw new BusinessException("Failed to complete trip");
        }

        // -------------------------------
        // Search trips
        // -------------------------------
        public async Task<List<TripSearchResultDto>> SearchTripsAsync(string? from, string? to, DateTime? date, int passengers)
        {
            var trips = await _unitOfWork.Trips
                .GetQueryable()
                .Include(t => t.Route)
                .Include(t => t.Driver).ThenInclude(d => d.UserProfile)
                .Include(t => t.Driver).ThenInclude(d => d.Vehicles)
                .Include(t => t.Bookings)
                .Where(t =>
                    (string.IsNullOrWhiteSpace(from) || t.Route.StartLocation.Contains(from)) &&
                    (string.IsNullOrWhiteSpace(to) || t.Route.EndLocation.Contains(to)) &&
                    (!date.HasValue || t.StartTime.Date == date.Value.Date) &&
                    (passengers <= 0 || t.AvailableSeats >= passengers)
                )
                .OrderBy(t => t.StartTime)
                .ToListAsync();

            if (!trips.Any()) return new List<TripSearchResultDto>();

            var driverIds = trips.Select(t => t.DriverId).Distinct().ToList();
            var allDriverRatings = await _unitOfWork.Ratings.GetForDriversAsync(driverIds);
            var ratingLookup = allDriverRatings.GroupBy(r => r.Trip.DriverId)
                                               .ToDictionary(g => g.Key, g => new { Average = g.Average(r => r.Score!.Value), Count = g.Count() });

            return trips.Select(t =>
            {
                var driver = t.Driver;
                var profile = driver?.UserProfile;
                var vehicle = driver?.Vehicles.FirstOrDefault(v => v.IsVerified) ?? driver?.Vehicles.FirstOrDefault();

                int maxPassengers = vehicle?.SeatsCount ?? t.AvailableSeats;

                double driverRating = 0;
                int totalReviews = 0;
                if (ratingLookup.TryGetValue(t.DriverId, out var stats))
                {
                    driverRating = stats.Average;
                    totalReviews = stats.Count;
                }

                string driverName = profile?.FullName ?? "Unknown Driver";
                string vehicleType = vehicle != null ? $"{vehicle.VehicleMake} {vehicle.VehicleModel}" : "Unknown Vehicle";

                return new TripSearchResultDto
                {
                    TripId = t.TripId,
                    FromLocation = t.Route?.StartLocation ?? "",
                    ToLocation = t.Route?.EndLocation ?? "",
                    DepartureDate = t.StartTime.Date,
                    DepartureTime = t.StartTime.ToString("HH:mm"),
                    MaxPassengers = maxPassengers,
                    AvailableSeats = t.AvailableSeats,
                    NumberOfBookings = t.Bookings?.Count() ?? 0,
                    Price = t.PricePerSeat,
                    VehicleType = vehicleType,
                    DriverName = driverName,
                    DriverRating = driverRating,
                    TotalReviews = totalReviews
                };
            }).ToList();
        }

        // -------------------------------
        // Pagination
        // -------------------------------
        public async Task<PagedResult<TripDetailsDTO>> GetPagedTripsAsync(string? search, int pageNumber, int pageSize)
        {
            var pagedTrips = await _unitOfWork.Trips.GetPagedAsync(
                t => string.IsNullOrEmpty(search) || t.Status.Contains(search),
                pageNumber,
                pageSize,
                q => q.OrderBy(t => t.StartTime)
            );

            var tripDtos = new List<TripDetailsDTO>();
            foreach (var trip in pagedTrips.Items)
            {
                var details = await GetTripDetailsByIdAsync(trip.TripId);
                if (details != null) tripDtos.Add(details);
            }

            return new PagedResult<TripDetailsDTO>
            {
                Items = tripDtos,
                TotalCount = pagedTrips.TotalCount,
                PageNumber = pagedTrips.PageNumber,
                PageSize = pagedTrips.PageSize
            };
        }
    }
}
