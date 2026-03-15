using SupportWay.Data.DTOs;

namespace SupportWay.Services.Interfaces
{
    public interface ILocationService
    {
        Task<IEnumerable<LocationDto>> GetAllAsync();
        Task<LocationDto?> GetByIdAsync(Guid id);
        Task<LocationDto> CreateAsync(CreateLocationDto dto);
        Task UpdateAsync(Guid id, CreateLocationDto dto);
        Task DeleteAsync(Guid id);
    }
}