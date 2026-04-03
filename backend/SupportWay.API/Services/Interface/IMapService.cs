using SupportWay.Data.DTOs;

namespace SupportWay.API.Services.Interface
{
    public interface IMapService
    {
        Task<PagedResult<MapMarkerDto>> GetMarkersAsync(MapFilterParams filter, CancellationToken ct = default);
    }
}