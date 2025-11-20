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
            return routes.Select(r => new RouteDetailsDTO
            {
                RouteId = r.RouteId,
                RouteName = r.RouteName,
                StartLocation = r.StartLocation,
                EndLocation = r.EndLocation,
                RouteType = r.RouteType,
                IsCircular = r.IsCircular,
                CreatedAt = r.CreatedAt
            });
        }

        public async Task<RouteDetailsDTO?> GetRouteDetailsByIdAsync(int routeId)
        {
            var route = await _unitOfWork.Routes.GetByIdAsync(routeId);
            if (route == null) return null;

            var segments = await _unitOfWork.RouteSegments.FindAsync(s => s.RouteId == routeId);
            var segmentIds = segments.Select(s => s.SegmentId).ToList();
            var locations = await _unitOfWork.MapLocations.FindAsync(l => segmentIds.Contains(l.SegmentId));

            // Fetch simulated weather
            Weather latestWeather = null;
            var firstLoc = locations.FirstOrDefault();
            if (firstLoc != null)
            {
                latestWeather = await _weatherGateway.FetchWeatherDataAsync(route.RouteId, firstLoc.Latitude.Value, firstLoc.Longitude.Value);

                // Save weather to database so it gets a proper auto-incremented WeatherId
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
                LatestWeather = latestWeather != null ? new WeatherDTO
                {
                    WeatherId = latestWeather.WeatherId, // Now auto-incremented
                    RouteId = latestWeather.RouteId,
                    WeatherDate = latestWeather.WeatherDate,
                    Temperature = latestWeather.Temperature,
                    Condition = latestWeather.Condition,
                    WindSpeed = latestWeather.WindSpeed,
                    Humidity = latestWeather.Humidity
                } : null
            };
        }

        public async Task<RouteDetailsDTO> CreateRouteAsync(CreateRouteDTO dto)
        {
            // 1️⃣ Calculate totals
            decimal totalDistance = dto.Segments.Sum(s => s.SegmentDistanceKm ?? 0);
            int totalMinutes = dto.Segments.Sum(s => s.SegmentEstimatedMinutes ?? 0);

            // 2️⃣ Create route
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
            await _unitOfWork.SaveAsync(); // RouteId generated

            var allMapLocations = new List<MapLocation>();
            Weather latestWeather = null;

            foreach (var seg in dto.Segments)
            {
                // 3️⃣ Create segment
                var segment = new RouteSegment
                {
                    RouteId = route.RouteId,
                    SegmentOrder = seg.SegmentOrder,
                    StartPoint = seg.StartPoint,
                    EndPoint = seg.EndPoint,
                    DistanceKm = seg.SegmentDistanceKm,
                    EstimatedMinutes = seg.SegmentEstimatedMinutes
                };
                await _unitOfWork.RouteSegments.AddAsync(segment);
                await _unitOfWork.SaveAsync(); // SegmentId generated

                // 4️⃣ Fetch Map Locations from Google Maps
                var routeCalc = await _googleMaps.FetchRouteDetailsAsync(
                    seg.StartPoint, seg.EndPoint, route.RouteId, segment.SegmentId);

                foreach (var loc in routeCalc.MapLocations)
                {
                    await _unitOfWork.MapLocations.AddAsync(loc);
                }
                await _unitOfWork.SaveAsync(); // LocationId generated
                allMapLocations.AddRange(routeCalc.MapLocations);

                // 5️⃣ Fetch weather for first segment only
                if (latestWeather == null && routeCalc.MapLocations.Any())
                {
                    var firstLoc = routeCalc.MapLocations.First();
                    latestWeather = await _weatherGateway.FetchWeatherDataAsync(
                        route.RouteId, firstLoc.Latitude.Value, firstLoc.Longitude.Value);

                    // Save weather to DB to get auto-incremented WeatherId
                    await _unitOfWork.Weathers.AddAsync(latestWeather);
                    await _unitOfWork.SaveAsync();
                }
            }

            // 6️⃣ Prepare segments DTO
            var segmentsDTO = await _unitOfWork.RouteSegments.FindAsync(s => s.RouteId == route.RouteId);

            // 7️⃣ Return final RouteDetailsDTO
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
                Segments = segmentsDTO.Select(s => new RouteSegmentDTO
                {
                    SegmentId = s.SegmentId,
                    RouteId = s.RouteId,
                    SegmentOrder = s.SegmentOrder,
                    StartPoint = s.StartPoint,
                    EndPoint = s.EndPoint,
                    DistanceKm = s.DistanceKm,
                    EstimatedMinutes = s.EstimatedMinutes
                }).ToList(),
                MapLocations = allMapLocations.Select(l => new MapLocationDTO
                {
                    LocationId = l.LocationId, // now guaranteed non-zero
                    SegmentId = l.SegmentId,
                    Latitude = l.Latitude,
                    Longitude = l.Longitude,
                    Description = l.Description,
                    StopOrder = l.StopOrder,
                    GoogleAddress = l.GoogleAddress,
                    GooglePlaceId = l.GooglePlaceId
                }).ToList(),
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



        public async Task<PagedResult<RouteDetailsDTO>> GetPagedRoutesAsync(string? search, int pageNumber, int pageSize)
        {
            Expression<Func<Route, bool>> filter = r =>
                string.IsNullOrEmpty(search) || r.RouteName.Contains(search);

            var pagedRoutes = await _unitOfWork.Routes.GetPagedAsync(filter, pageNumber, pageSize, q => q.OrderBy(r => r.RouteName));

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
