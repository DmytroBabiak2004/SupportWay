using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportWay.Data.DTOs;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // Карта публічна — авторизація не потрібна
    public class MapController : ControllerBase
    {
        private readonly IHelpRequestsRepository _repo;

        public MapController(IHelpRequestsRepository repo)
            => _repo = repo;

        /// <summary>
        /// GET /api/map?supportTypeId=...&isActive=true&region=Харків&page=1&size=200
        /// Повертає маркери для карти з фільтрацією.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<RequestMapDto>), 200)]
        public async Task<IActionResult> GetMapMarkers(
            [FromQuery] MapFilterParams filter,
            CancellationToken ct)
        {
            var (items, total) = await _repo.GetForMapAsync(filter, ct);

            var dtos = items.Select(h =>
            {
                // Пріоритет координат: пряме поле → Location
                var lat = h.Latitude ?? h.Location?.Latitude ?? 0;
                var lng = h.Longitude ?? h.Location?.Longitude ?? 0;

                // Збираємо унікальні типи зі всіх RequestItems
                var supportTypes = h.RequestItems
                    .Where(ri => ri.SupportType != null)
                    .GroupBy(ri => ri.SupportTypeId)
                    .Select(g => new SupportType
                    {
                        Id = g.First().SupportType!.Id,
                        NameOfType = g.First().SupportType!.NameOfType
                    })
                    .ToList();

                return new RequestMapDto
                {
                    Id = h.Id,
                    // Content — це тіло поста; беремо перші 80 символів як заголовок
                    Title = h.Content?[..Math.Min(80, h.Content?.Length ?? 0)] ?? "",
                    Latitude = lat,
                    Longitude = lng,
                    Region = h.Location?.DistrictName ?? "",
                    TargetAmount = h.TargetAmount,
                    CollectedAmount = h.CollectedAmount,
                    IsActive = h.IsActive,
                    CreatedAt = h.CreatedAt,
                    SupportTypes = supportTypes
                };
            }).ToList();

            return Ok(new PagedResult<RequestMapDto>
            {
                Items = dtos,
                Total = total,
                Page = filter.Page,
                Size = filter.Size
            });
        }
    }
}