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
    }
}
