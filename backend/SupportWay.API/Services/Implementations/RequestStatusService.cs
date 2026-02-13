using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;
using SupportWay.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace SupportWay.Services.Implementations
{
    public class RequestStatusService : IRequestStatusService
    {
        private readonly IRequestStatusesRepository _repository;

        public RequestStatusService(IRequestStatusesRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<RequestStatus>> GetAllAsync() =>
            _repository.GetAllAsync();

        public Task<RequestStatus> GetByIdAsync(Guid id) =>
            _repository.GetByIdAsync(id);

        public Task AddAsync(RequestStatus status) =>
            _repository.AddAsync(status);

        public Task UpdateAsync(RequestStatus status) =>
            _repository.UpdateAsync(status);

        public Task DeleteAsync(Guid id) =>
            _repository.DeleteAsync(id);
    }
}