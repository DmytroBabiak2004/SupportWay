using SupportWay.Data.DTOs;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;
using SupportWay.Services.Interfaces;

namespace SupportWay.Services
{
    public class LocationService : ILocationService
    {
        private readonly ILocationsRepository _repository;

        public LocationService(ILocationsRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<LocationDto>> GetAllAsync()
        {
            var locations = await _repository.GetAllAsync();
            return locations.Select(MapToDto);
        }

        public async Task<LocationDto?> GetByIdAsync(Guid id)
        {
            var location = await _repository.GetByIdAsync(id);
            if (location == null) return null;
            return MapToDto(location);
        }

        public async Task<LocationDto> CreateAsync(CreateLocationDto dto)
        {
            var location = new Location
            {
                LocationId = Guid.NewGuid(),
                DistrictName = dto.DistrictName ?? string.Empty,
                Address = dto.Address ?? string.Empty,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude
            };

            await _repository.AddAsync(location);

            return MapToDto(location);
        }

        public async Task UpdateAsync(Guid id, CreateLocationDto dto)
        {
            var location = await _repository.GetByIdAsync(id);
            if (location != null)
            {
                location.DistrictName = dto.DistrictName ?? location.DistrictName;
                location.Address = dto.Address ?? location.Address;
                if (dto.Latitude.HasValue) location.Latitude = dto.Latitude;
                if (dto.Longitude.HasValue) location.Longitude = dto.Longitude;
                await _repository.UpdateAsync(location);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }

        private static LocationDto MapToDto(Location l) => new LocationDto
        {
            LocationId = l.LocationId,
            DistrictName = l.DistrictName,
            Address = l.Address,
            Latitude = l.Latitude,
            Longitude = l.Longitude
        };
    }
}