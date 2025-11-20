using AutoMapper;
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

        public async Task<TripDetailsDTO> CreateTripAsync(CreateTripDTO tripDto)
        {
            var route = await _routeService.GetRouteDetailsByIdAsync(tripDto.RouteId);
            if (route == null) throw new BusinessException($"Route {tripDto.RouteId} not found.");

            var driver = await _unitOfWork.Users.GetByIdAsync(tripDto.DriverId);
            if (driver == null) throw new BusinessException($"Driver {tripDto.DriverId} not found.");

            var trip = _mapper.Map<Trip>(tripDto);
            await _unitOfWork.Trips.AddAsync(trip);
            await _unitOfWork.SaveAsync();

            return await GetTripDetailsByIdAsync(trip.TripId) ?? throw new BusinessException("Failed to create trip");
        }

        public async Task<TripDetailsDTO?> GetTripDetailsByIdAsync(int tripId)
        {
            var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
            if (trip == null) return null;

            var tripDto = _mapper.Map<TripDetailsDTO>(trip);
            tripDto.Route = await _routeService.GetRouteDetailsByIdAsync(trip.RouteId);

            var locations = await _unitOfWork.TripLocations.GetLocationsByTripIdAsync(tripId);
            tripDto.TripLocations = _mapper.Map<List<TripLocationDTO>>(locations);

            return tripDto;
        }

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

            return await GetTripDetailsByIdAsync(tripId) ?? throw new BusinessException("Failed to start trip");
        }

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

            return await GetTripDetailsByIdAsync(tripId) ?? throw new BusinessException("Failed to complete trip");
        }

        // Optional: paginated trips
        public async Task<PagedResult<TripDetailsDTO>> GetPagedTripsAsync(string? search, int pageNumber, int pageSize)
        {
            var pagedTrips = await _unitOfWork.Trips.GetPagedAsync(
                t => string.IsNullOrEmpty(search) || t.Status.Contains(search),
                pageNumber,
                pageSize,
                q => q.OrderBy(t => t.StartTime)
            );

            return new PagedResult<TripDetailsDTO>
            {
                Items = pagedTrips.Items.Select(t => _mapper.Map<TripDetailsDTO>(t)),
                TotalCount = pagedTrips.TotalCount,
                PageNumber = pagedTrips.PageNumber,
                PageSize = pagedTrips.PageSize
            };
        }

        public async Task<List<TripSearchResultDto>> SearchTripsAsync(
     string? from,
     string? to,
     DateTime? date,
     int passengers)
        {
            // 1) Load trips matching the filters
            var pagedTrips = await _unitOfWork.Trips.GetPagedAsync(
                t =>
                    (string.IsNullOrWhiteSpace(from) || t.Route.StartLocation.Contains(from)) &&
                    (string.IsNullOrWhiteSpace(to) || t.Route.EndLocation.Contains(to)) &&
                    (!date.HasValue || t.StartTime.Date == date.Value.Date) &&
                    (passengers <= 0 || t.AvailableSeats >= passengers),

                pageNumber: 1,
                pageSize: 1000,
                orderBy: q => q.OrderBy(t => t.StartTime),

                // Includes
                t => t.Bookings,
                t => t.Driver,
                t => t.Driver.UserProfile,
                t => t.Driver.Vehicles,
                t => t.Route
            );

            var trips = pagedTrips.Items;

            if (!trips.Any())
                return new List<TripSearchResultDto>();

            // ---------------------------------------------------------
            // 2) Collect all drivers from these trips
            // ---------------------------------------------------------
            var driverIds = trips
                .Select(t => t.DriverId)
                .Distinct()
                .ToList();

            // ---------------------------------------------------------
            // 3) Get ALL ratings for ALL those drivers' trips
            // ---------------------------------------------------------
            var allDriverRatings = await _unitOfWork.Ratings.GetForDriversAsync(driverIds);

            // ---------------------------------------------------------
            // 4) Group ratings by DriverId (overall driver rating)
            // ---------------------------------------------------------
            var ratingLookup = allDriverRatings
                .GroupBy(r => r.Trip.DriverId)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        Average = g.Average(r => r.Score!.Value),
                        Count = g.Count()
                    }
                );

            // ---------------------------------------------------------
            // 5) Map to DTO
            // ---------------------------------------------------------
            var result = trips.Select(t =>
            {
                var driver = t.Driver;
                var driverProfile = driver?.UserProfile;

                // Select vehicle (prefer verified)
                var vehicle =
                    driver?.Vehicles.FirstOrDefault(v => v.IsVerified)
                    ?? driver?.Vehicles.FirstOrDefault();

                // Max seats = vehicle seats if exists
                var maxPassengers = vehicle?.SeatsCount ?? t.AvailableSeats;

                // Fetch overall rating from lookup
                double driverRating = 0;
                int totalReviews = 0;

                if (ratingLookup.TryGetValue(t.DriverId, out var stats))
                {
                    driverRating = stats.Average;
                    totalReviews = stats.Count;
                }

                // Driver name safely
                var driverName = string.IsNullOrWhiteSpace(driverProfile?.FullName)
                    ? "Unknown Driver"
                    : driverProfile.FullName;

                // Vehicle type text
                var vehicleType = vehicle != null
                    ? $"{vehicle.VehicleMake} {vehicle.VehicleModel}"
                    : "Unknown Vehicle";

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

            return result;
        }

    }


}

