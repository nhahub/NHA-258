using Microsoft.AspNetCore.Mvc;
using SmartTransportation.BLL.DTOs.Route;
using SmartTransportation.BLL.Interfaces;
using System.Threading.Tasks;
using System;

namespace SmartTransportation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoutesController : ControllerBase
    {
        private readonly IRouteService _routeService;

        public RoutesController(IRouteService routeService)
        {
            _routeService = routeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoutes()
        {
            var routes = await _routeService.GetAllRoutesAsync();
            return Ok(routes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRouteById(int id)
        {
            var route = await _routeService.GetRouteDetailsByIdAsync(id);
            if (route == null) return NotFound();
            return Ok(route);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoute([FromBody] CreateRouteDTO routeDto)
        {
            try
            {
                var newRoute = await _routeService.CreateRouteAsync(routeDto);
                return CreatedAtAction(nameof(GetRouteById), new { id = newRoute.RouteId }, newRoute);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while creating the route: {ex.Message}");
            }
        }
    }
}