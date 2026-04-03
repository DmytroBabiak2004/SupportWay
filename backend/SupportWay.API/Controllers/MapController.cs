using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportWay.API.Services.Interface;
using SupportWay.Data.DTOs;

namespace SupportWay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class MapController : ControllerBase
    {
        private readonly IMapService _mapService;

        public MapController(IMapService mapService)
            => _mapService = mapService;

        /// <summary>
        /// GET /api/map/markers?supportTypeId=...&amp;isActive=true&amp;region=Харків&amp;search=...&amp;page=1&amp;size=200
        /// Повертає маркери для карти. Один маркер = один RequestItem.
        /// </summary>
        [HttpGet("markers")]
        [ProducesResponseType(typeof(PagedResult<MapMarkerDto>), 200)]
        public async Task<IActionResult> GetMarkers(
            [FromQuery] MapFilterParams filter,
            CancellationToken ct)
        {
            var result = await _mapService.GetMarkersAsync(filter, ct);
            return Ok(result);
        }
    }
}