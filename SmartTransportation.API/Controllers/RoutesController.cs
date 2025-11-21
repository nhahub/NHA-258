using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTransportation.BLL.DTOs.Route;
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.BLL.Services;
using SmartTransportation.DAL.Models.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class RoutesController : ControllerBase
    {
        private readonly IRouteService _routeService;

        public RoutesController(IRouteService routeService)
        {
            _routeService = routeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RouteDetailsDTO>>> GetAllRoutes()
        {
            var routes = await _routeService.GetAllRoutesAsync();
            return Ok(routes);
        }

        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<RouteDetailsDTO>>> GetPagedRoutes(
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var pagedRoutes = await _routeService.GetPagedRoutesAsync(search, pageNumber, pageSize);
            return Ok(pagedRoutes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RouteDetailsDTO>> GetRouteById(int id)
        {
            var route = await _routeService.GetRouteDetailsByIdAsync(id);
            if (route == null)
                return NotFound(new { Error = "NotFound", Message = $"Route with ID {id} not found." });

            return Ok(route);
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRoute([FromBody] CreateRouteDTO routeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Error = "BadRequest", Message = "Invalid route data provided.", Details = ModelState });

            try
            {
                var newRoute = await _routeService.CreateRouteAsync(routeDto);
                return CreatedAtAction(nameof(GetRouteById), new { id = newRoute.RouteId }, newRoute);
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, new { Error = "Forbidden", Message = "You do not have permission to create a route." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = "ValidationError", Message = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { Error = "ServerError", Message = $"An unexpected error occurred: {ex.Message}" });
            }
        }
    }
}
