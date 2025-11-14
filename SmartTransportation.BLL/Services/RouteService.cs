// Path: SmartTransportation.BLL/Services/RouteService.cs
using AutoMapper;
using SmartTransportation.BLL.DTOs.Route;
using SmartTransportation.BLL.DTOs.Location;
using SmartTransportation.BLL.DTOs.Weather;
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartTransportation.BLL.Exceptions;

namespace SmartTransportation.BLL.Services
{
    public class RouteService : IRouteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGoogleMapsGateway _googleMapsGateway;
        private readonly IOpenWeatherGateway _openWeatherGateway;
        private readonly IMapper _mapper;

        public RouteService(
            IUnitOfWork unitOfWork,
            IGoogleMapsGateway googleMapsGateway,
            IOpenWeatherGateway openWeatherGateway,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _googleMapsGateway = googleMapsGateway;
            _openWeatherGateway = openWeatherGateway;
            _mapper = mapper;
        }

        public async Task<RouteDetailsDTO> CreateRouteAsync(CreateRouteDTO routeDto)
        {
            var route = _mapper.Map<Route>(routeDto);
            await _unitOfWork.Routes.AddAsync(route);
            await _unitOfWork.SaveAsync();

            decimal totalDistance = 0;
            int totalTime = 0;
            var allMapLocations = new List<MapLocation>();

            foreach (var segDto in routeDto.Segments)
            {
                var segmentCalc = await _googleMapsGateway.FetchRouteDetailsAsync(
                    segDto.StartPoint,
                    segDto.EndPoint,
                    route.RouteId,
                    0
                );

                var segment = _mapper.Map<RouteSegment>(segDto);
                segment.RouteId = route.RouteId;
                segment.DistanceKm = segmentCalc.TotalDistanceKm;
                segment.EstimatedMinutes = segmentCalc.EstimatedTimeMinutes;
                await _unitOfWork.RouteSegments.AddAsync(segment);
                await _unitOfWork.SaveAsync();

                // Map and save each location
                foreach (var calculatedLoc in segmentCalc.MapLocations)
                {
                    var location = new MapLocation
                    {
                        SegmentId = segment.SegmentId,
                        Latitude = calculatedLoc.Latitude,
                        Longitude = calculatedLoc.Longitude,
                        Description = calculatedLoc.Description,
                        StopOrder = calculatedLoc.StopOrder,
                        GooglePlaceId = calculatedLoc.GooglePlaceId,
                        GoogleAddress = calculatedLoc.GoogleAddress
                    };

                    await _unitOfWork.MapLocations.AddAsync(location);
                    allMapLocations.Add(location);
                }

                totalDistance += segmentCalc.TotalDistanceKm;
                totalTime += segmentCalc.EstimatedTimeMinutes;
            }

            route.TotalDistanceKm = totalDistance;
            route.EstimatedTimeMinutes = totalTime;
            _unitOfWork.Routes.Update(route);

            // Get the end location for weather
            if (allMapLocations.Any())
            {
                var endLoc = allMapLocations[^1];
                if (endLoc.Latitude.HasValue && endLoc.Longitude.HasValue)
                {
                    var weather = await _openWeatherGateway.FetchWeatherDataAsync(
                        route.RouteId,
                        endLoc.Latitude.Value,
                        endLoc.Longitude.Value
                    );
                    await _unitOfWork.Weathers.AddAsync(weather);
                }
            }

            await _unitOfWork.SaveAsync();

            var routeDetails = await GetRouteDetailsByIdAsync(route.RouteId);
            return routeDetails ?? throw new BusinessException("Failed to create route");
        }

        public async Task<RouteDetailsDTO?> GetRouteDetailsByIdAsync(int routeId)
        {
            var route = await _unitOfWork.Routes.GetByIdAsync(routeId);
            if (route == null) return null;

            var segments = await _unitOfWork.RouteSegments.FindAsync(s => s.RouteId == routeId);
            var locations = await _unitOfWork.MapLocations.FindAsync(ml => ml.Segment.RouteId == routeId);
            var weather = (await _unitOfWork.Weathers.FindAsync(w => w.RouteId == routeId))
                                .OrderByDescending(w => w.WeatherDate)
                                .FirstOrDefault();

            var routeDto = _mapper.Map<RouteDetailsDTO>(route);
            routeDto.Segments = _mapper.Map<List<RouteSegmentDTO>>(segments);
            routeDto.MapLocations = _mapper.Map<List<MapLocationDTO>>(locations);
            routeDto.LatestWeather = weather != null ? _mapper.Map<WeatherDTO>(weather) : null;
            return routeDto;
        }

        public async Task<IEnumerable<RouteDetailsDTO>> GetAllRoutesAsync()
        {
            var routes = await _unitOfWork.Routes.GetAllAsync();
            var detailedRoutes = new List<RouteDetailsDTO>();
            
            foreach (var route in routes)
            {
                var details = await GetRouteDetailsByIdAsync(route.RouteId);
                if (details != null)
                {
                    detailedRoutes.Add(details);
                }
            }
            
            return detailedRoutes;
        }
    }
}