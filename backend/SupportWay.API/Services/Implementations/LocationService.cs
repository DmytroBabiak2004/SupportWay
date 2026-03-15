using SupportWay.Data.DTOs;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;
using SupportWay.Services.Interfaces;

namespace SupportWay.Services
{
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository _repository;

        public LocationService(ILocationRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<LocationDto>> GetAllAsync()
        {
            var locations = await _repository.GetAllAsync();

            // Мапінг Entity -> DTO
            return locations.Select(l => new LocationDto
            {
                LocationId = l.LocationId,
                DistrictName = l.DistrictName
            });
        }

        public async Task<LocationDto?> GetByIdAsync(Guid id)
        {
            var location = await _repository.GetByIdAsync(id);
            if (location == null) return null;

            return new LocationDto
            {
                LocationId = location.LocationId,
                DistrictName = location.DistrictName
            };
        }

        public async Task<LocationDto> CreateAsync(CreateLocationDto dto)
        {
            var location = new Location
            {
                LocationId = Guid.NewGuid(),
                DistrictName = dto.DistrictName
            };

            await _repository.AddAsync(location);

            return new LocationDto
            {
                LocationId = location.LocationId,
                DistrictName = location.DistrictName
            };
        }

        public async Task UpdateAsync(Guid id, CreateLocationDto dto)
        {
            var location = await _repository.GetByIdAsync(id);
            if (location != null)
            {
                location.DistrictName = dto.DistrictName;
                await _repository.UpdateAsync(location);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}