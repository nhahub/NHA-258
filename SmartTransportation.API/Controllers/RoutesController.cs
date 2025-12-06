using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTransportation.BLL.DTOs.Route;
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.DAL.Models.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoutesController : ControllerBase
    {
        private readonly IRouteService _routeService;

        public RoutesController(IRouteService routeService)
        {
            _routeService = routeService;
        }

        // -------------------------------------------------------------
        // GET ALL ROUTES
        // -------------------------------------------------------------
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<RouteDetailsDTO>>> GetAllRoutes()
        {
            var routes = await _routeService.GetAllRoutesAsync();
            return Ok(routes);
        }

        // -------------------------------------------------------------
        // PAGED ROUTES
        // -------------------------------------------------------------
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<RouteDetailsDTO>>> GetPagedRoutes(
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var pagedRoutes = await _routeService.GetPagedRoutesAsync(search, pageNumber, pageSize);
            return Ok(pagedRoutes);
        }

        // -------------------------------------------------------------
        // GET ROUTE BY ID
        // -------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<RouteDetailsDTO>> GetRouteById(int id)
        {
            var route = await _routeService.GetRouteDetailsByIdAsync(id);
            if (route == null)
                return NotFound(new { Error = "NotFound", Message = $"Route with ID {id} not found." });

            return Ok(route);
        }

        // -------------------------------------------------------------
        // CREATE ROUTE
        // -------------------------------------------------------------
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRoute([FromBody] CreateRouteDTO routeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Error = "BadRequest", Message = "Invalid route data.", Details = ModelState });

            try
            {
                var newRoute = await _routeService.CreateRouteAsync(routeDto);
                return CreatedAtAction(nameof(GetRouteById), new { id = newRoute.RouteId }, newRoute);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = "ValidationError", Message = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { Error = "ServerError", Message = "Unexpected server error occurred." });
            }
        }

        // -------------------------------------------------------------
        // UPDATE ROUTE
        // -------------------------------------------------------------
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRoute(int id, [FromBody] UpdateRouteDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Error = "BadRequest", Message = "Invalid update data.", Details = ModelState });

            try
            {
                var updatedRoute = await _routeService.UpdateRouteAsync(id, dto);
                if (updatedRoute == null)
                    return NotFound(new { Error = "NotFound", Message = $"Route with ID {id} not found." });

                return Ok(updatedRoute);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = "ValidationError", Message = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { Error = "ServerError", Message = "An unexpected error occurred." });
            }
        }

        // -------------------------------------------------------------
        // DELETE ROUTE
        // -------------------------------------------------------------
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            try
            {
                var deleted = await _routeService.DeleteRouteAsync(id);
                if (!deleted)
                    return NotFound(new { Error = "NotFound", Message = $"Route with ID {id} does not exist." });

                return Ok(new { Message = $"Route {id} deleted successfully." });
            }
            catch
            {
                return StatusCode(500, new { Error = "ServerError", Message = "An unexpected error occurred." });
            }
        }

        [HttpGet("ValidateUniqueRoute")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateUniqueRoute([FromQuery] string startLocation, [FromQuery] string endLocation)
        {
            var exists = await _routeService.RouteExistsAsync(startLocation, endLocation);

            if (exists)
                return new JsonResult($"A route from '{startLocation}' to '{endLocation}' already exists.");

            return new JsonResult(true); 
        }


    }
}
