using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTransportation.BLL.DTOs.Route;
using SmartTransportation.BLL.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all endpoints
    public class RoutesController : ControllerBase
    {
        private readonly IRouteService _routeService;

        public RoutesController(IRouteService routeService)
        {
            _routeService = routeService;
        }

        // =====================
        // GET: api/Routes
        // =====================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RouteDetailsDTO>>> GetAllRoutes()
        {
            var routes = await _routeService.GetAllRoutesAsync();
            return Ok(routes);
        }

        // =====================
        // GET: api/Routes/{id}
        // =====================
        [HttpGet("{id}")]
        public async Task<ActionResult<RouteDetailsDTO>> GetRouteById(int id)
        {
            var route = await _routeService.GetRouteDetailsByIdAsync(id);
            if (route == null)
            {
                return NotFound(new
                {
                    Error = "NotFound",
                    Message = $"Route with ID {id} not found."
                });
            }

            return Ok(route);
        }

        // =====================
        // POST: api/Routes
        // =====================
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only Admin can create routes
        public async Task<IActionResult> CreateRoute([FromBody] CreateRouteDTO routeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Error = "BadRequest",
                    Message = "Invalid route data provided.",
                    Details = ModelState
                });
            }

            try
            {
                var newRoute = await _routeService.CreateRouteAsync(routeDto);
                return CreatedAtAction(
                    nameof(GetRouteById),
                    new { id = newRoute.RouteId },
                    newRoute
                );
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, new
                {
                    Error = "Forbidden",
                    Message = "You do not have permission to create a route."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    Error = "ValidationError",
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "ServerError",
                    Message = $"An unexpected error occurred: {ex.Message}"
                });
            }
        }
    }
}
