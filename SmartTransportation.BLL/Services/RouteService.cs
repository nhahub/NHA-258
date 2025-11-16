using SmartTransportation.BLL.DTOs.Route;
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Models.Common;
using SmartTransportation.DAL.Repositories.Generic;
using System.Linq.Expressions;

namespace SmartTransportation.BLL.Services
{
    public class RouteService : IRouteService
    {
        private readonly IGenericRepository<Route> _routeRepo;

        public RouteService(IGenericRepository<Route> routeRepo)
        {
            _routeRepo = routeRepo;
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
            var route = new Route
            {
                RouteName = dto.RouteName,
                StartLocation = dto.StartLocation,
                EndLocation = dto.EndLocation,
                RouteType = dto.RouteType,
                IsCircular = dto.IsCircular
            };

            await _routeRepo.AddAsync(route);
            await _routeRepo.SaveAsync(); // ✅ use SaveAsync instead of _context

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
