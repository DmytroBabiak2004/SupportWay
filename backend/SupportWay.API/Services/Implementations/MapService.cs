using SupportWay.API.Services.Interface;
using SupportWay.Data.DTOs;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.API.Services.Implementations
{
    public class MapService : IMapService
    {
        private readonly IHelpRequestsRepository _repo;

        public MapService(IHelpRequestsRepository repo)
            => _repo = repo;

        public async Task<PagedResult<MapMarkerDto>> GetMarkersAsync(
            MapFilterParams filter,
            CancellationToken ct = default)
        {
            var (items, total) = await _repo.GetMapMarkersAsync(filter, ct);

            return new PagedResult<MapMarkerDto>
            {
                Items = items,
                Total = total,
                Page = filter.Page,
                Size = filter.Size
            };
        }
    }
}