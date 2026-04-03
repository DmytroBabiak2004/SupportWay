using SupportWay.API.DTOs;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;
using SupportWay.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupportWay.Services.Implementations
{
    public class SupportTypeService : ISupportTypeService
    {
        private readonly ISupportTypesRepository _repository;

        public SupportTypeService(ISupportTypesRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<SupportTypeDto>> GetAllAsync()
        {
            var items = await _repository.GetAllAsync();
            return items.Select(x => new SupportTypeDto
            {
                Id = x.Id,
                NameOfType = x.NameOfType
            });
        }

        public async Task<SupportTypeDto?> GetByIdAsync(Guid id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null) return null;

            return new SupportTypeDto
            {
                Id = item.Id,
                NameOfType = item.NameOfType
            };
        }

        public async Task<SupportTypeDto> CreateAsync(CreateSupportTypeDto dto)
        {
            var entity = new SupportType
            {
                Id = Guid.NewGuid(),
                NameOfType = dto.NameOfType
            };

            await _repository.AddAsync(entity);

            return new SupportTypeDto
            {
                Id = entity.Id,
                NameOfType = entity.NameOfType
            };
        }

        public async Task UpdateAsync(Guid id, CreateSupportTypeDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity != null)
            {
                entity.NameOfType = dto.NameOfType;
                await _repository.UpdateAsync(entity);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}