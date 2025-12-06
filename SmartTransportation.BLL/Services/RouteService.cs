using Microsoft.EntityFrameworkCore;
using SmartTransportation.BLL.DTOs.Location;
using SmartTransportation.BLL.DTOs.Route;
using SmartTransportation.BLL.DTOs.Weather;
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Models.Common;
using SmartTransportation.DAL.Repositories.UnitOfWork;
using System.Linq.Expressions;

namespace SmartTransportation.BLL.Services
{
    public class RouteService : IRouteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGoogleMapsGateway _googleMaps;
        private readonly IOpenWeatherGateway _weatherGateway;

        public RouteService(
            IUnitOfWork unitOfWork,
            IGoogleMapsGateway googleMaps,
            IOpenWeatherGateway weatherGateway)
        {
            _unitOfWork = unitOfWork;
            _googleMaps = googleMaps;
            _weatherGateway = weatherGateway;
        }

        public async Task<IEnumerable<RouteDetailsDTO>> GetAllRoutesAsync()
        {
            var routes = await _unitOfWork.Routes.GetAllAsync();
            var allSegments = await _unitOfWork.RouteSegments.GetAllAsync();
            var allLocations = await _unitOfWork.MapLocations.GetAllAsync();

            return routes.Select(r => new RouteDetailsDTO
            {
                RouteId = r.RouteId,
                RouteName = r.RouteName,
                StartLocation = r.StartLocation,
                EndLocation = r.EndLocation,
                RouteType = r.RouteType,
                IsCircular = r.IsCircular,
                TotalDistanceKm = r.TotalDistanceKm,
                EstimatedTimeMinutes = r.EstimatedTimeMinutes,
                CreatedAt = r.CreatedAt,
                Segments = allSegments
                    .Where(s => s.RouteId == r.RouteId)
                    .Select(s => new RouteSegmentDTO
                    {
                        SegmentId = s.SegmentId,
                        RouteId = s.RouteId,
                        SegmentOrder = s.SegmentOrder,
                        StartPoint = s.StartPoint,
                        EndPoint = s.EndPoint,
                        DistanceKm = s.DistanceKm,
                        EstimatedMinutes = s.EstimatedMinutes
                    }).ToList(),
                MapLocations = allLocations
                    .Where(l => allSegments.Any(s => s.SegmentId == l.SegmentId && s.RouteId == r.RouteId))
                    .Select(l => new MapLocationDTO
                    {
                        LocationId = l.LocationId,
                        SegmentId = l.SegmentId,
                        Latitude = l.Latitude,
                        Longitude = l.Longitude,
                        Description = l.Description,
                        StopOrder = l.StopOrder,
                        GoogleAddress = l.GoogleAddress,
                        GooglePlaceId = l.GooglePlaceId
                    }).ToList()
            });
        }

        public async Task<RouteDetailsDTO?> GetRouteDetailsByIdAsync(int routeId)
        {
            var route = await _unitOfWork.Routes.GetByIdAsync(routeId);
            if (route == null) return null;

            var segments = await _unitOfWork.RouteSegments.FindAsync(s => s.RouteId == routeId);
            var segmentIds = segments.Select(s => s.SegmentId).ToList();
            var locations = await _unitOfWork.MapLocations.FindAsync(l => segmentIds.Contains(l.SegmentId));

            Weather latestWeather = null;
            var firstLoc = locations.FirstOrDefault();
            if (firstLoc != null)
            {
                latestWeather = await _weatherGateway.FetchWeatherDataAsync(route.RouteId, firstLoc.Latitude ?? 0, firstLoc.Longitude ?? 0);
                await _unitOfWork.Weathers.AddAsync(latestWeather);
                await _unitOfWork.SaveAsync();
            }

            return new RouteDetailsDTO
            {
                RouteId = route.RouteId,
                RouteName = route.RouteName,
                StartLocation = route.StartLocation,
                EndLocation = route.EndLocation,
                RouteType = route.RouteType,
                IsCircular = route.IsCircular,
                TotalDistanceKm = route.TotalDistanceKm,
                EstimatedTimeMinutes = route.EstimatedTimeMinutes,
                CreatedAt = route.CreatedAt,
                Segments = segments.Select(s => new RouteSegmentDTO
                {
                    SegmentId = s.SegmentId,
                    RouteId = s.RouteId,
                    SegmentOrder = s.SegmentOrder,
                    StartPoint = s.StartPoint,
                    EndPoint = s.EndPoint,
                    DistanceKm = s.DistanceKm,
                    EstimatedMinutes = s.EstimatedMinutes
                }).ToList(),
                MapLocations = locations.Select(l => new MapLocationDTO
                {
                    LocationId = l.LocationId,
                    SegmentId = l.SegmentId,
                    Latitude = l.Latitude,
                    Longitude = l.Longitude,
                    Description = l.Description,
                    StopOrder = l.StopOrder,
                    GoogleAddress = l.GoogleAddress,
                    GooglePlaceId = l.GooglePlaceId
                }).ToList(),
                LatestWeather = latestWeather != null
                    ? new WeatherDTO
                    {
                        WeatherId = latestWeather.WeatherId,
                        RouteId = latestWeather.RouteId,
                        WeatherDate = latestWeather.WeatherDate,
                        Temperature = latestWeather.Temperature,
                        Condition = latestWeather.Condition,
                        WindSpeed = latestWeather.WindSpeed,
                        Humidity = latestWeather.Humidity
                    }
                    : null
            };
        }

