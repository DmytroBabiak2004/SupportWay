using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;
using SupportWay.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace SupportWay.Services.Implementations
{
    
    public class SupportTypeService : ISupportTypeService
    {
        private readonly ISupportTypesRepository _repository;

        public SupportTypeService(ISupportTypesRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<SupportType>> GetAllAsync() =>
            _repository.GetAllAsync();

        public Task<SupportType> GetByIdAsync(Guid id) =>
            _repository.GetByIdAsync(id);

        public Task AddAsync(SupportType type) =>
            _repository.AddAsync(type);

        public Task UpdateAsync(SupportType type) =>
            _repository.UpdateAsync(type);

        public Task DeleteAsync(Guid id) =>
            _repository.DeleteAsync(id);
    }
}