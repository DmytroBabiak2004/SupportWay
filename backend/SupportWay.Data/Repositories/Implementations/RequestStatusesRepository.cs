// Implementations/RequestStatusRepository.cs
using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupportWay.Data.Repositories.Implementations
{
    public class RequestStatusesRepository : IRequestStatusesRepository
    {
        private readonly SupportWayContext _context;

        public RequestStatusesRepository(SupportWayContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RequestStatus>> GetAllAsync() =>
            await _context.RequestStatuses.ToListAsync();

        public async Task<RequestStatus> GetByIdAsync(int id) =>
            await _context.RequestStatuses.FindAsync(id);

        public async Task AddAsync(RequestStatus status)
        {
            _context.RequestStatuses.Add(status);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RequestStatus status)
        {
            _context.RequestStatuses.Update(status);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var status = await _context.RequestStatuses.FindAsync(id);
            if (status != null)
            {
                _context.RequestStatuses.Remove(status);
                await _context.SaveChangesAsync();
            }
        }
    }
}
