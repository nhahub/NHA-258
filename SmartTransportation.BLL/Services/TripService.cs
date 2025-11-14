// Path: SmartTransportation.BLL/Services/TripService.cs
// *** الإصدار 19: تم الإصلاح ليتوافق مع Unit of Work Pattern ***
using AutoMapper;
using SmartTransportation.BLL.DTOs;
using SmartTransportation.BLL.DTOs.Trip; 
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.BLL.Exceptions;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.UnitOfWork; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartTransportation.BLL.DTOs.Location;

namespace SmartTransportation.BLL.Services
{
    public class TripService : ITripService
    {
        private readonly IUnitOfWork _unitOfWork; 
        private readonly IRouteService _routeService;
        private readonly IMapper _mapper;

        public TripService(
            IUnitOfWork unitOfWork, 
            IRouteService routeService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _routeService = routeService;
            _mapper = mapper;
        }

        public async Task<TripDetailsDTO> CreateTripAsync(CreateTripDTO tripDto)
        {
            var routeExists = await _routeService.GetRouteDetailsByIdAsync(tripDto.RouteId);
            if (routeExists == null)
            {
                throw new BusinessException($"Route with ID {tripDto.RouteId} not found.");
            }

            var driverExists = await _unitOfWork.Users.GetByIdAsync(tripDto.DriverId);
            if (driverExists == null)
            {
                throw new BusinessException($"Driver with ID {tripDto.DriverId} not found.");
            }

            var trip = _mapper.Map<Trip>(tripDto);
            await _unitOfWork.Trips.AddAsync(trip);
            await _unitOfWork.SaveAsync(); 

            var details = await GetTripDetailsByIdAsync(trip.TripId);
            if (details == null)
            {
                throw new BusinessException("Failed to create trip");
            }
            return details;
        }

        public async Task<TripDetailsDTO?> GetTripDetailsByIdAsync(int tripId)
        {
            var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
            if (trip == null) return null;

            var tripDto = _mapper.Map<TripDetailsDTO>(trip);
            
            // Load route details
            var route = await _routeService.GetRouteDetailsByIdAsync(trip.RouteId);
            tripDto.Route = route;

            // Load trip locations
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
                if (details != null)
                {
                    tripDtos.Add(details);
                }
            }

            return tripDtos;
        }

        public async Task<IEnumerable<TripDetailsDTO>> GetAllTripsAsync()
        {
            var trips = await _unitOfWork.Trips.GetAllAsync();
            var tripDtos = new List<TripDetailsDTO>();
            
            foreach (var trip in trips)
            {
                var details = await GetTripDetailsByIdAsync(trip.TripId);
                if (details != null)
                {
                    tripDtos.Add(details);
                }
            }
            
            return tripDtos;
        }

        public async Task<TripDetailsDTO> StartTripAsync(int tripId)
        {
            var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
            if (trip == null)
            {
                throw new BusinessException("Trip not found.");
            }

            if (trip.Status != "Scheduled")
            {
                throw new BusinessException($"Trip is already {trip.Status}. Cannot start it.");
            }

            trip.Status = "Active";
            trip.StartTime = DateTime.UtcNow;
            _unitOfWork.Trips.Update(trip);
            await _unitOfWork.SaveAsync();

            var details = await GetTripDetailsByIdAsync(tripId);
            if (details == null)
            {
                throw new BusinessException("Failed to start trip");
            }
            return details;
        }

        public async Task<TripDetailsDTO> CompleteTripAsync(int tripId)
        {
            var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
            if (trip == null)
            {
                throw new BusinessException("Trip not found.");
            }

            if (trip.Status != "Active")
            {
                throw new BusinessException($"Trip status is {trip.Status}. Only active trips can be completed.");
            }

            trip.Status = "Completed";
            trip.EndTime = DateTime.UtcNow;
            _unitOfWork.Trips.Update(trip);
            await _unitOfWork.SaveAsync();

            var details = await GetTripDetailsByIdAsync(tripId);
            if (details == null)
            {
                throw new BusinessException("Failed to complete trip");
            }
            return details;
        }
    }
}