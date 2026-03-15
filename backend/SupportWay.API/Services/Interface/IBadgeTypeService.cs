using SupportWay.API.DTOs.BadgeTypes;


namespace SupportWay.API.Services.Interfaces
{
    public interface IBadgeTypeService
    {
        Task<List<BadgeTypeResponse>> GetAllAsync();
        Task<BadgeTypeResponse?> GetByIdAsync(Guid id);
        Task<Guid> CreateAsync(CreateBadgeTypeRequest request);
        Task UpdateAsync(UpdateBadgeTypeRequest request);
        Task DeleteAsync(Guid id);
    }
}