        public async Task<RouteDetailsDTO> CreateRouteAsync(CreateRouteDTO dto)
        {
            decimal totalDistance = dto.Segments.Sum(s => s.SegmentDistanceKm ?? 0);
            int totalMinutes = dto.Segments.Sum(s => s.SegmentEstimatedMinutes ?? 0);

            var route = new Route
            {
                RouteName = dto.RouteName,
                StartLocation = dto.StartLocation,
                EndLocation = dto.EndLocation,
                RouteType = dto.RouteType,
                IsCircular = dto.IsCircular,
                TotalDistanceKm = totalDistance,
                EstimatedTimeMinutes = totalMinutes,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Routes.AddAsync(route);
            await _unitOfWork.SaveAsync();

            var allSegments = new List<RouteSegment>();
            var allLocations = new List<MapLocation>();

            foreach (var seg in dto.Segments)
            {
                var segment = new RouteSegment
                {
                    RouteId = route.RouteId,
                    SegmentOrder = seg.SegmentOrder,
                    StartPoint = seg.StartPoint,
                    EndPoint = seg.EndPoint,
                    DistanceKm = seg.SegmentDistanceKm ?? 0,
                    EstimatedMinutes = seg.SegmentEstimatedMinutes ?? 0
                };
                allSegments.Add(segment);
            }

            await _unitOfWork.RouteSegments.AddRangeAsync(allSegments);
            await _unitOfWork.SaveAsync();

            // Fetch MapLocations for all segments
            foreach (var seg in allSegments)
            {
                var maps = await _googleMaps.FetchRouteDetailsAsync(seg.StartPoint, seg.EndPoint, route.RouteId, seg.SegmentId);
                allLocations.AddRange(maps.MapLocations);
            }

            await _unitOfWork.MapLocations.AddRangeAsync(allLocations);
            await _unitOfWork.SaveAsync();

            // Fetch weather for first location if exists
            Weather latestWeather = null;
            var firstLoc = allLocations.FirstOrDefault();
            if (firstLoc != null)
            {
                latestWeather = await _weatherGateway.FetchWeatherDataAsync(route.RouteId, firstLoc.Latitude ?? 0, firstLoc.Longitude ?? 0);
                await _unitOfWork.Weathers.AddAsync(latestWeather);
                await _unitOfWork.SaveAsync();
            }

            // Return DTO
            var segmentsDTO = allSegments.Select(s => new RouteSegmentDTO
            {
                SegmentId = s.SegmentId,
                RouteId = s.RouteId,
                SegmentOrder = s.SegmentOrder,
                StartPoint = s.StartPoint,
                EndPoint = s.EndPoint,
                DistanceKm = s.DistanceKm,
                EstimatedMinutes = s.EstimatedMinutes
            }).ToList();

            var mapLocationsDTO = allLocations.Select(l => new MapLocationDTO
            {
                LocationId = l.LocationId,
                SegmentId = l.SegmentId,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                Description = l.Description,
                StopOrder = l.StopOrder,
                GoogleAddress = l.GoogleAddress,
                GooglePlaceId = l.GooglePlaceId
            }).ToList();

            return new RouteDetailsDTO
            {
                RouteId = route.RouteId,
                RouteName = route.RouteName,
                StartLocation = route.StartLocation,
                EndLocation = route.EndLocation,
                RouteType = route.RouteType,
                IsCircular = route.IsCircular,
                TotalDistanceKm = route.TotalDistanceKm,
                EstimatedTimeMinutes = route.EstimatedTimeMinutes,
                CreatedAt = route.CreatedAt,
                Segments = segmentsDTO,
                MapLocations = mapLocationsDTO,
                LatestWeather = latestWeather != null ? new WeatherDTO
                {
                    WeatherId = latestWeather.WeatherId,
                    RouteId = latestWeather.RouteId,
                    WeatherDate = latestWeather.WeatherDate,
                    Temperature = latestWeather.Temperature,
                    Condition = latestWeather.Condition,
                    WindSpeed = latestWeather.WindSpeed,
                    Humidity = latestWeather.Humidity
                } : null
            };
        }



        public async Task<RouteDetailsDTO?> UpdateRouteAsync(int routeId, UpdateRouteDTO dto)
        {
            var route = await _unitOfWork.Routes.GetByIdAsync(routeId);
            if (route == null) return null;

            route.RouteName = dto.RouteName;
            route.StartLocation = dto.StartLocation;
            route.EndLocation = dto.EndLocation;
            route.RouteType = dto.RouteType;
            route.IsCircular = dto.IsCircular;

            var oldSegments = await _unitOfWork.RouteSegments.FindAsync(s => s.RouteId == routeId);

            foreach (var seg in oldSegments)
            {
                var locs = await _unitOfWork.MapLocations.FindAsync(l => l.SegmentId == seg.SegmentId);
                _unitOfWork.MapLocations.RemoveRange(locs);
            }

            _unitOfWork.RouteSegments.RemoveRange(oldSegments);

            decimal totalDistance = 0;
            int totalMinutes = 0;

            foreach (var s in dto.Segments)
            {
                var segment = new RouteSegment
                {
                    RouteId = route.RouteId,
                    SegmentOrder = s.SegmentOrder,
                    StartPoint = s.StartPoint,
                    EndPoint = s.EndPoint,
                    DistanceKm = s.SegmentDistanceKm,
                    EstimatedMinutes = s.SegmentEstimatedMinutes
                };

                await _unitOfWork.RouteSegments.AddAsync(segment);
                await _unitOfWork.SaveAsync();

                totalDistance += segment.DistanceKm ?? 0;
                totalMinutes += segment.EstimatedMinutes ?? 0;

                var maps = await _googleMaps.FetchRouteDetailsAsync(segment.StartPoint, segment.EndPoint, route.RouteId, segment.SegmentId);

                foreach (var loc in maps.MapLocations)
                {
                    await _unitOfWork.MapLocations.AddAsync(loc);
                }

                await _unitOfWork.SaveAsync();
            }

            route.TotalDistanceKm = totalDistance;
            route.EstimatedTimeMinutes = totalMinutes;

            _unitOfWork.Routes.Update(route);
            await _unitOfWork.SaveAsync();

            return await GetRouteDetailsByIdAsync(routeId);
        }


        public async Task<bool> DeleteRouteAsync(int routeId)
        {
            var route = await _unitOfWork.Routes.GetByIdAsync(routeId);
            if (route == null) return false;

            var segments = await _unitOfWork.RouteSegments.FindAsync(s => s.RouteId == routeId);

            foreach (var seg in segments)
            {
                var locs = await _unitOfWork.MapLocations.FindAsync(l => l.SegmentId == seg.SegmentId);
                _unitOfWork.MapLocations.RemoveRange(locs);
            }

            _unitOfWork.RouteSegments.RemoveRange(segments);

            var weather = await _unitOfWork.Weathers.FindAsync(w => w.RouteId == routeId);
            _unitOfWork.Weathers.RemoveRange(weather);

            _unitOfWork.Routes.Remove(route);
            await _unitOfWork.SaveAsync();

            return true;
        }
        public async Task<bool> RouteExistsAsync(string start, string end)
        {
            return await _unitOfWork.Routes
                .GetQueryable()
                .AnyAsync(r => r.StartLocation == start && r.EndLocation == end);
        }


        public async Task<PagedResult<RouteDetailsDTO>> GetPagedRoutesAsync(string? search, int pageNumber, int pageSize)
        {
            Expression<Func<Route, bool>> filter = r =>
                string.IsNullOrEmpty(search) || r.RouteName.Contains(search);

            var pagedRoutes = await _unitOfWork.Routes.GetPagedAsync(
                filter,
                pageNumber,
                pageSize,
                q => q.OrderBy(r => r.RouteName));

            return new PagedResult<RouteDetailsDTO>
            {
                Items = pagedRoutes.Items.Select(r => new RouteDetailsDTO
                {
                    RouteId = r.RouteId,
                    RouteName = r.RouteName,
                    StartLocation = r.StartLocation,
                    EndLocation = r.EndLocation,
                    RouteType = r.RouteType,
                    IsCircular = r.IsCircular,
                    CreatedAt = r.CreatedAt
                }).ToList(),
                TotalCount = pagedRoutes.TotalCount,
                PageNumber = pagedRoutes.PageNumber,
                PageSize = pagedRoutes.PageSize
            };
        }
    }
}
