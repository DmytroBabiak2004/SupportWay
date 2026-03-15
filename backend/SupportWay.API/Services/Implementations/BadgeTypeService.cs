using SupportWay.API.DTOs.BadgeTypes;
using SupportWay.API.Services.Interfaces;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.API.Services
{
    public class BadgeTypeService : IBadgeTypeService
    {
        private readonly IBadgeTypeRepository _badgeTypeRepository;

        public BadgeTypeService(IBadgeTypeRepository badgeTypeRepository)
        {
            _badgeTypeRepository = badgeTypeRepository;
        }

        public async Task<List<BadgeTypeResponse>> GetAllAsync()
        {
            var badgeTypes = await _badgeTypeRepository.GetAllAsync();

            return badgeTypes.Select(x => new BadgeTypeResponse
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
        }

        public async Task<BadgeTypeResponse?> GetByIdAsync(Guid id)
        {
            var badgeType = await _badgeTypeRepository.GetByIdAsync(id);

            if (badgeType == null)
                return null;

            return new BadgeTypeResponse
            {
                Id = badgeType.Id,
                Name = badgeType.Name
            };
        }

        public async Task<Guid> CreateAsync(CreateBadgeTypeRequest request)
        {
            var existing = await _badgeTypeRepository.GetByNameAsync(request.Name);
            if (existing != null)
                throw new Exception("Такий тип нагороди вже існує.");

            var badgeType = new BadgeType
            {
                Id = Guid.NewGuid(),
                Name = request.Name
            };

            await _badgeTypeRepository.AddAsync(badgeType);
            await _badgeTypeRepository.SaveChangesAsync();

            return badgeType.Id;
        }

        public async Task UpdateAsync(UpdateBadgeTypeRequest request)
        {
            var badgeType = await _badgeTypeRepository.GetByIdAsync(request.Id);
            if (badgeType == null)
                throw new Exception("Тип нагороди не знайдено.");

            var existingByName = await _badgeTypeRepository.GetByNameAsync(request.Name);
            if (existingByName != null && existingByName.Id != request.Id)
                throw new Exception("Такий тип нагороди вже існує.");

            badgeType.Name = request.Name;

            _badgeTypeRepository.Update(badgeType);
            await _badgeTypeRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var badgeType = await _badgeTypeRepository.GetByIdAsync(id);
            if (badgeType == null)
                throw new Exception("Тип нагороди не знайдено.");

            _badgeTypeRepository.Delete(badgeType);
            await _badgeTypeRepository.SaveChangesAsync();
        }
    }
}