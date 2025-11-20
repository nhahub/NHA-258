using SmartTransportation.BLL.DTOs.Route;
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.DAL.Models;
using SmartTransportation.BLL.DTOs;
using SmartTransportation.DAL.Models.Common;
using SmartTransportation.DAL.Repositories.Generic;
using SmartTransportation.DAL.Repositories.UnitOfWork;
using System.Linq.Expressions;

namespace SmartTransportation.BLL.Services
{
    public class RouteService : IRouteService
    {
        private readonly IGenericRepository<Route> _routeRepo;
        private readonly IUnitOfWork _unitOfWork;

        public RouteService(
     IGenericRepository<Route> routeRepo,
     IUnitOfWork unitOfWork)
        {
            _routeRepo = routeRepo;
            _unitOfWork = unitOfWork;
        }
        public async Task<IEnumerable<RouteDetailsDTO>> GetAllRoutesAsync()
        {
            var routes = await _routeRepo.GetAllAsync();
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
            var route = await _routeRepo.GetByIdAsync(routeId);
            if (route == null) return null;

            return new RouteDetailsDTO
            {
                RouteId = route.RouteId,
                RouteName = route.RouteName,
                StartLocation = route.StartLocation,
                EndLocation = route.EndLocation,
                RouteType = route.RouteType,
                IsCircular = route.IsCircular,
                CreatedAt = route.CreatedAt
            };
        }
        public async Task<RouteDetailsDTO> CreateRouteAsync(CreateRouteDTO dto)
        {
            // 1️⃣ Calculate totals
            decimal totalDistance = dto.Segments.Sum(s => s.SegmentDistanceKm ?? 0);
            int totalMinutes = dto.Segments.Sum(s => s.SegmentEstimatedMinutes ?? 0);

            // 2️⃣ Create Route
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

            // 3️⃣ Add Route
            await _routeRepo.AddAsync(route);
            await _unitOfWork.SaveAsync(); // save to get route.RouteId

            // 4️⃣ Add segments using UnitOfWork repository
            foreach (var seg in dto.Segments)
            {
                var segment = new RouteSegment
                {
                    RouteId = route.RouteId,
                    SegmentOrder = seg.SegmentOrder,
                    StartPoint = seg.StartPoint,
                    EndPoint = seg.EndPoint,
                    DistanceKm = seg.SegmentDistanceKm,          // map to EF property
                    EstimatedMinutes = seg.SegmentEstimatedMinutes // map to EF property
                };

                await _unitOfWork.RouteSegments.AddAsync(segment);
            }

            await _unitOfWork.SaveAsync();


            await _unitOfWork.SaveAsync(); // save all segments

            // 5️⃣ Return DTO
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
                Segments = dto.Segments.Select(s => new RouteSegmentDTO
                {
                    SegmentOrder = s.SegmentOrder,
                    StartPoint = s.StartPoint,
                    EndPoint = s.EndPoint,
                    SegmentDistanceKm = s.SegmentDistanceKm,
                    SegmentEstimatedMinutes = s.SegmentEstimatedMinutes
                }).ToList()
            };
        }


        public async Task<PagedResult<RouteDetailsDTO>> GetPagedRoutesAsync(
            string? search,
            int pageNumber,
            int pageSize)
        {
            Expression<Func<Route, bool>> filter = r =>
                string.IsNullOrEmpty(search) || r.RouteName.Contains(search);

            var pagedRoutes = await _routeRepo.GetPagedAsync(filter, pageNumber, pageSize, q => q.OrderBy(r => r.RouteName));

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
